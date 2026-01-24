using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Logging;
using LillyQuest.Engine.Managers.Screens.Base;
using Serilog.Events;

namespace LillyQuest.Engine.Screens.Logging;

public sealed class RenderedLine
{
    public IReadOnlyList<StyledSpan> Spans { get; set; }
    public float BlinkRemaining { get; set; }

    public RenderedLine(IReadOnlyList<StyledSpan> spans, float blinkRemaining)
    {
        Spans = spans;
        BlinkRemaining = blinkRemaining;
    }
}

public class LogScreen : BaseScreen
{
    public static readonly LyColor TraceColor = LyColor.DimGray;
    public static readonly LyColor DebugColor = LyColor.Gray;
    public static readonly LyColor InfoColor = LyColor.White;
    public static readonly LyColor WarningColor = LyColor.Yellow;
    public static readonly LyColor ErrorColor = LyColor.OrangeRed;
    public static readonly LyColor FatalColor = LyColor.Red;

    private readonly ILogEventDispatcher _dispatcher;
    private readonly IFontManager _fontManager;
    private readonly BBCodeParser _bbcodeParser = new();
    private readonly TypewriterQueue _typewriterQueue;
    private readonly List<RenderedLine> _lines = [];
    private RenderedLine? _currentLine;
    private float _currentLineBlinkRemaining;
    private int _currentLineSequence;
    private int _consumedCompletedLines;

    private float _blinkElapsed;
    private float _cursorBlinkElapsed;

    public string FontName { get; set; } = "default_font_log";
    public int FontSize { get; set; } = 14;
    public LyColor BackgroundColor { get; set; } = LyColor.Black;
    public float BackgroundAlpha { get; set; } = 0.6f;
    public int DispatchBatchSize { get; set; } = 64;
    public float FatalBlinkDuration { get; set; } = 1f;
    public float FatalBlinkFrequency { get; set; } = 6f;
    public float TypewriterSpeed { get; set; } = 120f;
    public float CursorBlinkFrequency { get; set; } = 4f;
    public LyColor CursorColor { get; set; } = LyColor.White;

    public LogScreen(ILogEventDispatcher dispatcher, IFontManager fontManager)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _fontManager = fontManager ?? throw new ArgumentNullException(nameof(fontManager));
        _typewriterQueue = new(TypewriterSpeed);
        Margin = new(10, 10, 10, 10);
        _dispatcher.OnLogEntries += HandleLogEntries;
    }

    public LyColor GetBackgroundColorWithAlpha()
    {
        var alpha = (byte)Math.Clamp(MathF.Round(BackgroundColor.A * BackgroundAlpha), 0f, 255f);

        return BackgroundColor.WithAlpha(alpha);
    }

    public override void OnUnload()
    {
        _dispatcher.OnLogEntries -= HandleLogEntries;
        base.OnUnload();
    }

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        spriteBatch.PushTranslation(Position);

        spriteBatch.DrawRectangle(Vector2.Zero, Size, GetBackgroundColorWithAlpha());

        var lineHeight = GetLineHeight();
        var y = Size.Y - Margin.W - lineHeight;
        var x = Margin.X;
        var cursorPosition = (Vector2?)null;
        var cursorHeight = lineHeight;

        var totalLines = _lines.Count + (_currentLine != null ? 1 : 0);
        for (var i = totalLines - 1; i >= 0 && y >= Margin.Y; i--)
        {
            var line = ResolveRenderLine(i, totalLines);
            var lineVisible = ResolveLineVisibility(line);
            if (lineVisible)
            {
                DrawStyledLine(spriteBatch, line, new Vector2(x, y));
            }

            if (_currentLine != null && ReferenceEquals(line, _currentLine))
            {
                var cursorX = x + MeasureWidth(_typewriterQueue.CurrentLineText);
                cursorPosition = new Vector2(cursorX, y);
            }

            y -= lineHeight;
        }

        if (cursorPosition.HasValue && IsCursorVisible())
        {
            var cursorWidth = MathF.Max(2f, MeasureWidth("W") * 0.6f);
            spriteBatch.DrawRectangle(cursorPosition.Value, new Vector2(cursorWidth, cursorHeight), CursorColor);
        }

        spriteBatch.PopTranslation();
    }

    public override void Update(GameTime gameTime)
    {
        _dispatcher.Dispatch(DispatchBatchSize);
        var deltaSeconds = (float)gameTime.Elapsed.TotalSeconds;
        _blinkElapsed += deltaSeconds;
        _cursorBlinkElapsed += deltaSeconds;

        _typewriterQueue.CharactersPerSecond = TypewriterSpeed;
        _typewriterQueue.Update(gameTime.Elapsed);
        ConsumeCompletedLines();
        UpdateCurrentLine();
        TrimToMaxLines();

        foreach (var line in _lines)
        {
            if (line.BlinkRemaining <= 0f)
            {
                continue;
            }

            line.BlinkRemaining = Math.Max(0f, line.BlinkRemaining - deltaSeconds);
        }

        if (_currentLine != null && _currentLineBlinkRemaining > 0f)
        {
            _currentLineBlinkRemaining = Math.Max(0f, _currentLineBlinkRemaining - deltaSeconds);
            _currentLine.BlinkRemaining = _currentLineBlinkRemaining;
        }
    }

    protected IReadOnlyList<RenderedLine> GetLines()
        => _lines.AsReadOnly();

    private void HandleLogEntries(IReadOnlyList<LogEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            AddEntry(entry);
        }
    }

    private void AddEntry(LogEntry entry)
    {
        var maxWidth = GetAvailableWidth();

        if (maxWidth <= 0f)
        {
            return;
        }

        var color = GetColorForLevel(entry.Level);
        var blink = entry.Level == LogEventLevel.Fatal ? FatalBlinkDuration : 0f;
        if (TryOverwriteLastLine(entry.Message, color, blink))
        {
            return;
        }
        var defaultStyle = new StyledSpan(string.Empty, color, null, false, false, false);
        var spans = _bbcodeParser.Parse(NormalizeMessage(entry.Message), defaultStyle);
        var lines = WrapSpans(spans, maxWidth);

        foreach (var line in lines)
        {
            _typewriterQueue.EnqueueLine(line, blink);
        }
    }

    private void TrimToMaxLines()
    {
        var maxLines = GetMaxVisibleLines();

        if (maxLines <= 0)
        {
            _lines.Clear();

            return;
        }

        var reserved = _currentLine != null ? 1 : 0;
        var maxStored = Math.Max(0, maxLines - reserved);

        while (_lines.Count > maxStored)
        {
            _lines.RemoveAt(0);
        }
    }

    private bool TryOverwriteLastLine(string message, LyColor color, float blink)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        if (message.Contains("\r\n", StringComparison.Ordinal))
        {
            return false;
        }

        if (message.Contains('\n'))
        {
            return false;
        }

        var lastIndex = message.LastIndexOf('\r');
        if (lastIndex < 0)
        {
            return false;
        }

        string newText;
        if (lastIndex == message.Length - 1)
        {
            newText = message[..lastIndex];
        }
        else
        {
            newText = message[(lastIndex + 1)..];
        }

        var defaultStyle = new StyledSpan(string.Empty, color, null, false, false, false);
        var spans = _bbcodeParser.Parse(newText, defaultStyle);
        var lines = WrapSpans(spans, GetAvailableWidth());

        if (_typewriterQueue.HasCurrentLine)
        {
            _typewriterQueue.ReplaceCurrentLine(lines.Count > 0 ? lines[^1] : Array.Empty<StyledSpan>(), blink);
        }
        else if (_lines.Count > 0)
        {
            _lines.RemoveAt(_lines.Count - 1);
            foreach (var line in lines)
            {
                _lines.Add(new RenderedLine(line, blink));
            }
        }
        else if (lines.Count > 0)
        {
            _lines.Add(new RenderedLine(lines[0], blink));
        }

        return true;
    }

    private float GetAvailableWidth()
        => MathF.Max(0f, Size.X - Margin.X - Margin.Z);

    private float GetAvailableHeight()
        => MathF.Max(0f, Size.Y - Margin.Y - Margin.W);

    private float GetLineHeight()
    {
        var height = _fontManager.MeasureText(FontName, FontSize, "Ag").Y;

        return height > 0f ? height : 1f;
    }

    private int GetMaxVisibleLines()
    {
        var height = GetAvailableHeight();
        var lineHeight = GetLineHeight();

        if (lineHeight <= 0f)
        {
            return 0;
        }

        return Math.Max(1, (int)MathF.Floor(height / lineHeight));
    }

    private bool ResolveLineVisibility(RenderedLine line)
    {
        if (line.BlinkRemaining <= 0f)
        {
            return true;
        }

        var blinkPhase = (int)MathF.Floor(_blinkElapsed * FatalBlinkFrequency) % 2;

        return blinkPhase == 0;
    }

    private LyColor GetColorForLevel(LogEventLevel level)
        => level switch
        {
            LogEventLevel.Verbose => TraceColor,
            LogEventLevel.Debug => DebugColor,
            LogEventLevel.Information => InfoColor,
            LogEventLevel.Warning => WarningColor,
            LogEventLevel.Error => ErrorColor,
            LogEventLevel.Fatal => FatalColor,
            _ => InfoColor
        };

    private List<IReadOnlyList<StyledSpan>> WrapSpans(IReadOnlyList<StyledSpan> spans, float maxWidth)
    {
        var lines = new List<IReadOnlyList<StyledSpan>>();
        if (spans.Count == 0)
        {
            return lines;
        }

        var currentLine = new List<StyledSpan>();
        var currentWidth = 0f;

        foreach (var token in TokenizeSpans(spans))
        {
            if (token.IsLineBreak)
            {
                lines.Add(currentLine);
                currentLine = new List<StyledSpan>();
                currentWidth = 0f;
                continue;
            }

            var tokenWidth = MeasureWidth(token.Span.Text);
            if (currentWidth + tokenWidth <= maxWidth)
            {
                currentLine.Add(token.Span);
                currentWidth += tokenWidth;
                continue;
            }

            if (tokenWidth > maxWidth)
            {
                SplitTokenAcrossLines(token.Span, maxWidth, lines, ref currentLine, ref currentWidth);
                continue;
            }

            lines.Add(currentLine);
            currentLine = new List<StyledSpan> { token.Span };
            currentWidth = tokenWidth;
        }

        if (currentLine.Count > 0 || lines.Count == 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private IEnumerable<StyledToken> TokenizeSpans(IReadOnlyList<StyledSpan> spans)
    {
        foreach (var span in spans)
        {
            if (string.IsNullOrEmpty(span.Text))
            {
                continue;
            }

            var text = span.Text;
            var start = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '\n')
                {
                    if (i > start)
                    {
                        yield return new StyledToken(span with { Text = text[start..i] }, false);
                    }

                    yield return StyledToken.LineBreak;
                    start = i + 1;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    if (i > start)
                    {
                        yield return new StyledToken(span with { Text = text[start..i] }, false);
                    }

                    yield return new StyledToken(span with { Text = ch.ToString() }, false);
                    start = i + 1;
                }
            }

            if (start < text.Length)
            {
                yield return new StyledToken(span with { Text = text[start..] }, false);
            }
        }
    }

    private void SplitTokenAcrossLines(
        StyledSpan token,
        float maxWidth,
        List<IReadOnlyList<StyledSpan>> lines,
        ref List<StyledSpan> currentLine,
        ref float currentWidth)
    {
        var current = string.Empty;
        var remainingWidth = maxWidth - currentWidth;

        foreach (var ch in token.Text)
        {
            var candidate = current + ch;
            var candidateWidth = MeasureWidth(candidate);
            if (candidateWidth > remainingWidth && current.Length > 0)
            {
                currentLine.Add(token with { Text = current });
                currentWidth += MeasureWidth(current);
                lines.Add(currentLine);
                currentLine = new List<StyledSpan>();
                currentWidth = 0f;
                remainingWidth = maxWidth;
                current = ch.ToString();
            }
            else
            {
                if (candidateWidth > remainingWidth && current.Length == 0)
                {
                    if (currentLine.Count > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<StyledSpan>();
                        currentWidth = 0f;
                        remainingWidth = maxWidth;
                    }
                }

                current = candidate;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            currentLine.Add(token with { Text = current });
            currentWidth += MeasureWidth(current);
        }
    }

    private float MeasureWidth(string text)
        => _fontManager.MeasureText(FontName, FontSize, text).X;

    private string NormalizeMessage(string message)
        => message.Replace("\r\n", "\n").Replace('\r', '\n');

    private RenderedLine ResolveRenderLine(int index, int totalLines)
    {
        if (_currentLine == null)
        {
            return _lines[index];
        }

        if (index == totalLines - 1)
        {
            return _currentLine;
        }

        return _lines[index];
    }

    private void ConsumeCompletedLines()
    {
        var completed = _typewriterQueue.CompletedLines;
        for (var i = _consumedCompletedLines; i < completed.Count; i++)
        {
            var line = completed[i];
            _lines.Add(new RenderedLine(line.Spans, line.BlinkRemaining));
        }

        _consumedCompletedLines = completed.Count;
    }

    private void UpdateCurrentLine()
    {
        if (!_typewriterQueue.HasCurrentLine)
        {
            _currentLine = null;
            _currentLineBlinkRemaining = 0f;
            return;
        }

        if (_currentLineSequence != _typewriterQueue.CurrentLineSequence)
        {
            _currentLineSequence = _typewriterQueue.CurrentLineSequence;
            _currentLineBlinkRemaining = _typewriterQueue.CurrentLineBlinkRemaining;
        }

        _currentLine = new RenderedLine(_typewriterQueue.CurrentLineSpans, _currentLineBlinkRemaining);
    }

    private bool IsCursorVisible()
    {
        if (_currentLine == null)
        {
            return false;
        }

        if (CursorBlinkFrequency <= 0f)
        {
            return true;
        }

        var blinkPhase = (int)MathF.Floor(_cursorBlinkElapsed * CursorBlinkFrequency) % 2;
        return blinkPhase == 0;
    }

    private void DrawStyledLine(SpriteBatch spriteBatch, RenderedLine line, Vector2 origin)
    {
        var x = origin.X;
        foreach (var span in line.Spans)
        {
            if (span.Background.HasValue)
            {
                var size = _fontManager.MeasureText(FontName, FontSize, span.Text);
                if (size.X > 0 && size.Y > 0)
                {
                    spriteBatch.DrawRectangle(new Vector2(x, origin.Y), size, span.Background.Value);
                }
            }

            var drawColor = span.Foreground;
            if (span.Bold)
            {
                spriteBatch.DrawFont(FontName, FontSize, span.Text, new Vector2(x + 1, origin.Y), drawColor);
            }

            var italicOffset = span.Italic ? 1f : 0f;
            spriteBatch.DrawFont(FontName, FontSize, span.Text, new Vector2(x + italicOffset, origin.Y), drawColor);

            if (span.Underline)
            {
                var size = _fontManager.MeasureText(FontName, FontSize, span.Text);
                var thickness = MathF.Max(1f, FontSize * 0.05f);
                var underlineY = origin.Y + size.Y - thickness;
                spriteBatch.DrawRectangle(new Vector2(x, underlineY), new Vector2(size.X, thickness), drawColor);
            }

            x += MeasureWidth(span.Text);
        }
    }

    private readonly record struct StyledToken(StyledSpan Span, bool IsLineBreak)
    {
        public static StyledToken LineBreak => new(new StyledSpan(string.Empty, LyColor.White, null, false, false, false), true);
    }
}

using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Logging;
using LillyQuest.Engine.Managers.Screens.Base;
using Serilog.Events;

namespace LillyQuest.Engine.Screens.Logging;

public sealed class LogLine
{
    public string Text { get; set; }
    public LyColor Color { get; set; }
    public float BlinkRemaining { get; set; }

    public LogLine(string text, LyColor color, float blinkRemaining)
    {
        Text = text;
        Color = color;
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
    private readonly List<LogLine> _lines = [];

    private float _blinkElapsed;

    public string FontName { get; set; } = "default_font_log";
    public int FontSize { get; set; } = 14;
    public LyColor BackgroundColor { get; set; } = LyColor.Black;
    public float BackgroundAlpha { get; set; } = 0.6f;
    public int DispatchBatchSize { get; set; } = 64;
    public float FatalBlinkDuration { get; set; } = 1f;
    public float FatalBlinkFrequency { get; set; } = 6f;

    public LogScreen(ILogEventDispatcher dispatcher, IFontManager fontManager)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _fontManager = fontManager ?? throw new ArgumentNullException(nameof(fontManager));
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

        for (var i = _lines.Count - 1; i >= 0 && y >= Margin.Y; i--)
        {
            var line = _lines[i];
            var color = ResolveLineColor(line);
            if (color.A > 0)
            {
                spriteBatch.DrawFont(FontName, FontSize, line.Text, new(x, y), color);
            }

            y -= lineHeight;
        }

        spriteBatch.PopTranslation();
    }

    public override void Update(GameTime gameTime)
    {
        _dispatcher.Dispatch(DispatchBatchSize);
        var deltaSeconds = (float)gameTime.Elapsed.TotalSeconds;
        _blinkElapsed += deltaSeconds;

        foreach (var line in _lines)
        {
            if (line.BlinkRemaining <= 0f)
            {
                continue;
            }

            line.BlinkRemaining = Math.Max(0f, line.BlinkRemaining - deltaSeconds);
        }
    }

    protected IReadOnlyList<LogLine> GetLines()
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
        var lines = WrapText(entry.Message, maxWidth);

        foreach (var line in lines)
        {
            _lines.Add(new LogLine(line, color, blink));
        }

        TrimToMaxLines();
    }

    private void TrimToMaxLines()
    {
        var maxLines = GetMaxVisibleLines();

        if (maxLines <= 0)
        {
            _lines.Clear();

            return;
        }

        while (_lines.Count > maxLines)
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

        if (_lines.Count == 0)
        {
            _lines.Add(new LogLine(newText, color, blink));
        }
        else
        {
            _lines[^1] = new LogLine(newText, color, blink);
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

    private LyColor ResolveLineColor(LogLine line)
    {
        if (line.BlinkRemaining <= 0f)
        {
            return line.Color;
        }

        var blinkPhase = (int)MathF.Floor(_blinkElapsed * FatalBlinkFrequency) % 2;

        return blinkPhase == 0 ? line.Color : LyColor.Transparent;
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

    private List<string> WrapText(string text, float maxWidth)
    {
        var result = new List<string>();

        if (string.IsNullOrEmpty(text))
        {
            result.Add(string.Empty);

            return result;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var paragraphs = normalized.Split('\n');

        foreach (var paragraph in paragraphs)
        {
            WrapParagraph(paragraph, maxWidth, result);
        }

        return result;
    }

    private void WrapParagraph(string paragraph, float maxWidth, List<string> output)
    {
        if (string.IsNullOrEmpty(paragraph))
        {
            output.Add(string.Empty);

            return;
        }

        var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = string.Empty;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(current))
            {
                AppendWord(word, maxWidth, output, ref current);
                continue;
            }

            var candidate = $"{current} {word}";

            if (MeasureWidth(candidate) <= maxWidth)
            {
                current = candidate;
            }
            else
            {
                output.Add(current);
                current = string.Empty;
                AppendWord(word, maxWidth, output, ref current);
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            output.Add(current);
        }
    }

    private void AppendWord(string word, float maxWidth, List<string> output, ref string current)
    {
        if (MeasureWidth(word) <= maxWidth)
        {
            current = word;

            return;
        }

        var chunks = SplitLongWord(word, maxWidth);

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];

            if (i == chunks.Count - 1)
            {
                current = chunk;
            }
            else
            {
                output.Add(chunk);
            }
        }
    }

    private List<string> SplitLongWord(string word, float maxWidth)
    {
        var chunks = new List<string>();
        var current = string.Empty;

        foreach (var ch in word)
        {
            var candidate = current + ch;
            if (!string.IsNullOrEmpty(current) && MeasureWidth(candidate) > maxWidth)
            {
                chunks.Add(current);
                current = ch.ToString();
            }
            else
            {
                current = candidate;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            chunks.Add(current);
        }

        return chunks;
    }

    private float MeasureWidth(string text)
        => _fontManager.MeasureText(FontName, FontSize, text).X;
}

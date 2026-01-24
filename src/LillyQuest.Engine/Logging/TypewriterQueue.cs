namespace LillyQuest.Engine.Logging;

public sealed class TypewriterQueue
{
    public readonly record struct TypewriterLineState(IReadOnlyList<StyledSpan> Spans, float BlinkRemaining);

    private readonly Queue<TypewriterLineState> _pendingLines = new();
    private readonly List<TypewriterLineState> _completedLines = [];
    private TypewriterLine? _currentLine;
    private TypewriterLineState _currentLineState;
    private int _currentLineSequence;
    private float _charactersPerSecond;
    private int _visibleChars;
    private IReadOnlyList<StyledSpan> _visibleSpans = Array.Empty<StyledSpan>();
    private string _visibleText = string.Empty;

    public TypewriterQueue(float charactersPerSecond)
    {
        CharactersPerSecond = charactersPerSecond;
    }

    public float CharactersPerSecond
    {
        get => _charactersPerSecond;
        set => _charactersPerSecond = MathF.Max(0f, value);
    }

    public IReadOnlyList<TypewriterLineState> CompletedLines => _completedLines;

    public IReadOnlyList<StyledSpan> CurrentLineSpans => _visibleSpans;

    public string CurrentLineText => _visibleText;

    public float CurrentLineBlinkRemaining => _currentLine == null ? 0f : _currentLineState.BlinkRemaining;

    public int CurrentLineSequence => _currentLineSequence;

    public bool HasCurrentLine => _currentLine != null;

    public void EnqueueLine(IReadOnlyList<StyledSpan> line)
    {
        EnqueueLine(line, 0f);
    }

    public void EnqueueLine(IReadOnlyList<StyledSpan> line, float blinkRemaining)
    {
        ArgumentNullException.ThrowIfNull(line);
        _pendingLines.Enqueue(new TypewriterLineState(line, blinkRemaining));
    }

    public void EnqueueLines(IEnumerable<IReadOnlyList<StyledSpan>> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        foreach (var line in lines)
        {
            EnqueueLine(line);
        }
    }

    public void Update(TimeSpan delta)
    {
        var deltaChars = CharactersPerSecond * (float)delta.TotalSeconds;
        if (deltaChars <= 0f)
        {
            return;
        }

        Advance(deltaChars);
    }

    public bool ReplaceCurrentLine(IReadOnlyList<StyledSpan> line, float blinkRemaining)
    {
        if (_currentLine == null)
        {
            return false;
        }

        _currentLineState = new TypewriterLineState(line, blinkRemaining);
        _currentLine = new TypewriterLine(line);
        _visibleChars = 0;
        _visibleSpans = Array.Empty<StyledSpan>();
        _visibleText = string.Empty;
        _currentLineSequence++;
        return true;
    }

    private void Advance(float charactersToConsume)
    {
        while (charactersToConsume > 0f)
        {
            if (_currentLine == null)
            {
                if (_pendingLines.Count == 0)
                {
                    return;
                }

                _currentLineState = _pendingLines.Dequeue();
                _currentLine = new TypewriterLine(_currentLineState.Spans);
                _currentLineSequence++;
                _visibleChars = 0;
                _visibleSpans = Array.Empty<StyledSpan>();
                _visibleText = string.Empty;
            }

            var remaining = _currentLine.Length - _visibleChars;
            if (remaining <= 0)
            {
                FinishCurrentLine();
                continue;
            }

            var step = Math.Min(remaining, (int)MathF.Floor(charactersToConsume));
            if (step <= 0)
            {
                return;
            }

            _visibleChars += step;
            _visibleSpans = _currentLine.BuildVisibleSpans(_visibleChars);
            _visibleText = _currentLine.BuildVisibleText(_visibleChars);
            charactersToConsume -= step;

            if (_visibleChars >= _currentLine.Length)
            {
                FinishCurrentLine();
            }
        }
    }

    private void FinishCurrentLine()
    {
        if (_currentLine == null)
        {
            return;
        }

        _completedLines.Add(_currentLineState);
        _currentLine = null;
        _visibleChars = 0;
        _visibleSpans = Array.Empty<StyledSpan>();
        _visibleText = string.Empty;
    }

    private sealed class TypewriterLine
    {
        public TypewriterLine(IReadOnlyList<StyledSpan> spans)
        {
            Spans = spans;
            Length = spans.Sum(span => span.Text.Length);
        }

        public IReadOnlyList<StyledSpan> Spans { get; }
        public int Length { get; }

        public IReadOnlyList<StyledSpan> BuildVisibleSpans(int visibleChars)
        {
            if (visibleChars <= 0)
            {
                return Array.Empty<StyledSpan>();
            }

            var remaining = visibleChars;
            var result = new List<StyledSpan>();

            foreach (var span in Spans)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (span.Text.Length <= remaining)
                {
                    result.Add(span);
                    remaining -= span.Text.Length;
                    continue;
                }

                var partialText = span.Text[..remaining];
                result.Add(span with { Text = partialText });
                remaining = 0;
            }

            return result;
        }

        public string BuildVisibleText(int visibleChars)
        {
            if (visibleChars <= 0)
            {
                return string.Empty;
            }

            var remaining = visibleChars;
            var parts = new List<string>();

            foreach (var span in Spans)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (span.Text.Length <= remaining)
                {
                    parts.Add(span.Text);
                    remaining -= span.Text.Length;
                    continue;
                }

                parts.Add(span.Text[..remaining]);
                remaining = 0;
            }

            return string.Concat(parts);
        }
    }
}

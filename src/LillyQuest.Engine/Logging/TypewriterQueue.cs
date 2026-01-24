namespace LillyQuest.Engine.Logging;

public sealed class TypewriterQueue
{
    private readonly Queue<TypewriterLine> _pendingLines = new();
    private readonly List<IReadOnlyList<StyledSpan>> _completedLines = [];
    private TypewriterLine? _currentLine;
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

    public IReadOnlyList<IReadOnlyList<StyledSpan>> CompletedLines => _completedLines;

    public IReadOnlyList<StyledSpan> CurrentLineSpans => _visibleSpans;

    public string CurrentLineText => _visibleText;

    public void EnqueueLine(IReadOnlyList<StyledSpan> line)
    {
        ArgumentNullException.ThrowIfNull(line);
        _pendingLines.Enqueue(new TypewriterLine(line));
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

                _currentLine = _pendingLines.Dequeue();
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

        _completedLines.Add(_currentLine.Spans);
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

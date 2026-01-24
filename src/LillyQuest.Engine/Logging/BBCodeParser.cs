using System.Reflection;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Logging;

public sealed class BBCodeParser
{
    private static readonly Dictionary<string, LyColor> NamedColors = BuildNamedColors();

    public List<StyledSpan> Parse(string input, StyledSpan? defaultStyle = null)
    {
        var style = defaultStyle ?? new StyledSpan(string.Empty, LyColor.White, null, false, false, false);
        var stack = new Stack<StyleState>();
        stack.Push(new StyleState(style.Foreground, style.Background, style.Bold, style.Italic, style.Underline));

        var spans = new List<StyledSpan>();

        if (string.IsNullOrEmpty(input))
        {
            return spans;
        }

        var index = 0;
        while (index < input.Length)
        {
            var current = input[index];
            if (current == '[')
            {
                var closing = input.IndexOf(']', index + 1);
                if (closing < 0)
                {
                    AppendSpan(spans, stack.Peek(), input[index..]);
                    break;
                }

                var tagContent = input.Substring(index + 1, closing - index - 1);
                if (TryHandleTag(tagContent, stack))
                {
                    index = closing + 1;
                    continue;
                }

                AppendSpan(spans, stack.Peek(), "[");
                index++;
                continue;
            }

            var nextTag = input.IndexOf('[', index);
            if (nextTag < 0)
            {
                AppendSpan(spans, stack.Peek(), input[index..]);
                break;
            }

            AppendSpan(spans, stack.Peek(), input.Substring(index, nextTag - index));
            index = nextTag;
        }

        return spans;
    }

    private static bool TryHandleTag(string content, Stack<StyleState> stack)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var trimmed = content.Trim();
        if (trimmed.StartsWith('/'))
        {
            var closingName = trimmed[1..].Trim();
            if (!IsSupportedTag(closingName))
            {
                return false;
            }

            if (stack.Count > 1)
            {
                stack.Pop();
            }

            return true;
        }

        var tagName = trimmed;
        string? tagValue = null;
        var equalsIndex = trimmed.IndexOf('=');
        if (equalsIndex >= 0)
        {
            tagName = trimmed[..equalsIndex].Trim();
            tagValue = trimmed[(equalsIndex + 1)..].Trim();
        }

        var current = stack.Peek();

        switch (tagName.ToLowerInvariant())
        {
            case "b":
                stack.Push(current with { Bold = true });
                return true;
            case "i":
                stack.Push(current with { Italic = true });
                return true;
            case "u":
                stack.Push(current with { Underline = true });
                return true;
            case "color":
            {
                var foreground = current.Foreground;
                if (!string.IsNullOrWhiteSpace(tagValue) &&
                    TryParseColor(tagValue, out var parsed))
                {
                    foreground = parsed;
                }

                stack.Push(current with { Foreground = foreground });
                return true;
            }
            case "bcolor":
            {
                var background = current.Background;
                if (!string.IsNullOrWhiteSpace(tagValue) &&
                    TryParseColor(tagValue, out var parsed))
                {
                    background = parsed;
                }

                stack.Push(current with { Background = background });
                return true;
            }
            default:
                return false;
        }
    }

    private static bool IsSupportedTag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return name.Equals("b", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("i", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("u", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("color", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("bcolor", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendSpan(List<StyledSpan> spans, StyleState state, string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        spans.Add(new StyledSpan(text, state.Foreground, state.Background, state.Bold, state.Italic, state.Underline));
    }

    private static bool TryParseColor(string value, out LyColor color)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            color = default;
            return false;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith('#') || trimmed.Length is 6 or 8)
        {
            try
            {
                color = LyColor.FromHex(trimmed);
                return true;
            }
            catch
            {
                // ignore invalid hex
            }
        }

        if (NamedColors.TryGetValue(trimmed, out color))
        {
            return true;
        }

        color = default;
        return false;
    }

    private static Dictionary<string, LyColor> BuildNamedColors()
    {
        var colors = new Dictionary<string, LyColor>(StringComparer.OrdinalIgnoreCase);
        var properties = typeof(LyColor).GetProperties(BindingFlags.Public | BindingFlags.Static);

        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(LyColor))
            {
                continue;
            }

            var value = (LyColor)property.GetValue(null)!;
            colors[property.Name] = value;
        }

        return colors;
    }

    private readonly record struct StyleState(
        LyColor Foreground,
        LyColor? Background,
        bool Bold,
        bool Italic,
        bool Underline
    );
}

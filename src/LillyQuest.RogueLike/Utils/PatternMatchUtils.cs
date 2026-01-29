namespace LillyQuest.RogueLike.Utils;

public static class PatternMatchUtils
{
    public static bool Matches(string value, string pattern)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        if (pattern == "*")
        {
            return true;
        }

        var startsWithWildcard = pattern.StartsWith('*');
        var endsWithWildcard = pattern.EndsWith('*');

        if (!startsWithWildcard && !endsWithWildcard)
        {
            return string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);
        }

        var trimmed = pattern.Trim('*');

        if (string.IsNullOrEmpty(trimmed))
        {
            return true;
        }

        if (startsWithWildcard && endsWithWildcard)
        {
            return value.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
        }

        if (startsWithWildcard)
        {
            return value.EndsWith(trimmed, StringComparison.OrdinalIgnoreCase);
        }

        return value.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase);
    }
}

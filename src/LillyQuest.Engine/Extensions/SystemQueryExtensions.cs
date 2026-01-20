using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Extensions;

public static class SystemQueryTypeExtensions
{
    public static IReadOnlyList<SystemQueryType> GetFlags(this SystemQueryType value)
    {
        if (value == SystemQueryType.None)
        {
            return [];
        }

        var result = new List<SystemQueryType>();

        foreach (var flag in Enum.GetValues<SystemQueryType>())
        {
            if (flag == SystemQueryType.None)
            {
                continue;
            }

            if (value.HasFlag(flag))
            {
                result.Add(flag);
            }
        }

        return result;
    }
}

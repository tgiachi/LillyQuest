using Humanizer;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Colorschemas;

public class ColorSchemaJson : BaseJsonEntity
{
    public LyColor Color { get; set; }

    public override string ToString()
        => $"{nameof(Color)}: {Color.ToString()}";
}

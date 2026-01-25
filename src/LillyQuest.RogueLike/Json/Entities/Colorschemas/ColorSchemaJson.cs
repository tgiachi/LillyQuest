using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Colorschemas;

public class ColorSchemaJson : BaseJsonEntity
{
    public LyColor BackgroundColor { get; set; }

    public LyColor ForegroundColor { get; set; }
}

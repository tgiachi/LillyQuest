using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Colorschemas;

public class ColorSchemaDefintionJson : BaseJsonEntity
{
    public List<ColorSchemaJson> Colors { get; set; } = [];

}

using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Prefabs;

public class MapPrefabDefinitionJson : BaseJsonEntity
{
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public List<string> Content { get; set; }
    public Dictionary<string, MapPaletteEntry> Palette { get; set; }
}

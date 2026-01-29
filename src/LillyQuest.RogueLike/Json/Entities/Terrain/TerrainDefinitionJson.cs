using System.Text.Json.Serialization;
using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Terrain;

public class TerrainDefinitionJson : BaseJsonEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }

    public List<string> Flags { get; set; } = [];
    public int MovementCost { get; set;  } = 1;


}

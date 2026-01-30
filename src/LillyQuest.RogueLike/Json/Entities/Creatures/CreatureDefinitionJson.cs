using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Creatures;

public class CreatureDefinitionJson : BaseJsonEntity
{
    public List<string> Flags { get; set; } = [];
    public string BrainType { get; set; }
    public CreatureGenderType Gender { get; set; }
    public string? Name { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
}

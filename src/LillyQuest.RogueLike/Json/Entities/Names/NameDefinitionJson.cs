using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Creatures;

namespace LillyQuest.RogueLike.Json.Entities.Names;

public class NameDefinitionJson : BaseJsonEntity
{
    public CreatureGenderType Gender { get; set; }
    public List<string> FirstNames { get; set; } = [];
    public List<string> LastNames { get; set; } = [];

}

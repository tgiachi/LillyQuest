using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.LootTables;

/// <summary>
/// JSON definition for a loot table.
/// </summary>
public class LootTableDefinitionJson : BaseJsonEntity
{
    /// <summary>
    /// Number of rolls on this table (dice notation, e.g. "1", "2d4").
    /// </summary>
    public string Rolls { get; set; } = "1";

    /// <summary>
    /// Entries in this loot table.
    /// </summary>
    public List<LootEntryJson> Entries { get; set; } = [];
}

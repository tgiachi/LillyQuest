namespace LillyQuest.RogueLike.Json.Entities.LootTables;

/// <summary>
/// A single entry in a loot table.
/// </summary>
public class LootEntryJson
{
    /// <summary>
    /// ID of the item to drop. Mutually exclusive with LootTableId.
    /// </summary>
    public string? ItemId { get; set; }

    /// <summary>
    /// ID of a nested loot table. Mutually exclusive with ItemId.
    /// </summary>
    public string? LootTableId { get; set; }

    /// <summary>
    /// Percentage chance (0.0 - 100.0) of this entry being selected.
    /// </summary>
    public float Chance { get; set; }

    /// <summary>
    /// Number of items to drop (dice notation, e.g. "1", "2d6").
    /// </summary>
    public string Count { get; set; } = "1";
}

using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Json.Entities.Items;

/// <summary>
/// JSON definition for an item.
/// </summary>
public class ItemDefinitionJson : BaseJsonEntity
{
    // Identity and display
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public List<string> Flags { get; set; } = [];

    // Physical
    public float Weight { get; set; } = 1.0f;

    // Equipment stats (nullable - only for equipment)
    public string? Damage { get; set; }
    public string? Defense { get; set; }
    public int? AccuracyBonus { get; set; }
    public int? EvasionBonus { get; set; }
    public EquipmentSlot? Slot { get; set; }

    // Consumable
    public string? UseEffect { get; set; }
    public int? Charges { get; set; }
    public int? Nutrition { get; set; }

    // Container
    public bool IsContainer { get; set; }
    public float? Capacity { get; set; }
    public string? LootTable { get; set; }
}

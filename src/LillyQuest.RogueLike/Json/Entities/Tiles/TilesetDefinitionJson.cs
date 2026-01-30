using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Json.Entities.Tiles;

/// <summary>
/// Represents a tileset definition containing metadata and a collection of tile definitions.
/// A tileset groups related tiles that are typically used together in the same map layer.
/// </summary>
public class TilesetDefinitionJson : BaseJsonEntity
{
    /// <summary>
    /// Gets or sets the name of this tileset.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the path to the texture file used by this tileset.
    /// </summary>
    public string TextureName { get; set; }

    /// <summary>
    /// Gets or sets the collection of tile definitions that belong to this tileset.
    /// </summary>
    public List<TileDefinition> Tiles { get; set; } = [];
}

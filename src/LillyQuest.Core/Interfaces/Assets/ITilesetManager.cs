using LillyQuest.Core.Data.Assets.Tiles;

namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Manages the loading and access of tilesets.
/// A tileset is a texture containing a grid of fixed-size tiles.
/// </summary>
public interface ITilesetManager : IDisposable
{
    /// <summary>
    /// Gets all loaded tilesets.
    /// </summary>
    IReadOnlyDictionary<string, Tileset> GetAllTilesets();

    /// <summary>
    /// Gets a loaded tileset by name.
    /// </summary>
    /// <param name="name">Unique name of the tileset</param>
    /// <returns>The requested tileset</returns>
    /// <exception cref="KeyNotFoundException">If the tileset is not loaded</exception>
    Tileset GetTileset(string name);

    /// <summary>
    /// Checks if a tileset is already loaded.
    /// </summary>
    /// <param name="name">Name of the tileset</param>
    /// <returns>true if the tileset is loaded, false otherwise</returns>
    bool HasTileset(string name);

    /// <summary>
    /// Loads a tileset from file.
    /// </summary>
    /// <param name="name">Unique name to assign to the tileset</param>
    /// <param name="filePath">Path to the image file (PNG, JPG, etc.)</param>
    /// <param name="tileWidth">Width of each tile in pixels</param>
    /// <param name="tileHeight">Height of each tile in pixels</param>
    /// <param name="spacing">Space between tiles in pixels (default: 0)</param>
    /// <param name="margin">Margin around tiles in pixels (default: 0)</param>
    void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin);

    /// <summary>
    /// Loads a tileset from binary data (PNG).
    /// </summary>
    /// <param name="name">Unique name to assign to the tileset</param>
    /// <param name="content">PNG data in memory</param>
    /// <param name="tileWidth">Width of each tile in pixels</param>
    /// <param name="tileHeight">Height of each tile in pixels</param>
    /// <param name="spacing">Space between tiles in pixels</param>
    /// <param name="margin">Margin around tiles in pixels</param>
    void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin);

    /// <summary>
    /// Attempts to get a loaded tileset safely.
    /// </summary>
    /// <param name="name">Name of the tileset</param>
    /// <param name="tileset">Tileset found, null if not found</param>
    /// <returns>true if the tileset was found, false otherwise</returns>
    bool TryGetTileset(string name, out Tileset tileset);

    /// <summary>
    /// Unloads a tileset from memory.
    /// </summary>
    /// <param name="name">Name of the tileset to unload</param>
    void UnloadTileset(string name);
}

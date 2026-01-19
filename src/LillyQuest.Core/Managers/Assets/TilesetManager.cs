using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using Serilog;

namespace LillyQuest.Core.Managers.Assets;

public class TilesetManager : ITilesetManager
{
    private readonly ILogger _logger = Log.ForContext<TilesetManager>();
    private readonly ITextureManager _textureManager;
    private readonly Dictionary<string, Tileset> _tilesets = new();

    public TilesetManager(ITextureManager textureManager)
        => _textureManager = textureManager;

    public void Dispose()
    {
        foreach (var tileset in _tilesets.Values)
        {
            tileset.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public Tileset GetTileset(string name)
        => _tilesets.TryGetValue(name, out var tileset)
               ? tileset
               : throw new KeyNotFoundException($"Tileset with name {name} not found.");

    public bool HasTileset(string name)
        => _tilesets.ContainsKey(name);

    public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
    {
        if (_tilesets.ContainsKey(name))
        {
            _logger.Warning("Tileset with name {Name} already loaded.", name);

            return;
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Tileset file not found: {filePath}");
        }

        var textureName = $"{name}_texture";
        _textureManager.LoadTextureWithChromaKey(textureName, filePath);
        var texture = _textureManager.GetTexture(textureName);

        var tileset = new Tileset(filePath, tileWidth, tileHeight, spacing, margin, texture);
        _tilesets[name] = tileset;
        _logger.Information("Tileset {Name} loaded from {FilePath} with magenta chroma key.", name, filePath);
    }

    public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
    {
        if (_tilesets.ContainsKey(name))
        {
            _logger.Warning("Tileset with name {Name} already loaded.", name);

            return;
        }

        if (content.Length == 0)
        {
            throw new ArgumentException("Tileset data cannot be empty.", nameof(content));
        }

        var textureName = $"{name}_texture";
        _textureManager.LoadTextureFromPngWithChromaKey(textureName, content);
        var texture = _textureManager.GetTexture(textureName);

        var tileset = new Tileset($"[embedded]_{name}", tileWidth, tileHeight, spacing, margin, texture);
        _tilesets[name] = tileset;
        _logger.Information("Tileset {Name} loaded from data with magenta chroma key.", name);
    }

    public bool TryGetTileset(string name, out Tileset tileset)
        => _tilesets.TryGetValue(name, out tileset);

    public void UnloadTileset(string name)
    {
        if (_tilesets.Remove(name, out var tileset))
        {
            var textureName = $"{name}_texture";

            if (_textureManager.HasTexture(textureName))
            {
                _textureManager.UnloadTexture(textureName);
            }
            tileset.Dispose();
            _logger.Information("Tileset {Name} unloaded.", name);
        }
        else
        {
            _logger.Warning("Tileset with name {Name} not found.", name);
        }
    }
}

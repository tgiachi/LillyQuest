using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using Serilog;

namespace LillyQuest.RogueLike.Services.Loaders;

public class TileSetService : IDataLoaderReceiver
{
    private readonly ILogger _logger = Log.ForContext<TileSetService>();
    private readonly ColorService _colorService;

    private readonly Dictionary<string, Dictionary<string, TileDefinition>> _tilesets = new();
    private readonly Dictionary<string, Dictionary<string, ResolvedTileData>> _resolvedTilesets = new();

    public string DefaultTileset { get; set; }

    public TileSetService(ColorService colorService)
        => _colorService = colorService;

    public void ClearData()
    {
        _tilesets.Clear();
        _resolvedTilesets.Clear();
    }

    public bool VerifyLoadedData()
    {
        foreach (var tileset in _tilesets.Values)
        {
            foreach (var tile in tileset)
            {
                if (string.IsNullOrEmpty(tile.Value.Id))
                {
                    throw new InvalidOperationException("Tile with missing ID found during verification");

                }

                if (string.IsNullOrEmpty(tile.Value.FgColor) || string.IsNullOrEmpty(tile.Value.BgColor))
                {
                    throw new InvalidOperationException($"Tile {tile.Value.Id} has missing color IDs during verification");
                }

                var fgColor = _colorService.GetColorById(tile.Value.FgColor);
                var bgColor = _colorService.GetColorById(tile.Value.BgColor);

                if (fgColor is null || bgColor is null)
                {
                    throw new InvalidOperationException($"Tile {tile.Value.Id} has unresolved colors during verification");
                }
            }
        }

        return true;
    }

    public bool TryGetAnyTilesetName(out string tilesetName)
    {
        tilesetName = string.Empty;

        if (_tilesets.Count == 0)
        {
            return false;
        }

        tilesetName = _tilesets.Keys.First();

        return true;
    }

    public Type[] GetLoadTypes()
        => [typeof(TilesetDefinitionJson)];

    public async Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        var tilesets = entities.Cast<TilesetDefinitionJson>().ToList();

        foreach (var tileset in tilesets)
        {
            if (string.IsNullOrEmpty(tileset.Name))
            {
                _logger.Warning("Tileset definition missing name for id {Id}", tileset.Id);

                continue;
            }

            var tilesById = new Dictionary<string, TileDefinition>();
            var resolvedById = new Dictionary<string, ResolvedTileData>();

            foreach (var tile in tileset.Tiles)
            {
                if (string.IsNullOrEmpty(tile.Id))
                {
                    _logger.Warning("Tileset {TilesetName} has tile with missing id", tileset.Name);

                    continue;
                }

                tilesById[tile.Id] = tile;

                if (string.IsNullOrEmpty(tile.FgColor) || string.IsNullOrEmpty(tile.BgColor))
                {
                    _logger.Warning(
                        "Tile {TileId} in tileset {TilesetName} has missing color ids (fg: {FgColor}, bg: {BgColor})",
                        tile.Id,
                        tileset.Name,
                        tile.FgColor,
                        tile.BgColor
                    );

                    continue;
                }

                var fgColor = _colorService.GetColorById(tile.FgColor);
                var bgColor = _colorService.GetColorById(tile.BgColor);

                if (fgColor is null || bgColor is null)
                {
                    _logger.Warning(
                        "Tile {TileId} in tileset {TilesetName} has unresolved colors (fg: {FgColor}, bg: {BgColor})",
                        tile.Id,
                        tileset.Name,
                        tile.FgColor,
                        tile.BgColor
                    );

                    continue;
                }

                var symbol = tile.Symbol ?? string.Empty;
                resolvedById[tile.Id] = new(tile.Id, symbol, fgColor.Value, bgColor.Value, tile.Animation);
            }

            _tilesets[tileset.Name] = tilesById;
            _resolvedTilesets[tileset.Name] = resolvedById;

            _logger.Information(
                "Loaded Tileset {Name} with {TileCount} tiles, {ResolvedCount} resolved",
                tileset.Name,
                tilesById.Count,
                resolvedById.Count
            );
        }
    }

    public bool TryGetTile(string tileId, out ResolvedTileData tile, string? tilesetName = null)
    {
        tile = null!;

        var effectiveTileset = string.IsNullOrEmpty(tilesetName) ? DefaultTileset : tilesetName;

        if (string.IsNullOrEmpty(effectiveTileset))
        {
            _logger.Warning("No tileset specified and no default tileset defined");

            return false;
        }

        if (!_resolvedTilesets.TryGetValue(effectiveTileset, out var tilesById))
        {
            _logger.Warning("Tileset {TilesetName} not found", effectiveTileset);

            return false;
        }

        if (tilesById.TryGetValue(tileId, out var resolvedTile))
        {
            tile = resolvedTile;

            return true;
        }

        _logger.Warning("Tile with ID {TileId} not found in tileset {TilesetName}", tileId, effectiveTileset);

        return false;
    }
}

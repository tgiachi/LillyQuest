using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Game.Systems;

public sealed class MapRenderSystem : GameEntity, IUpdateableEntity
{
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapRenderState> _states = new();

    public MapRenderSystem(int chunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        _chunkSize = chunkSize;
        Name = nameof(MapRenderSystem);
    }

    private sealed record MapRenderState(
        LyQuestMap Map,
        TilesetSurfaceScreen Surface,
        IFOVService? FovService,
        DirtyChunkTracker DirtyTracker
    );

    public IReadOnlyCollection<ChunkCoord> GetDirtyChunks(LyQuestMap map)
        => _states.TryGetValue(map, out var state)
               ? state.DirtyTracker.DirtyChunks
               : Array.Empty<ChunkCoord>();

    public void HandleObjectMoved(LyQuestMap map, Point oldPosition, Point newPosition)
    {
        if (_states.TryGetValue(map, out var state))
        {
            state.DirtyTracker.MarkDirtyForTile(oldPosition.X, oldPosition.Y);
            state.DirtyTracker.MarkDirtyForTile(newPosition.X, newPosition.Y);
        }
    }

    public bool HasMap(LyQuestMap map)
        => _states.ContainsKey(map);

    public void MarkDirtyForTile(LyQuestMap map, int x, int y)
    {
        if (_states.TryGetValue(map, out var state))
        {
            state.DirtyTracker.MarkDirtyForTile(x, y);
        }
    }

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen surface, IFOVService? fovService)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        var state = new MapRenderState(map, surface, fovService, new(_chunkSize));
        _states[map] = state;

        map.ObjectMoved += (_, args) => HandleObjectMoved(map, args.OldPosition, args.NewPosition);
        map.ObjectAdded += (_, args) => HandleObjectMoved(map, args.Item.Position, args.Item.Position);
        map.ObjectRemoved += (_, args) => HandleObjectMoved(map, args.Item.Position, args.Item.Position);
    }

    public void UnregisterMap(LyQuestMap map)
        => _states.Remove(map);

    public void Update(GameTime gameTime)
    {
        foreach (var state in _states.Values)
        {
            if (state.DirtyTracker.DirtyChunks.Count == 0)
            {
                continue;
            }

            foreach (var chunk in state.DirtyTracker.DirtyChunks)
            {
                RebuildChunk(state, chunk);
            }

            state.DirtyTracker.DirtyChunks.Clear();
        }
    }

    private static TileRenderData BuildCreatureTile(
        LyQuestMap map,
        IFOVService? fovService,
        Point position
    )
    {
        var renderCreature = fovService == null || fovService.IsVisible(position);
        var empty = new TileRenderData(-1, LyColor.White);

        if (!renderCreature)
        {
            return empty;
        }

        foreach (var obj in map.GetObjectsAt(position))
        {
            if (obj is CreatureGameObject creature)
            {
                return new(
                    creature.Tile.Symbol[0],
                    creature.Tile.ForegroundColor,
                    creature.Tile.BackgroundColor
                );
            }
        }

        return empty;
    }

    private static TileRenderData BuildTerrainTile(
        LyQuestMap map,
        IFOVService? fovService,
        Point position
    )
    {
        var tile = new TileRenderData(-1, LyColor.White);

        if (map.GetTerrainAt(position) is TerrainGameObject terrain)
        {
            tile = new(
                terrain.Tile.Symbol[0],
                terrain.Tile.ForegroundColor,
                terrain.Tile.BackgroundColor
            );
        }

        if (fovService == null)
        {
            return tile;
        }

        var isVisible = fovService.IsVisible(position);
        var isExplored = fovService.IsExplored(position);

        if (!isExplored)
        {
            return new(-1, LyColor.White);
        }

        return !isVisible && isExplored ? DarkenTile(tile) : tile;
    }

    private static TileRenderData DarkenTile(TileRenderData tile)
    {
        return new(
            tile.TileIndex,
            DarkenColor(tile.ForegroundColor, 0.5f),
            DarkenColor(tile.BackgroundColor, 0.5f),
            tile.Flip
        );

        static LyColor DarkenColor(LyColor color, float factor)
            => new(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
    }

    private void RebuildChunk(MapRenderState state, ChunkCoord chunk)
    {
        var map = state.Map;
        var surface = state.Surface;
        var fovService = state.FovService;
        var startX = chunk.X * _chunkSize;
        var startY = chunk.Y * _chunkSize;
        var endX = Math.Min(startX + _chunkSize, map.Width);
        var endY = Math.Min(startY + _chunkSize, map.Height);

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var position = new Point(x, y);
                var tile = BuildTerrainTile(map, fovService, position);
                surface.AddTileToSurface((int)MapLayer.Terrain, x, y, tile);

                var creatureTile = BuildCreatureTile(map, fovService, position);
                surface.AddTileToSurface((int)MapLayer.Creatures, x, y, creatureTile);
            }
        }
    }
}

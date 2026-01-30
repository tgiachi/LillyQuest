using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Events;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.GameObjects.Base;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Rendering;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace LillyQuest.RogueLike.Systems;

public sealed class MapRenderSystem : GameEntity, IUpdateableEntity, IMapAwareSystem, IMapHandler
{
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapRenderState> _states = new();
    private TilesetSurfaceScreen? _screen;
    private FovSystem? _fovSystem;

    public MapRenderSystem(int chunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        _chunkSize = chunkSize;
        Name = nameof(MapRenderSystem);
    }

    private sealed record MapRenderState(
        LyQuestMap Map,
        TilesetSurfaceScreen Surface,
        FovSystem? FovSystem,
        DirtyChunkTracker DirtyTracker,
        Dictionary<BaseGameObject, Action<BaseGameObject>> TileChangedHandlers
    );

    public void Configure(TilesetSurfaceScreen screen, FovSystem? fovSystem)
    {
        _screen = screen;
        _fovSystem = fovSystem;
    }

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

    public void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap)
    {
        if (oldMap != null)
        {
            UnregisterMap(oldMap);
        }

        OnMapRegistered(newMap);
    }

    public void OnMapRegistered(LyQuestMap map)
    {
        if (_screen == null)
        {
            throw new InvalidOperationException("MapRenderSystem.Configure must be called before registering a map.");
        }

        RegisterMap(map, _screen, _fovSystem);
    }

    public void OnMapUnregistered(LyQuestMap map)
        => UnregisterMap(map);

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen surface, FovSystem? fovSystem)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        var state = new MapRenderState(
            map,
            surface,
            fovSystem,
            new(_chunkSize),
            new()
        );
        _states[map] = state;

        map.ObjectMoved += (_, args) => HandleObjectMoved(map, args.OldPosition, args.NewPosition);
        map.ObjectAdded += (_, args) => HandleObjectMoved(map, args.Item.Position, args.Item.Position);
        map.ObjectRemoved += (_, args) => HandleObjectMoved(map, args.Item.Position, args.Item.Position);

        map.ObjectAdded += (_, args) =>
                           {
                               if (args.Item is BaseGameObject baseGameObject)
                               {
                                   SubscribeToTileChanges(state, baseGameObject);
                               }
                           };
        map.ObjectRemoved += (_, args) =>
                             {
                                 if (args.Item is BaseGameObject baseGameObject)
                                 {
                                     UnsubscribeFromTileChanges(state, baseGameObject);
                                 }
                             };

        // Subscribe to FOV updates
        if (fovSystem != null)
        {
            fovSystem.FovUpdated += OnFovUpdated;
        }

        SubscribeToExistingEntities(map, state);
        SubscribeToExistingTerrain(map, state);
    }

    public void UnregisterMap(LyQuestMap map)
    {
        if (_states.Remove(map, out var state))
        {
            // Unsubscribe from FOV updates
            if (state.FovSystem != null)
            {
                state.FovSystem.FovUpdated -= OnFovUpdated;
            }

            foreach (var handler in state.TileChangedHandlers)
            {
                handler.Key.VisualTileChanged -= handler.Value;
            }

            state.TileChangedHandlers.Clear();
        }
    }

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
        FovSystem? fovSystem,
        Point position
    )
    {
        var renderCreature = fovSystem == null || fovSystem.IsVisible(map, position);
        var empty = new TileRenderData(-1, LyColor.White);

        if (!renderCreature)
        {
            return empty;
        }

        foreach (var obj in map.GetObjectsAt(position))
        {
            if (obj is CreatureGameObject creature)
            {
                var tile = new TileRenderData(
                    creature.Tile.Symbol[0],
                    creature.Tile.ForegroundColor,
                    creature.Tile.BackgroundColor,
                    creature.Tile.Flip
                );

                if (fovSystem != null)
                {
                    tile = tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
                }

                return tile;
            }
        }

        return empty;
    }

    private static TileRenderData BuildItemTile(
        LyQuestMap map,
        FovSystem? fovSystem,
        Point position
    )
    {
        var renderItem = fovSystem == null || fovSystem.IsVisible(map, position);
        var empty = new TileRenderData(-1, LyColor.White);

        if (!renderItem)
        {
            return empty;
        }

        foreach (var obj in map.GetObjectsAt(position))
        {
            if (obj is ItemGameObject item)
            {
                var tile = new TileRenderData(
                    item.Tile.Symbol[0],
                    item.Tile.ForegroundColor,
                    item.Tile.BackgroundColor,
                    item.Tile.Flip
                );

                if (fovSystem != null)
                {
                    tile = tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
                }

                return tile;
            }
        }

        return empty;
    }

    private static TileRenderData BuildTerrainTile(
        LyQuestMap map,
        FovSystem? fovSystem,
        Point position
    )
    {
        var tile = new TileRenderData(-1, LyColor.White);

        if (map.GetTerrainAt(position) is TerrainGameObject terrain)
        {
            tile = new(
                terrain.Tile.Symbol[0],
                terrain.Tile.ForegroundColor,
                terrain.Tile.BackgroundColor,
                terrain.Tile.Flip
            );
        }

        if (fovSystem == null)
        {
            return tile;
        }

        var isVisible = fovSystem.IsVisible(map, position);
        var isExplored = fovSystem.IsExplored(map, position);

        if (!isExplored)
        {
            return new(-1, LyColor.White);
        }

        if (!isVisible && isExplored)
        {
            return tile.Darken(0.5f);
        }

        return tile.Darken(fovSystem.GetVisibilityFalloff(map, position));
    }

    private void OnFovUpdated(object? sender, FovUpdatedEventArgs e)
    {
        if (!_states.TryGetValue(e.Map, out var state))
        {
            return;
        }

        // Mark previously visible tiles as dirty (they may need to be dimmed)
        foreach (var position in e.PreviousVisibleTiles)
        {
            state.DirtyTracker.MarkDirtyForTile(position.X, position.Y);
        }

        // Mark currently visible tiles as dirty (they need to be rendered)
        foreach (var position in e.CurrentVisibleTiles)
        {
            state.DirtyTracker.MarkDirtyForTile(position.X, position.Y);
        }
    }

    private void RebuildChunk(MapRenderState state, ChunkCoord chunk)
    {
        var map = state.Map;
        var surface = state.Surface;
        var fovSystem = state.FovSystem;
        var startX = chunk.X * _chunkSize;
        var startY = chunk.Y * _chunkSize;
        var endX = Math.Min(startX + _chunkSize, map.Width);
        var endY = Math.Min(startY + _chunkSize, map.Height);

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var position = new Point(x, y);
                var tile = BuildTerrainTile(map, fovSystem, position);
                surface.AddTileToSurface((int)MapLayer.Terrain, x, y, tile);

                var creatureTile = BuildCreatureTile(map, fovSystem, position);
                surface.AddTileToSurface((int)MapLayer.Creatures, x, y, creatureTile);

                var itemTile = BuildItemTile(map, fovSystem, position);
                surface.AddTileToSurface((int)MapLayer.Items, x, y, itemTile);
            }
        }
    }

    private void SubscribeToExistingEntities(LyQuestMap map, MapRenderState state)
    {
        foreach (var layer in map.Entities.Layers)
        {
            foreach (var entity in layer.Items)
            {
                if (entity is BaseGameObject baseGameObject)
                {
                    SubscribeToTileChanges(state, baseGameObject);
                }
            }
        }
    }

    private void SubscribeToExistingTerrain(LyQuestMap map, MapRenderState state)
    {
        foreach (var position in map.Positions())
        {
            if (map.GetTerrainAt(position) is BaseGameObject terrain)
            {
                SubscribeToTileChanges(state, terrain);
            }
        }
    }

    private void SubscribeToTileChanges(MapRenderState state, BaseGameObject baseGameObject)
    {
        if (state.TileChangedHandlers.ContainsKey(baseGameObject))
        {
            return;
        }

        void Handler(BaseGameObject obj)
            => state.DirtyTracker.MarkDirtyForTile(obj.Position.X, obj.Position.Y);

        state.TileChangedHandlers[baseGameObject] = Handler;
        baseGameObject.VisualTileChanged += Handler;
    }

    private void UnsubscribeFromTileChanges(MapRenderState state, BaseGameObject baseGameObject)
    {
        if (state.TileChangedHandlers.Remove(baseGameObject, out var handler))
        {
            baseGameObject.VisualTileChanged -= handler;
        }
    }
}

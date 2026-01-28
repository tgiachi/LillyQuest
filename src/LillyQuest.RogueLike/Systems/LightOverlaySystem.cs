using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Rendering;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Systems;

public sealed class LightOverlaySystem : GameEntity, IUpdateableEntity, IMapAwareSystem
{
    private const byte MaxBackgroundAlpha = 128;
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapState> _states = new();
    private readonly Dictionary<LyQuestMap, FovSystem?> _fovSystems = new();

    private sealed record MapState(
        LyQuestMap Map,
        TilesetSurfaceScreen Surface,
        DirtyChunkTracker DirtyTracker
    );

    public LightOverlaySystem(int chunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        _chunkSize = chunkSize;
        Name = nameof(LightOverlaySystem);
    }

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen surface, FovSystem? fovSystem)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        _states[map] = new MapState(map, surface, new DirtyChunkTracker(_chunkSize));
        _fovSystems[map] = fovSystem;
    }

    public void UnregisterMap(LyQuestMap map)
    {
        _states.Remove(map);
        _fovSystems.Remove(map);
    }

    public void MarkDirtyForRadius(LyQuestMap map, Point center, int radius)
    {
        if (!_states.TryGetValue(map, out var state))
        {
            return;
        }

        for (var y = center.Y - radius; y <= center.Y + radius; y++)
        {
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
                {
                    continue;
                }

                state.DirtyTracker.MarkDirtyForTile(x, y);
            }
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

    private void RebuildChunk(MapState state, ChunkCoord chunk)
    {
        var map = state.Map;
        var surface = state.Surface;
        var fovSystem = _fovSystems[map];
        var startX = chunk.X * _chunkSize;
        var startY = chunk.Y * _chunkSize;
        var endX = Math.Min(startX + _chunkSize, map.Width);
        var endY = Math.Min(startY + _chunkSize, map.Height);

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var position = new Point(x, y);
                var lightTile = BuildLightTile(map, fovSystem, position);
                surface.AddTileToSurface((int)MapLayer.Effects, x, y, lightTile);
            }
        }
    }

    private static TileRenderData BuildLightTile(
        LyQuestMap map,
        FovSystem? fovSystem,
        Point position
    )
    {
        if (fovSystem != null && !fovSystem.IsVisible(map, position))
        {
            return new TileRenderData(-1, LyColor.Transparent);
        }

        var overlayTileIndex = ResolveOverlayTileIndex(map, position);

        foreach (var layer in map.Entities.Layers)
        {
            foreach (var entity in layer.Items)
            {
                if (entity is not ItemGameObject item)
                {
                    continue;
                }

                var light = item.GoRogueComponents.GetFirstOrDefault<LightSourceComponent>();
                if (light == null)
                {
                    continue;
                }

                var distance = Distance.Euclidean.Calculate(item.Position, position);
                if (distance > light.Radius)
                {
                    continue;
                }

                var t = (float)(distance / light.Radius);
                var color = light.StartColor.Lerp(light.EndColor, t);
                var background = LyColor.Transparent;
                var backgroundComponent = item.GoRogueComponents.GetFirstOrDefault<LightBackgroundComponent>();
                if (backgroundComponent != null)
                {
                    var baseBackground = backgroundComponent.StartBackground.Lerp(backgroundComponent.EndBackground, t);
                    var targetAlpha = (byte)Math.Clamp((int)(MaxBackgroundAlpha * (1f - t)), 0, MaxBackgroundAlpha);
                    var finalAlpha = baseBackground.A < targetAlpha ? baseBackground.A : targetAlpha;
                    background = baseBackground.WithAlpha(finalAlpha);
                }

                return new TileRenderData(overlayTileIndex, color, background);
            }
        }

        return new TileRenderData(-1, LyColor.Transparent);
    }

    private static int ResolveOverlayTileIndex(LyQuestMap map, Point position)
    {
        if (map.GetTerrainAt(position) is TerrainGameObject terrain && !string.IsNullOrEmpty(terrain.Tile.Symbol))
        {
            return terrain.Tile.Symbol[0];
        }

        return 0;
    }
}

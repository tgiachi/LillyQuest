using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Game.Systems;

public sealed class LightOverlaySystem : GameEntity, IUpdateableEntity
{
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapState> _states = new();
    private readonly Dictionary<LyQuestMap, IFOVService?> _fovServices = new();

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

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen surface, IFOVService? fovService)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        _states[map] = new MapState(map, surface, new DirtyChunkTracker(_chunkSize));
        _fovServices[map] = fovService;
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
        var fovService = _fovServices[map];
        var startX = chunk.X * _chunkSize;
        var startY = chunk.Y * _chunkSize;
        var endX = Math.Min(startX + _chunkSize, map.Width);
        var endY = Math.Min(startY + _chunkSize, map.Height);

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var position = new Point(x, y);
                var lightTile = BuildLightTile(map, fovService, position);
                surface.AddTileToSurface((int)MapLayer.Effects, x, y, lightTile);
            }
        }
    }

    private static TileRenderData BuildLightTile(
        LyQuestMap map,
        IFOVService? fovService,
        Point position
    )
    {
        if (fovService != null && !fovService.IsVisible(position))
        {
            return new TileRenderData(-1, LyColor.Transparent);
        }

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
                var color = Lerp(light.StartColor, light.EndColor, t);
                return new TileRenderData(0, color);
            }
        }

        return new TileRenderData(-1, LyColor.Transparent);
    }

    private static LyColor Lerp(LyColor start, LyColor end, float t)
        => new(
            (byte)(start.A + (end.A - start.A) * t),
            (byte)(start.R + (end.R - start.R) * t),
            (byte)(start.G + (end.G - start.G) * t),
            (byte)(start.B + (end.B - start.B) * t)
        );
}

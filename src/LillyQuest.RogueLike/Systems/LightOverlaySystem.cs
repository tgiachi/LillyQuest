using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.Events;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Rendering;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Systems;

public sealed class LightOverlaySystem : GameEntity, IUpdateableEntity, IMapAwareSystem, IMapHandler
{
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapState> _states = new();
    private readonly Dictionary<LyQuestMap, FovSystem?> _fovSystems = new();
    private readonly Dictionary<LyQuestMap, List<ItemGameObject>> _cachedLightSources = new();
    private readonly TilesetSurfaceScreen _screen;
    private readonly FovSystem? _fovSystem;
    private readonly Dictionary<LyQuestMap, Dictionary<ItemGameObject, LightState>> _lightStates = new();

    private sealed record MapState(
        LyQuestMap Map,
        TilesetSurfaceScreen Surface,
        DirtyChunkTracker DirtyTracker
    );

    private sealed record LightState(
        float FlickerFactor,
        int EffectiveRadius,
        float LastFlickerFactor,
        int LastEffectiveRadius
    );

    public LightOverlaySystem(int chunkSize, TilesetSurfaceScreen screen, FovSystem? fovSystem)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        ArgumentNullException.ThrowIfNull(screen);

        _chunkSize = chunkSize;
        _screen = screen;
        _fovSystem = fovSystem;
        Name = nameof(LightOverlaySystem);
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

        _states[map] = new(map, surface, new(_chunkSize));
        _fovSystems[map] = fovSystem;
        _lightStates[map] = new();

        // Subscribe to FOV updates to mark light sources dirty when visibility changes
        if (fovSystem != null)
        {
            fovSystem.FovUpdated += OnFovUpdated;
        }

        // Subscribe to object removal to clean up light state when items are removed from map
        map.ObjectRemoved += (sender, args) =>
        {
            if (args.Item is ItemGameObject item && _lightStates.TryGetValue(map, out var states))
            {
                states.Remove(item);
            }
        };
    }

    public void UnregisterMap(LyQuestMap map)
    {
        // Unsubscribe from FOV updates
        if (_fovSystems.TryGetValue(map, out var fovSystem) && fovSystem != null)
        {
            fovSystem.FovUpdated -= OnFovUpdated;
        }

        _states.Remove(map);
        _fovSystems.Remove(map);
        _cachedLightSources.Remove(map);
        _lightStates.Remove(map);
    }

    public void Update(GameTime gameTime)
    {
        UpdateFlickerStates(gameTime);

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

    private static LyColor ApplyFlickerToBackground(LyColor baseColor, float factor)
    {
        var shift = Math.Clamp((factor - 1f) * 0.5f, -0.5f, 0.5f);

        return shift >= 0f
                   ? baseColor.Lerp(LyColor.White, shift)
                   : baseColor.Lerp(LyColor.Black, -shift);
    }

    private static LyColor ApplyFlickerToColor(LyColor start, LyColor end, float t, float factor)
    {
        var baseColor = start.Lerp(end, t);
        var shift = Math.Clamp((factor - 1f) * 0.5f, -0.5f, 0.5f);

        return shift >= 0f
                   ? baseColor.Lerp(LyColor.White, shift)
                   : baseColor.Lerp(LyColor.Black, -shift);
    }

    private TileRenderData BuildLightTile(
        LyQuestMap map,
        FovSystem? fovSystem,
        Point position
    )
    {
        if (fovSystem != null && !fovSystem.IsVisible(map, position))
        {
            return new(-1, LyColor.Transparent);
        }

        var overlayTileIndex = ResolveOverlayTileIndex(map, position);
        var lightStatesForMap = _lightStates.TryGetValue(map, out var states) ? states : new();

        foreach (var layer in map.Entities.Layers)
        {
            foreach (var entity in layer.Items)
            {
                if (entity is not ItemGameObject item)
                {
                    continue;
                }

                var torch = item.GoRogueComponents.GetFirstOrDefault<TorchComponent>();

                if (torch == null)
                {
                    continue;
                }

                var flicker = item.GoRogueComponents.GetFirstOrDefault<LightFlickerComponent>();
                var lightState = flicker != null && lightStatesForMap.TryGetValue(item, out var state)
                                     ? state
                                     : null;
                var flickerFactor = lightState?.FlickerFactor ?? 1f;
                var effectiveRadius = lightState?.EffectiveRadius ?? torch.Radius;

                var distance = Distance.Euclidean.Calculate(item.Position, position);

                if (distance > effectiveRadius)
                {
                    continue;
                }

                var t = (float)(distance / effectiveRadius);
                var t2 = t * t;
                var color = ApplyFlickerToColor(torch.ForegroundStart, torch.ForegroundEnd, t2, flickerFactor);
                var background = torch.BackgroundStart.Lerp(torch.BackgroundEnd, t2);
                background = ApplyFlickerToBackground(background, flickerFactor);
                var alpha = (byte)Math.Clamp((int)(torch.BackgroundAlpha * (1f - t2)), 0, torch.BackgroundAlpha);
                background = background.WithAlpha(alpha);

                if (map.GetTerrainAt(position) is TerrainGameObject terrain && !terrain.IsTransparent)
                {
                    color = color.Lerp(terrain.Tile.ForegroundColor, 0.5f);
                    var terrainBackground = terrain.Tile.BackgroundColor ?? LyColor.Transparent;
                    background = background.Lerp(terrainBackground, 0.5f);
                }

                return new(overlayTileIndex, color, background);
            }
        }

        return new(-1, LyColor.Transparent);
    }

    private static float CalculateFlickerFactor(ItemGameObject item, LightFlickerComponent flicker, double timeSeconds)
    {
        var intensity = Math.Clamp(flicker.Intensity, 0f, 1f);

        if (intensity <= 0f || flicker.FrequencyHz <= 0f)
        {
            return 1f;
        }

        var seed = flicker.Seed ?? item.GetHashCode();
        var baseTime = timeSeconds * flicker.FrequencyHz;

        if (flicker.Mode == LightFlickerMode.Deterministic)
        {
            var value = MathF.Sin((float)(baseTime + seed)) * 0.5f + 0.5f;

            return 1f - intensity + 2f * intensity * value;
        }

        // Use deterministic noise instead of creating Random() every frame
        // This avoids allocation overhead while maintaining pseudo-random appearance
        var bucket = (int)Math.Floor(baseTime);
        var combinedSeed = unchecked(seed * 73856093 ^ bucket * 19349663);

        // Simple LCG (Linear Congruential Generator) for deterministic pseudo-randomness
        var lcg = unchecked((combinedSeed * 1103515245 + 12345) & 0x7fffffff);
        var valueRandom = (float)lcg / 0x7fffffff;

        return 1f - intensity + 2f * intensity * valueRandom;
    }

    private static int GetEffectiveRadius(TorchComponent torch, LightFlickerComponent? flicker, float factor)
    {
        if (flicker == null || flicker.RadiusJitter <= 0f)
        {
            return torch.Radius;
        }

        var delta = flicker.RadiusJitter * (factor - 1f);
        var radius = (int)MathF.Round(torch.Radius + delta);

        return Math.Max(1, radius);
    }

    private void MarkAllLightSourcesDirty(LyQuestMap map)
    {
        // Use cached light sources instead of iterating all entities every time
        if (!_cachedLightSources.TryGetValue(map, out var lightSources))
        {
            return;
        }

        foreach (var item in lightSources)
        {
            var torch = item.GoRogueComponents.GetFirstOrDefault<TorchComponent>();

            if (torch != null)
            {
                MarkDirtyForRadius(map, item.Position, torch.Radius);
            }
        }
    }

    private void OnFovUpdated(object? sender, FovUpdatedEventArgs e)
    {
        if (!_states.TryGetValue(e.Map, out _))
        {
            return;
        }

        // Mark all light sources as dirty when FOV changes
        MarkAllLightSourcesDirty(e.Map);
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

    private static int ResolveOverlayTileIndex(LyQuestMap map, Point position)
    {
        if (map.GetTerrainAt(position) is TerrainGameObject terrain && !string.IsNullOrEmpty(terrain.Tile.Symbol))
        {
            return terrain.Tile.Symbol[0];
        }

        return 0;
    }

    private void UpdateFlickerStates(GameTime gameTime)
    {
        foreach (var state in _states.Values)
        {
            var map = state.Map;
            var lightSources = new List<ItemGameObject>();
            var lightStatesForMap = _lightStates[map];

            foreach (var layer in map.Entities.Layers)
            {
                foreach (var entity in layer.Items)
                {
                    if (entity is not ItemGameObject item)
                    {
                        continue;
                    }

                    var torch = item.GoRogueComponents.GetFirstOrDefault<TorchComponent>();

                    if (torch == null)
                    {
                        continue;
                    }

                    // Cache this light source for future FOV updates
                    lightSources.Add(item);

                    var flicker = item.GoRogueComponents.GetFirstOrDefault<LightFlickerComponent>();

                    if (flicker == null)
                    {
                        continue;
                    }

                    var factor = CalculateFlickerFactor(item, flicker, gameTime.TotalGameTime.TotalSeconds);
                    var radius = GetEffectiveRadius(torch, flicker, factor);

                    if (lightStatesForMap.TryGetValue(item, out var lastState))
                    {
                        if (Math.Abs(factor - lastState.LastFlickerFactor) < 0.0001f && radius == lastState.LastEffectiveRadius)
                        {
                            continue;
                        }
                    }

                    lightStatesForMap[item] = new(factor, radius, factor, radius);

                    var maxRadius = torch.Radius + (int)MathF.Ceiling(MathF.Max(0f, flicker.RadiusJitter));
                    MarkDirtyForRadius(map, item.Position, maxRadius);
                }
            }

            // Update cache of light sources for this map
            _cachedLightSources[map] = lightSources;
        }
    }
}

using GoRogue.FOV;
using LillyQuest.Engine.Entities;
using LillyQuest.RogueLike.Data.Tiles;
using LillyQuest.RogueLike.Events;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Systems;

/// <summary>
/// System that manages field of view and fog of war using GoRogue's shadowcasting algorithm.
/// </summary>
public sealed class FovSystem : GameEntity, IMapAwareSystem, IMapHandler
{
    private const int DefaultFovRadius = 10;

    private readonly int _fovRadius;
    private readonly Dictionary<LyQuestMap, FovState> _states = new();

    /// <summary>
    /// Raised when the field of view has been updated.
    /// </summary>
    public event EventHandler<FovUpdatedEventArgs>? FovUpdated;

    public FovSystem(int fovRadius = DefaultFovRadius)
    {
        _fovRadius = fovRadius;
        Name = nameof(FovSystem);
    }

    private sealed class FovState
    {
        public LyQuestMap Map { get; }
        public RecursiveShadowcastingFOV Fov { get; }
        public HashSet<Point> CurrentVisibleTiles { get; } = new();
        public HashSet<Point> ExploredTiles { get; } = new();
        public Dictionary<Point, TileMemory> TileMemory { get; } = new();
        public Dictionary<Point, float> VisibilityFalloff { get; } = new();
        public Point LastViewerPosition { get; set; } = new(-1, -1);

        public FovState(LyQuestMap map, RecursiveShadowcastingFOV fov)
        {
            Map = map;
            Fov = fov;
        }
    }

    /// <summary>
    /// Tiles currently visible on the specified map.
    /// </summary>
    public IReadOnlySet<Point> GetCurrentVisibleTiles(LyQuestMap map)
        => _states.TryGetValue(map, out var state)
               ? state.CurrentVisibleTiles
               : new();

    /// <summary>
    /// Tiles that have been explored on the specified map.
    /// </summary>
    public IReadOnlySet<Point> GetExploredTiles(LyQuestMap map)
        => _states.TryGetValue(map, out var state)
               ? state.ExploredTiles
               : new();

    /// <summary>
    /// Get the memorized tile data for an explored position.
    /// </summary>
    public TileMemory? GetMemorizedTile(LyQuestMap map, Point position)
        => _states.TryGetValue(map, out var state) && state.TileMemory.TryGetValue(position, out var memory)
               ? memory
               : null;

    /// <summary>
    /// Check if a position has been explored on the specified map.
    /// </summary>
    public bool IsExplored(LyQuestMap map, Point position)
        => _states.TryGetValue(map, out var state) && state.ExploredTiles.Contains(position);

    /// <summary>
    /// Check if a position is currently visible on the specified map.
    /// </summary>
    public bool IsVisible(LyQuestMap map, Point position)
        => _states.TryGetValue(map, out var state) && state.CurrentVisibleTiles.Contains(position);

    /// <summary>
    /// Get the visibility falloff factor for a visible tile. Returns 1 for full intensity.
    /// </summary>
    public float GetVisibilityFalloff(LyQuestMap map, Point position)
        => _states.TryGetValue(map, out var state) && state.VisibilityFalloff.TryGetValue(position, out var falloff)
               ? falloff
               : 1f;

    /// <summary>
    /// Store visual information about a tile for fog of war display.
    /// </summary>
    public void MemorizeTile(LyQuestMap map, Point position, char symbol, Color foreground, Color background)
    {
        if (_states.TryGetValue(map, out var state))
        {
            state.TileMemory[position] = new(symbol, foreground, background);
        }
    }

    public void RegisterMap(LyQuestMap map)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        var fov = new RecursiveShadowcastingFOV(map.TransparencyView);
        _states[map] = new(map, fov);
    }

    public void UnregisterMap(LyQuestMap map)
    {
        _states.Remove(map);
    }

    /// <summary>
    /// Recalculate FOV from the given position using shadowcasting.
    /// </summary>
    public void UpdateFov(LyQuestMap map, Point viewerPosition)
    {
        if (!_states.TryGetValue(map, out var state))
        {
            return;
        }

        // Validate position is within bounds
        if (viewerPosition.X < 0 ||
            viewerPosition.X >= map.Width ||
            viewerPosition.Y < 0 ||
            viewerPosition.Y >= map.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(viewerPosition),
                $"Position {viewerPosition} is outside map bounds ({map.Width}x{map.Height})"
            );
        }

        // Skip recalculation if viewer hasn't moved
        if (state.LastViewerPosition == viewerPosition)
        {
            return;
        }

        // Capture previous visible tiles before update
        var previousVisibleTiles = new HashSet<Point>(state.CurrentVisibleTiles);

        // Calculate FOV from viewer position using circular radius
        state.Fov.Calculate(viewerPosition, _fovRadius, Distance.Euclidean);

        // Update current visible tiles
        state.CurrentVisibleTiles.Clear();
        state.VisibilityFalloff.Clear();

        foreach (var pos in state.Fov.CurrentFOV)
        {
            state.CurrentVisibleTiles.Add(pos);
        }

        foreach (var pos in state.CurrentVisibleTiles)
        {
            var distance = Distance.Euclidean.Calculate(viewerPosition, pos);
            state.VisibilityFalloff[pos] = CalculateFalloff(distance, _fovRadius);
        }

        // Mark all visible tiles as explored
        foreach (var visiblePos in state.CurrentVisibleTiles)
        {
            state.ExploredTiles.Add(visiblePos);
        }

        state.LastViewerPosition = viewerPosition;

        // Raise event to notify listeners
        FovUpdated?.Invoke(this, new(map, previousVisibleTiles, state.CurrentVisibleTiles));
    }

    public void OnMapRegistered(LyQuestMap map)
        => RegisterMap(map);

    public void OnMapUnregistered(LyQuestMap map)
        => UnregisterMap(map);

    public void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap)
    {
        if (oldMap != null)
        {
            UnregisterMap(oldMap);
        }

        RegisterMap(newMap);
    }

    private static float CalculateFalloff(double distance, int radius)
    {
        if (distance <= radius - 3)
        {
            return 1f;
        }

        if (distance <= radius - 2)
        {
            return 0.75f;
        }

        if (distance <= radius - 1)
        {
            return 0.5f;
        }

        return 0.25f;
    }
}

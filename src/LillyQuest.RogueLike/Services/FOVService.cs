using GoRogue.FOV;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Services;

/// <summary>
/// Manages field of view and fog of war using GoRogue's shadowcasting algorithm.
/// </summary>
public class FOVService : IFOVService
{
    private const int FOV_RADIUS = 10;

    private LyQuestMap _map;
    private RecursiveShadowcastingFOV _fov;
    private HashSet<Point> _currentVisibleTiles = new();
    private readonly HashSet<Point> _exploredTiles = new();
    private readonly Dictionary<Point, TileMemory> _tileMemory = new();
    private Point _lastPlayerPosition;

    public IReadOnlySet<Point> CurrentVisibleTiles => _currentVisibleTiles;
    public IReadOnlySet<Point> ExploredTiles => _exploredTiles;

    public TileMemory? GetMemorizedTile(Point position)
        => _tileMemory.TryGetValue(position, out var memory) ? memory : null;

    public void Initialize(LyQuestMap map)
    {
        _map = map;

        // Initialize FOV with map's transparency grid
        _fov = new(map.TransparencyView);
        _lastPlayerPosition = new(-1, -1);
    }

    public bool IsExplored(Point position)
        => _exploredTiles.Contains(position);

    public bool IsVisible(Point position)
        => _currentVisibleTiles.Contains(position);

    /// <summary>
    /// Store visual information about a tile for fog of war display.
    /// Called by rendering system when a tile is explored.
    /// </summary>
    public void MemorializeTile(Point position, char symbol, Color foreground, Color background)
    {
        _tileMemory[position] = new(symbol, foreground, background);
    }

    public void UpdateFOV(Point playerPosition)
    {
        // Validate position is within bounds
        if (playerPosition.X < 0 ||
            playerPosition.X >= _map.Width ||
            playerPosition.Y < 0 ||
            playerPosition.Y >= _map.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(playerPosition),
                $"Position {playerPosition} is outside map bounds ({_map.Width}x{_map.Height})"
            );
        }

        // Skip recalculation if player hasn't moved
        if (_lastPlayerPosition == playerPosition)
        {
            return;
        }

        // Calculate FOV from player position using circular radius
        _fov.Calculate(playerPosition, FOV_RADIUS, Distance.Euclidean);

        // Update current visible tiles
        _currentVisibleTiles = new(_fov.CurrentFOV);

        // Mark all visible tiles as explored
        foreach (var visiblePos in _currentVisibleTiles)
        {
            _exploredTiles.Add(visiblePos);
        }

        _lastPlayerPosition = playerPosition;
    }
}

using System;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Interfaces.Services;

/// <summary>
/// Service for managing Field of View and fog of war visibility.
/// </summary>
public interface IFOVService
{
    /// <summary>
    /// Tiles currently visible to the player (within FOV radius).
    /// </summary>
    IReadOnlySet<Point> CurrentVisibleTiles { get; }

    /// <summary>
    /// Tiles that have been explored (ever been within FOV).
    /// </summary>
    IReadOnlySet<Point> ExploredTiles { get; }

    /// <summary>
    /// Recalculate FOV from the given position using shadowcasting.
    /// </summary>
    void UpdateFOV(Point playerPosition);

    /// <summary>
    /// Check if a position is currently visible (in FOV range).
    /// </summary>
    bool IsVisible(Point position);

    /// <summary>
    /// Check if a position has been explored (ever been in FOV).
    /// </summary>
    bool IsExplored(Point position);

    /// <summary>
    /// Get the memorized tile data for an explored position.
    /// </summary>
    TileMemory? GetMemorizedTile(Point position);
}

/// <summary>
/// Stores visual information about a tile for fog of war display.
/// </summary>
public record TileMemory(char Symbol, Color ForegroundColor, Color BackgroundColor);

using System.Numerics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Handles tile movement animations for a tileset surface.
/// </summary>
internal sealed class TilesetSurfaceAnimator
{
    private readonly TilesetSurface _surface;

    public TilesetSurfaceAnimator(TilesetSurface surface)
        => _surface = surface;

    /// <summary>
    /// Enqueues a tile movement for the specified layer.
    /// </summary>
    public bool EnqueueMove(
        int layerIndex,
        Vector2 source,
        Vector2 destination,
        float durationSeconds,
        bool bounce = false
    )
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return false;
        }

        var sourceX = (int)source.X;
        var sourceY = (int)source.Y;

        if (sourceX < 0 || sourceX >= _surface.Width || sourceY < 0 || sourceY >= _surface.Height)
        {
            return false;
        }

        var destinationX = (int)destination.X;
        var destinationY = (int)destination.Y;

        if (destinationX < 0 || destinationX >= _surface.Width || destinationY < 0 || destinationY >= _surface.Height)
        {
            return false;
        }

        var tile = _surface.GetTile(layerIndex, sourceX, sourceY);

        if (tile.TileIndex < 0)
        {
            return false;
        }

        var movement = new TileMovement(
            layerIndex,
            new(sourceX, sourceY),
            new(destinationX, destinationY),
            bounce,
            durationSeconds,
            tile
        );

        _surface.Layers[layerIndex].Movements.Pending.Enqueue(movement);

        return true;
    }

    /// <summary>
    /// Gets the active movements for a specific layer.
    /// </summary>
    public IReadOnlyList<TileMovement> GetActiveMovements(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return Array.Empty<TileMovement>();
        }

        return _surface.Layers[layerIndex].Movements.Active;
    }

    /// <summary>
    /// Gets the movement queue for a specific layer.
    /// </summary>
    public TileMovementQueue? GetLayerMovementQueue(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return null;
        }

        return _surface.Layers[layerIndex].Movements;
    }

    /// <summary>
    /// Gets the interpolated tile position for a movement based on its elapsed time.
    /// </summary>
    public static Vector2 GetMovementTilePosition(TileMovement movement)
    {
        var duration = movement.DurationSeconds <= 0f ? 0.0001f : movement.DurationSeconds;
        var progress = Math.Clamp(movement.ElapsedSeconds / duration, 0f, 1f);

        if (movement.Bounce && movement.State == TileMovementState.Returning)
        {
            return Vector2.Lerp(movement.DestinationTile, movement.SourceTile, progress);
        }

        return Vector2.Lerp(movement.SourceTile, movement.DestinationTile, progress);
    }

    /// <summary>
    /// Advances active tile movements and places tiles when complete.
    /// </summary>
    public void ProcessMovements(GameTime gameTime)
    {
        if (gameTime.Elapsed.TotalSeconds <= 0)
        {
            return;
        }

        var dt = (float)gameTime.Elapsed.TotalSeconds;

        for (var layerIndex = 0; layerIndex < _surface.Layers.Count; layerIndex++)
        {
            var layer = _surface.Layers[layerIndex];
            var active = layer.Movements.Active;
            var pending = layer.Movements.Pending;

            while (pending.Count > 0)
            {
                var movement = pending.Dequeue();
                var sourceX = (int)movement.SourceTile.X;
                var sourceY = (int)movement.SourceTile.Y;
                var destinationX = (int)movement.DestinationTile.X;
                var destinationY = (int)movement.DestinationTile.Y;

                var sourceTile = _surface.GetTile(layerIndex, sourceX, sourceY);

                if (sourceTile.TileIndex < 0)
                {
                    continue;
                }

                movement.DestinationTileData = _surface.GetTile(layerIndex, destinationX, destinationY);
                _surface.SetTile(layerIndex, destinationX, destinationY, new(-1, LyColor.White));
                _surface.SetTile(layerIndex, sourceX, sourceY, new(-1, LyColor.White));
                movement.State = TileMovementState.Active;
                active.Add(movement);
            }

            for (var i = active.Count - 1; i >= 0; i--)
            {
                var movement = active[i];
                movement.ElapsedSeconds += dt;

                var duration = movement.DurationSeconds <= 0f ? 0.0001f : movement.DurationSeconds;

                if (movement.ElapsedSeconds < duration)
                {
                    continue;
                }

                if (movement.Bounce)
                {
                    if (movement.State == TileMovementState.Active)
                    {
                        movement.State = TileMovementState.Returning;
                        movement.ElapsedSeconds = 0f;

                        continue;
                    }

                    var sourceX = (int)movement.SourceTile.X;
                    var sourceY = (int)movement.SourceTile.Y;
                    _surface.SetTile(layerIndex, sourceX, sourceY, movement.TileData);
                    _surface.SetTile(
                        layerIndex,
                        (int)movement.DestinationTile.X,
                        (int)movement.DestinationTile.Y,
                        movement.DestinationTileData
                    );
                    movement.State = TileMovementState.Completed;
                    active.RemoveAt(i);

                    continue;
                }

                var destX = (int)movement.DestinationTile.X;
                var destY = (int)movement.DestinationTile.Y;
                _surface.SetTile(layerIndex, destX, destY, movement.TileData);
                movement.State = TileMovementState.Completed;
                active.RemoveAt(i);
            }
        }
    }
}

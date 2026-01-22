using System.Numerics;

namespace LillyQuest.Engine.Extensions.TilesetSurface;

/// <summary>
/// Math helpers for tileset surface calculations.
/// </summary>
public static class TilesetSurfaceMathExtensions
{
    /// <param name="tileViewSize">Tile view size (columns, rows).</param>
    extension(Vector2 tileViewSize)
    {
        /// <summary>
        /// Computes the screen size from a tile view size, preserving the current size signature.
        /// </summary>
        /// <param name="currentSize">Current size (unused).</param>
        /// <param name="tileWidth">Tile width in pixels.</param>
        /// <param name="tileHeight">Tile height in pixels.</param>
        /// <param name="tileRenderScale">Tile render scale.</param>
        public Vector2 ApplyTileViewSize(
            Vector2 currentSize,
            int tileWidth,
            int tileHeight,
            float tileRenderScale
        )
        {
            _ = currentSize;

            return tileViewSize.ToScreenSize(tileWidth, tileHeight, tileRenderScale);
        }

        /// <summary>
        /// Converts a tile view size into screen size in pixels.
        /// </summary>
        /// <param name="tileWidth">Tile width in pixels.</param>
        /// <param name="tileHeight">Tile height in pixels.</param>
        /// <param name="tileRenderScale">Tile render scale.</param>
        public Vector2 ToScreenSize(
            int tileWidth,
            int tileHeight,
            float tileRenderScale
        )
            => new(
                tileViewSize.X * tileWidth * tileRenderScale,
                tileViewSize.Y * tileHeight * tileRenderScale
            );
    }
}

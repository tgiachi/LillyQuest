using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Core.Graphics.Extensions;

/// <summary>
/// Extension methods for SpriteBatch to simplify tile drawing from tilesets.
/// </summary>
public static class SpriteBatchTileExtensions
{
    /// <param name="spriteBatch">The sprite batch instance</param>
    extension(SpriteBatch spriteBatch)
    {
        /// <summary>
        /// Draws a tile from a tileset at the specified position with native tile size.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileIndex">Linear index of the tile (0-based)</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="color">Color tint for the tile</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="origin">Origin for rotation/scaling (default: zero)</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTile(
            Tileset tileset,
            int tileIndex,
            Vector2 position,
            LyColor color,
            float rotation = 0f,
            Vector2? origin = null,
            float depth = 0f
        )
        {
            var tileData = tileset.GetTile(tileIndex);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                new(tileset.TileWidth, tileset.TileHeight),
                color,
                rotation,
                origin ?? Vector2.Zero,
                sourceRect,
                depth
            );
        }

        /// <summary>
        /// Draws a tile from a tileset at the specified grid coordinates with native tile size.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileX">Column of the tile in the tileset grid</param>
        /// <param name="tileY">Row of the tile in the tileset grid</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="color">Color tint for the tile</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="origin">Origin for rotation/scaling (default: zero)</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTile(
            Tileset tileset,
            int tileX,
            int tileY,
            Vector2 position,
            LyColor color,
            float rotation = 0f,
            Vector2? origin = null,
            float depth = 0f
        )
        {
            var tileData = tileset.GetTile(tileX, tileY);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                new(tileset.TileWidth, tileset.TileHeight),
                color,
                rotation,
                origin ?? Vector2.Zero,
                sourceRect,
                depth
            );
        }

        /// <summary>
        /// Draws a tile from a tileset at the specified position with custom size.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileIndex">Linear index of the tile (0-based)</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="size">Size to draw the tile at</param>
        /// <param name="color">Color tint for the tile</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="origin">Origin for rotation (default: zero)</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTile(
            Tileset tileset,
            int tileIndex,
            Vector2 position,
            Vector2 size,
            LyColor color,
            float rotation = 0f,
            Vector2? origin = null,
            float depth = 0f
        )
        {
            var tileData = tileset.GetTile(tileIndex);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                size,
                color,
                rotation,
                origin ?? Vector2.Zero,
                sourceRect,
                depth
            );
        }

        /// <summary>
        /// Draws a tile from a tileset using grid coordinates with custom size.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileX">Column of the tile in the tileset grid</param>
        /// <param name="tileY">Row of the tile in the tileset grid</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="size">Size to draw the tile at</param>
        /// <param name="color">Color tint for the tile</param>
        /// <param name="rotation">Rotation in radians</param>
        /// <param name="origin">Origin for rotation (default: zero)</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTile(
            Tileset tileset,
            int tileX,
            int tileY,
            Vector2 position,
            Vector2 size,
            LyColor color,
            float rotation = 0f,
            Vector2? origin = null,
            float depth = 0f
        )
        {
            var tileData = tileset.GetTile(tileX, tileY);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                size,
                color,
                rotation,
                origin ?? Vector2.Zero,
                sourceRect,
                depth
            );
        }
        /// <summary>
        /// Draws a tile with background color and foreground tint.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileRenderData">Tile render data with background and foreground colors</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTileWithBackground(
            Tileset tileset,
            TileRenderData tileRenderData,
            Vector2 position,
            float depth = 0f
        )
        {
            // Draw background color (if not transparent)
            if (tileRenderData.BackgroundColor.A > 0)
            {
                var bgSize = new Vector2(tileset.TileWidth, tileset.TileHeight);
                DrawSolidRectangle(spriteBatch, position, bgSize, tileRenderData.BackgroundColor, depth);
            }

            var tileData = tileset.GetTile(tileRenderData.TileIndex);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);
            sourceRect = TileRenderData.ApplyFlip(sourceRect, tileRenderData.Flip);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                new Vector2(tileset.TileWidth, tileset.TileHeight),
                tileRenderData.ForegroundColor,
                0f,
                Vector2.Zero,
                sourceRect,
                depth + 0.001f
            );
        }

        /// <summary>
        /// Draws a tile with background color and foreground tint at custom size.
        /// </summary>
        /// <param name="tileset">The tileset to draw from</param>
        /// <param name="tileRenderData">Tile render data with background and foreground colors</param>
        /// <param name="position">World position to draw at</param>
        /// <param name="size">Size to draw the tile and background at</param>
        /// <param name="depth">Depth for layering</param>
        public void DrawTileWithBackground(
            Tileset tileset,
            TileRenderData tileRenderData,
            Vector2 position,
            Vector2 size,
            float depth = 0f
        )
        {
            // Draw background color (if not transparent)
            if (tileRenderData.BackgroundColor.A > 0)
            {
                DrawSolidRectangle(spriteBatch, position, size, tileRenderData.BackgroundColor, depth);
            }

            var tileData = tileset.GetTile(tileRenderData.TileIndex);
            var sourceRect = ConvertSourceRect(tileData.SourceRect);
            sourceRect = TileRenderData.ApplyFlip(sourceRect, tileRenderData.Flip);

            spriteBatch.Draw(
                tileset.Texture,
                position,
                size,
                tileRenderData.ForegroundColor,
                0f,
                Vector2.Zero,
                sourceRect,
                depth + 0.001f
            );
        }
    }

    /// <summary>
    /// Converts Rectangle<int> to Rectangle<float> for SpriteBatch compatibility.
    /// </summary>
    private static Rectangle<float> ConvertSourceRect(Rectangle<int> rect)
        => new(
            rect.Origin.X,
            rect.Origin.Y,
            rect.Size.X,
            rect.Size.Y
        );

    /// <summary>
    /// Draws a solid colored rectangle.
    /// </summary>
    private static void DrawSolidRectangle(
        SpriteBatch spriteBatch,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float depth
    )
    {
        spriteBatch.DrawRectangle(position, size, color, depth);
    }
}

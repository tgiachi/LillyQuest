using System.Text;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Engine.Extensions.TilesetSurface;

/// <summary>
/// Extensions for drawing text and shapes onto a tileset surface screen.
/// </summary>
public static class TilesetSurfaceTextExtensions
{
    /// <summary>
    /// CP437 line-drawing character codes.
    /// </summary>
    private const int BoxHorizontal = 196;
    private const int BoxVertical = 179;
    private const int BoxTopLeft = 218;
    private const int BoxTopRight = 191;
    private const int BoxBottomLeft = 192;
    private const int BoxBottomRight = 217;

    /// <summary>
    /// CP437 encoding used to map characters to tile indices.
    /// </summary>
    private static readonly Encoding Cp437 = CreateCp437Encoding();

    /// <param name="screen">Target surface screen.</param>
    extension(TilesetSurfaceScreen screen)
    {
        /// <summary>
        /// Writes text to the surface using CP437 character codes.
        /// Supports '\n' for new lines and '\r' is ignored.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="foregroundColor">Foreground color for glyphs.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void DrawText(
            string text,
            int startX,
            int startY,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var x = startX;
            var y = startY;

            foreach (var ch in text)
            {
                if (ch == '\n')
                {
                    y++;
                    x = startX;
                    continue;
                }

                if (ch == '\r')
                {
                    continue;
                }

                var tileIndex = ToCp437TileIndex(ch);
                var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);
                screen.AddTileToSurface(x, y, tileData);
                x++;
            }
        }

        /// <summary>
        /// Writes text using pixel coordinates (relative to the screen origin).
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="xPx">Pixel X.</param>
        /// <param name="yPx">Pixel Y.</param>
        /// <param name="foregroundColor">Foreground color for glyphs.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void DrawTextPixel(
            string text,
            int xPx,
            int yPx,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            if (!screen.TryGetLayerTileInfo(screen.SelectedLayerIndex, out var tileWidth, out var tileHeight, out var pixelOffset))
            {
                return;
            }

            screen.TryGetLayerViewOffsets(screen.SelectedLayerIndex, out var viewTileOffset, out var viewPixelOffset);

            var (tileX, tileY) = ComputeTileCoordinatesFromPixel(
                xPx,
                yPx,
                tileWidth,
                tileHeight,
                screen.TileRenderScale,
                pixelOffset,
                viewTileOffset,
                viewPixelOffset
            );

            screen.DrawText(text, tileX, tileY, foregroundColor, backgroundColor, flip);
        }

        /// <summary>
        /// Fills a rectangular area with the given tile.
        /// </summary>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="width">Width in tiles.</param>
        /// <param name="height">Height in tiles.</param>
        /// <param name="tileIndex">Tile index to place.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void FillRectangle(
            int startX,
            int startY,
            int width,
            int height,
            int tileIndex,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);

            for (var y = startY; y < startY + height; y++)
            {
                for (var x = startX; x < startX + width; x++)
                {
                    screen.AddTileToSurface(x, y, tileData);
                }
            }
        }

        /// <summary>
        /// Clears a rectangular area by setting tiles to empty (-1).
        /// </summary>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="width">Width in tiles.</param>
        /// <param name="height">Height in tiles.</param>
        public void ClearArea(int startX, int startY, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var tileData = new TileRenderData(-1, LyColor.White);

            for (var y = startY; y < startY + height; y++)
            {
                for (var x = startX; x < startX + width; x++)
                {
                    screen.AddTileToSurface(x, y, tileData);
                }
            }
        }

        /// <summary>
        /// Draws a line using Bresenham's algorithm.
        /// </summary>
        /// <param name="x0">Start X.</param>
        /// <param name="y0">Start Y.</param>
        /// <param name="x1">End X.</param>
        /// <param name="y1">End Y.</param>
        /// <param name="tileIndex">Tile index to draw.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void DrawLine(
            int x0,
            int y0,
            int x1,
            int y1,
            int tileIndex,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                screen.AddTileToSurface(x0, y0, tileData);

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Draws a rectangle border.
        /// </summary>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="width">Width in tiles.</param>
        /// <param name="height">Height in tiles.</param>
        /// <param name="tileIndex">Tile index to draw.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void DrawRectangle(
            int startX,
            int startY,
            int width,
            int height,
            int tileIndex,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var right = startX + width - 1;
            var bottom = startY + height - 1;

            screen.DrawLine(startX, startY, right, startY, tileIndex, foregroundColor, backgroundColor, flip);
            screen.DrawLine(startX, bottom, right, bottom, tileIndex, foregroundColor, backgroundColor, flip);
            screen.DrawLine(startX, startY, startX, bottom, tileIndex, foregroundColor, backgroundColor, flip);
            screen.DrawLine(right, startY, right, bottom, tileIndex, foregroundColor, backgroundColor, flip);
        }

        /// <summary>
        /// Draws a circle outline using the midpoint circle algorithm.
        /// </summary>
        /// <param name="centerX">Center X.</param>
        /// <param name="centerY">Center Y.</param>
        /// <param name="radius">Radius in tiles.</param>
        /// <param name="tileIndex">Tile index to draw.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void DrawCircle(
            int centerX,
            int centerY,
            int radius,
            int tileIndex,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            if (radius < 0)
            {
                return;
            }

            var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);
            var x = radius;
            var y = 0;
            var err = 0;

            while (x >= y)
            {
                PlotCirclePoints(screen, centerX, centerY, x, y, tileData);

                y++;
                if (err <= 0)
                {
                    err += 2 * y + 1;
                }
                else
                {
                    x--;
                    err += 2 * (y - x) + 1;
                }
            }
        }

        /// <summary>
        /// Flood-fills a region starting at the given tile.
        /// </summary>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="tileIndex">Replacement tile index.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <param name="flip">Optional flip for the foreground tile.</param>
        public void FloodFill(
            int startX,
            int startY,
            int tileIndex,
            LyColor foregroundColor,
            LyColor? backgroundColor = null,
            TileFlipType flip = TileFlipType.None
        )
        {
            var target = screen.GetTile(screen.SelectedLayerIndex, startX, startY);
            var replacement = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);

            if (TileMatches(target, replacement))
            {
                return;
            }

            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                var current = screen.GetTile(screen.SelectedLayerIndex, x, y);

                if (!TileMatches(current, target))
                {
                    continue;
                }

                screen.AddTileToSurface(x, y, replacement);

                queue.Enqueue((x + 1, y));
                queue.Enqueue((x - 1, y));
                queue.Enqueue((x, y + 1));
                queue.Enqueue((x, y - 1));
            }
        }

        /// <summary>
        /// Draws a CP437 line-drawing box (single-line).
        /// </summary>
        /// <param name="startX">Start tile X.</param>
        /// <param name="startY">Start tile Y.</param>
        /// <param name="width">Width in tiles.</param>
        /// <param name="height">Height in tiles.</param>
        /// <param name="foregroundColor">Foreground color for tiles.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        public void DrawBox(
            int startX,
            int startY,
            int width,
            int height,
            LyColor foregroundColor,
            LyColor? backgroundColor = null
        )
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var right = startX + width - 1;
            var bottom = startY + height - 1;

            screen.FillRectangle(startX, startY, width, 1, BoxHorizontal, foregroundColor, backgroundColor);
            screen.FillRectangle(startX, bottom, width, 1, BoxHorizontal, foregroundColor, backgroundColor);
            screen.FillRectangle(startX, startY, 1, height, BoxVertical, foregroundColor, backgroundColor);
            screen.FillRectangle(right, startY, 1, height, BoxVertical, foregroundColor, backgroundColor);

            screen.AddTileToSurface(startX, startY, new TileRenderData(BoxTopLeft, foregroundColor, backgroundColor));
            screen.AddTileToSurface(right, startY, new TileRenderData(BoxTopRight, foregroundColor, backgroundColor));
            screen.AddTileToSurface(startX, bottom, new TileRenderData(BoxBottomLeft, foregroundColor, backgroundColor));
            screen.AddTileToSurface(right, bottom, new TileRenderData(BoxBottomRight, foregroundColor, backgroundColor));
        }
    }

    /// <summary>
    /// Plots the eight symmetric points of a circle.
    /// </summary>
    private static void PlotCirclePoints(
        TilesetSurfaceScreen screen,
        int cx,
        int cy,
        int x,
        int y,
        TileRenderData tileData
    )
    {
        screen.AddTileToSurface(cx + x, cy + y, tileData);
        screen.AddTileToSurface(cx - x, cy + y, tileData);
        screen.AddTileToSurface(cx + x, cy - y, tileData);
        screen.AddTileToSurface(cx - x, cy - y, tileData);
        screen.AddTileToSurface(cx + y, cy + x, tileData);
        screen.AddTileToSurface(cx - y, cy + x, tileData);
        screen.AddTileToSurface(cx + y, cy - x, tileData);
        screen.AddTileToSurface(cx - y, cy - x, tileData);
    }

    /// <summary>
    /// Compares tiles for flood fill.
    /// </summary>
    private static bool TileMatches(TileRenderData a, TileRenderData b)
    {
        return a.TileIndex == b.TileIndex &&
               a.ForegroundColor == b.ForegroundColor &&
               a.BackgroundColor == b.BackgroundColor &&
               a.Flip == b.Flip;
    }

    /// <summary>
    /// Converts a Unicode char to a CP437 tile index.
    /// </summary>
    private static int ToCp437TileIndex(char ch)
    {
        Span<char> chars = stackalloc char[1];
        Span<byte> bytes = stackalloc byte[1];
        chars[0] = ch;
        var count = Cp437.GetBytes(chars, bytes);

        return count == 0 ? 0 : bytes[0];
    }

    /// <summary>
    /// Creates a CP437 encoding instance.
    /// </summary>
    private static Encoding CreateCp437Encoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(437);
    }

    /// <summary>
    /// Computes tile coordinates from pixel coordinates.
    /// </summary>
    public static (int x, int y) ComputeTileCoordinatesFromPixel(
        int xPx,
        int yPx,
        int tileWidth,
        int tileHeight,
        float tileRenderScale,
        System.Numerics.Vector2 pixelOffset,
        System.Numerics.Vector2 viewTileOffset,
        System.Numerics.Vector2 viewPixelOffset
    )
    {
        var scaledTileWidth = tileWidth * tileRenderScale;
        var scaledTileHeight = tileHeight * tileRenderScale;
        var viewOffsetPx = new System.Numerics.Vector2(
            viewTileOffset.X * scaledTileWidth,
            viewTileOffset.Y * scaledTileHeight
        ) + viewPixelOffset;

        var localX = xPx - pixelOffset.X + viewOffsetPx.X;
        var localY = yPx - pixelOffset.Y + viewOffsetPx.Y;

        return (
            (int)MathF.Floor(localX / scaledTileWidth),
            (int)MathF.Floor(localY / scaledTileHeight)
        );
    }
}

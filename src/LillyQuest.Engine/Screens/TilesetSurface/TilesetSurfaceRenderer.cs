using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Handles rendering logic for tileset surfaces.
/// </summary>
internal sealed class TilesetSurfaceRenderer
{
    private readonly TilesetSurface _surface;
    private readonly TilesetSurfaceRenderContext _context;
    private readonly Func<int, Tileset?> _getTileset;

    public TilesetSurfaceRenderer(
        TilesetSurface surface,
        TilesetSurfaceRenderContext context,
        Func<int, Tileset?> getTileset
    )
    {
        _surface = surface;
        _context = context;
        _getTileset = getTileset;
    }

    /// <summary>
    /// Calculates the visible tile range for frustum culling.
    /// </summary>
    public static (int minTileX, int minTileY, int maxTileX, int maxTileY) CalculateVisibleTileRange(
        float visibleX,
        float visibleY,
        float visibleWidth,
        float visibleHeight,
        float scaledTileWidth,
        float scaledTileHeight,
        int surfaceWidth,
        int surfaceHeight
    )
    {
        var minTileX = Math.Max(0, (int)MathF.Floor(visibleX / scaledTileWidth));
        var minTileY = Math.Max(0, (int)MathF.Floor(visibleY / scaledTileHeight));
        var maxTileX = Math.Min(surfaceWidth, (int)MathF.Floor((visibleX + visibleWidth) / scaledTileWidth) + 1);
        var maxTileY = Math.Min(surfaceHeight, (int)MathF.Floor((visibleY + visibleHeight) / scaledTileHeight) + 1);

        return (minTileX, minTileY, maxTileX, maxTileY);
    }

    /// <summary>
    /// Renders all visible layers.
    /// </summary>
    public void Render(SpriteBatch spriteBatch, TilesetSurfaceAnimator animator)
    {
        spriteBatch.DrawRectangle(_context.ScreenPosition, _context.ScreenSize, LyColor.Black);

        var scissorX = (int)(_context.ScreenPosition.X + _context.Margin.X);
        var scissorY = (int)(_context.ScreenPosition.Y + _context.Margin.Y);
        var scissorWidth = Math.Max(0, (int)(_context.ScreenSize.X - _context.Margin.X - _context.Margin.Z));
        var scissorHeight = Math.Max(0, (int)(_context.ScreenSize.Y - _context.Margin.Y - _context.Margin.W));

        spriteBatch.SetScissor(scissorX, scissorY, scissorWidth, scissorHeight);
        spriteBatch.PushTranslation(_context.ScreenPosition);

        for (var layerIndex = 0; layerIndex < _surface.Layers.Count; layerIndex++)
        {
            var layer = _surface.Layers[layerIndex];

            if (!layer.IsVisible)
            {
                continue;
            }

            RenderLayer(spriteBatch, layer, layerIndex, animator);
        }

        spriteBatch.PopTranslation();
        spriteBatch.DisableScissor();
    }

    private void RenderLayer(SpriteBatch spriteBatch, TileLayer layer, int layerIndex, TilesetSurfaceAnimator animator)
    {
        var tileset = _getTileset(layerIndex);

        if (tileset == null)
        {
            return;
        }

        var layerScale = layer.RenderScale;
        var scaledTileWidth = tileset.TileWidth * _context.TileRenderScale * layerScale;
        var scaledTileHeight = tileset.TileHeight * _context.TileRenderScale * layerScale;
        var layerOffset = layer.PixelOffset;
        var viewOffsetPx = new Vector2(
                               layer.ViewTileOffset.X * scaledTileWidth,
                               layer.ViewTileOffset.Y * scaledTileHeight
                           ) +
                           layer.ViewPixelOffset;

        var visibleX = _context.Margin.X - layerOffset.X + viewOffsetPx.X;
        var visibleY = _context.Margin.Y - layerOffset.Y + viewOffsetPx.Y;
        var visibleWidth = _context.ScreenSize.X - _context.Margin.X - _context.Margin.Z;
        var visibleHeight = _context.ScreenSize.Y - _context.Margin.Y - _context.Margin.W;

        var (minTileX, minTileY, maxTileX, maxTileY) = CalculateVisibleTileRange(
            visibleX, visibleY, visibleWidth, visibleHeight,
            scaledTileWidth, scaledTileHeight,
            _surface.Width, _surface.Height
        );

        if (minTileX >= maxTileX || minTileY >= maxTileY)
        {
            return;
        }

        RenderTileBackgrounds(spriteBatch, layer, minTileX, minTileY, maxTileX, maxTileY, scaledTileWidth, scaledTileHeight, viewOffsetPx, layerOffset);
        RenderTileForegrounds(spriteBatch, layer, tileset, minTileX, minTileY, maxTileX, maxTileY, scaledTileWidth, scaledTileHeight, viewOffsetPx, layerOffset);
        RenderActiveMovements(spriteBatch, layer, tileset, scaledTileWidth, scaledTileHeight, viewOffsetPx, layerOffset);
    }

    private void RenderTileBackgrounds(
        SpriteBatch spriteBatch,
        TileLayer layer,
        int minTileX, int minTileY, int maxTileX, int maxTileY,
        float scaledTileWidth, float scaledTileHeight,
        Vector2 viewOffsetPx, Vector2 layerOffset
    )
    {
        foreach (var (chunkX, chunkY, chunk) in layer.EnumerateChunksInRange(minTileX, minTileY, maxTileX - 1, maxTileY - 1))
        {
            var chunkBaseX = chunkX * TileChunk.Size;
            var chunkBaseY = chunkY * TileChunk.Size;
            var startX = Math.Max(minTileX, chunkBaseX);
            var startY = Math.Max(minTileY, chunkBaseY);
            var endX = Math.Min(maxTileX, chunkBaseX + TileChunk.Size);
            var endY = Math.Min(maxTileY, chunkBaseY + TileChunk.Size);

            for (var x = startX; x < endX; x++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var tileData = chunk.GetTile(x - chunkBaseX, y - chunkBaseY);

                    if (tileData.BackgroundColor.A == 0)
                    {
                        continue;
                    }

                    var position = new Vector2(x * scaledTileWidth, y * scaledTileHeight) - viewOffsetPx + layerOffset;
                    spriteBatch.DrawRectangle(position, new(scaledTileWidth, scaledTileHeight), tileData.BackgroundColor);
                }
            }
        }
    }

    private static void RenderTileForegrounds(
        SpriteBatch spriteBatch,
        TileLayer layer,
        Tileset tileset,
        int minTileX, int minTileY, int maxTileX, int maxTileY,
        float scaledTileWidth, float scaledTileHeight,
        Vector2 viewOffsetPx, Vector2 layerOffset
    )
    {
        foreach (var (chunkX, chunkY, chunk) in layer.EnumerateChunksInRange(minTileX, minTileY, maxTileX - 1, maxTileY - 1))
        {
            var chunkBaseX = chunkX * TileChunk.Size;
            var chunkBaseY = chunkY * TileChunk.Size;
            var startX = Math.Max(minTileX, chunkBaseX);
            var startY = Math.Max(minTileY, chunkBaseY);
            var endX = Math.Min(maxTileX, chunkBaseX + TileChunk.Size);
            var endY = Math.Min(maxTileY, chunkBaseY + TileChunk.Size);

            for (var x = startX; x < endX; x++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var tileData = chunk.GetTile(x - chunkBaseX, y - chunkBaseY);

                    if (tileData.TileIndex < 0 || tileData.TileIndex >= tileset.TileCount)
                    {
                        continue;
                    }

                    var tile = tileset.GetTile(tileData.TileIndex);
                    var uvX = (float)tile.SourceRect.Origin.X / tileset.Texture.Width;
                    var uvY = (float)tile.SourceRect.Origin.Y / tileset.Texture.Height;
                    var uvWidth = (float)tile.SourceRect.Size.X / tileset.Texture.Width;
                    var uvHeight = (float)tile.SourceRect.Size.Y / tileset.Texture.Height;

                    var sourceRect = new Rectangle<float>(uvX, uvY, uvWidth, uvHeight);
                    sourceRect = TileRenderData.ApplyFlip(sourceRect, tileData.Flip);

                    var color = tileData.ForegroundColor.WithAlpha((byte)(tileData.ForegroundColor.A * layer.Opacity));
                    var tilePosition = new Vector2(x * scaledTileWidth, y * scaledTileHeight) - viewOffsetPx + layerOffset;

                    spriteBatch.Draw(
                        tileset.Texture,
                        tilePosition,
                        new(scaledTileWidth, scaledTileHeight),
                        color,
                        0f,
                        Vector2.Zero,
                        sourceRect,
                        0f
                    );
                }
            }
        }
    }

    private static void RenderActiveMovements(
        SpriteBatch spriteBatch,
        TileLayer layer,
        Tileset tileset,
        float scaledTileWidth, float scaledTileHeight,
        Vector2 viewOffsetPx, Vector2 layerOffset
    )
    {
        var activeMovements = layer.Movements.Active;

        for (var i = 0; i < activeMovements.Count; i++)
        {
            var movement = activeMovements[i];
            var tileData = movement.TileData;

            if (tileData.TileIndex < 0 || tileData.TileIndex >= tileset.TileCount)
            {
                continue;
            }

            var tile = tileset.GetTile(tileData.TileIndex);
            var uvX = (float)tile.SourceRect.Origin.X / tileset.Texture.Width;
            var uvY = (float)tile.SourceRect.Origin.Y / tileset.Texture.Height;
            var uvWidth = (float)tile.SourceRect.Size.X / tileset.Texture.Width;
            var uvHeight = (float)tile.SourceRect.Size.Y / tileset.Texture.Height;

            var sourceRect = new Rectangle<float>(uvX, uvY, uvWidth, uvHeight);
            sourceRect = TileRenderData.ApplyFlip(sourceRect, tileData.Flip);

            var color = tileData.ForegroundColor.WithAlpha((byte)(tileData.ForegroundColor.A * layer.Opacity));
            var tilePosition = TilesetSurfaceAnimator.GetMovementTilePosition(movement);
            var pixelPosition = new Vector2(
                                    tilePosition.X * scaledTileWidth,
                                    tilePosition.Y * scaledTileHeight
                                ) -
                                viewOffsetPx +
                                layerOffset;

            spriteBatch.Draw(
                tileset.Texture,
                pixelPosition,
                new(scaledTileWidth, scaledTileHeight),
                color,
                0f,
                Vector2.Zero,
                sourceRect,
                0f
            );
        }
    }
}

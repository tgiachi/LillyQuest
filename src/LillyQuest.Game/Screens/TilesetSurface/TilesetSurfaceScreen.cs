using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Managers.Screens.Base;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LillyQuest.Game.Screens.TilesetSurface;

/// <summary>
/// Screen for drawing and editing tileset surfaces with multi-layer support.
/// </summary>
public class TilesetSurfaceScreen : BaseScreen
{
    private readonly ITilesetManager _tilesetManager;
    private TilesetSurface _surface;
    private Tileset _tileset;

    /// <summary>
    /// Number of layers to create on initialization.
    /// </summary>
    public int LayerCount { get; set; } = 3;

    /// <summary>
    /// Currently selected layer for drawing.
    /// </summary>
    public int SelectedLayerIndex { get; set; }

    /// <summary>
    /// Currently selected tile index to draw with.
    /// </summary>
    public int SelectedTileIndex { get; set; }

    /// <summary>
    /// Default tileset name used when a layer doesn't have a specific tileset assigned.
    /// </summary>
    public string DefaultTilesetName { get; set; } = "alloy";

    public TilesetSurfaceScreen(ITilesetManager tilesetManager)
    {
        _tilesetManager = tilesetManager;
        Size = new(800, 600);
        IsModal = false;
    }

    /// <summary>
    /// Gets the opacity of a specific layer.
    /// </summary>
    public float GetLayerOpacity(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return 1.0f;
        }

        return _surface.Layers[layerIndex].Opacity;
    }

    /// <summary>
    /// Gets the tileset name for a specific layer.
    /// Returns the layer's tileset name if set, otherwise returns the default.
    /// </summary>
    public string GetLayerTilesetName(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return DefaultTilesetName;
        }

        var layer = _surface.Layers[layerIndex];

        return string.IsNullOrEmpty(layer.TilesetName) ? DefaultTilesetName : layer.TilesetName;
    }

    public override void OnLoad()
    {
        _tileset = _tilesetManager.GetTileset(DefaultTilesetName);

        if (_tileset == null)
        {
            throw new InvalidOperationException($"Default tileset '{DefaultTilesetName}' not found");
        }

        _surface = new();
        _surface.Initialize(LayerCount);

        base.OnLoad();
    }

    public override bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (!HitTest(x, y))
        {
            return false;
        }

        // Only draw on left mouse button
        if (!buttons.Contains(MouseButton.Left))
        {
            return false;
        }

        var (tileX, tileY) = GetTileCoordinates(x, y);

        // Draw on the selected layer
        _surface.SetTile(SelectedLayerIndex, tileX, tileY, SelectedTileIndex);

        return true;
    }

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        // Draw black background
        spriteBatch.DrawRectangle(Position, Size, LyColor.Black);

        spriteBatch.PushTranslation(Position);

        // Render layers from bottom (0) to top (N-1)
        for (var layerIndex = 0; layerIndex < _surface.Layers.Count; layerIndex++)
        {
            var layer = _surface.Layers[layerIndex];

            if (!layer.IsVisible)
            {
                continue;
            }

            RenderLayer(spriteBatch, layer, layerIndex);
        }

        spriteBatch.PopTranslation();
    }

    /// <summary>
    /// Sets the opacity of a specific layer.
    /// </summary>
    public void SetLayerOpacity(int layerIndex, float opacity)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].Opacity = Math.Clamp(opacity, 0.0f, 1.0f);
    }

    /// <summary>
    /// Sets the tileset for a specific layer.
    /// If null or empty, the layer will use the default tileset.
    /// </summary>
    public void SetLayerTileset(int layerIndex, string? tilesetName)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].TilesetName = tilesetName;
    }

    /// <summary>
    /// Toggles visibility of a layer.
    /// </summary>
    public void ToggleLayerVisibility(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].IsVisible = !_surface.Layers[layerIndex].IsVisible;
    }

    /// <summary>
    /// Gets the tileset for a specific layer, with fallback to default.
    /// </summary>
    private Tileset? GetLayerTileset(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return null;
        }

        var layer = _surface.Layers[layerIndex];
        var tilesetNameToUse = string.IsNullOrEmpty(layer.TilesetName) ? DefaultTilesetName : layer.TilesetName;

        if (_tilesetManager.TryGetTileset(tilesetNameToUse, out var tileset))
        {
            return tileset;
        }

        // Fallback to default if the specified tileset doesn't exist
        if (_tilesetManager.TryGetTileset(DefaultTilesetName, out var defaultTileset))
        {
            return defaultTileset;
        }

        return null;
    }

    /// <summary>
    /// Converts mouse coordinates to tile coordinates based on the selected layer's tileset.
    /// </summary>
    private (int x, int y) GetTileCoordinates(int mouseX, int mouseY)
    {
        // Get the tileset of the selected layer
        var tileset = GetLayerTileset(SelectedLayerIndex);
        if (tileset == null)
        {
            return (0, 0);
        }

        // Adjust mouse position relative to screen position
        var relativeX = mouseX - (int)Position.X;
        var relativeY = mouseY - (int)Position.Y;

        // Calculate tile coordinates using the tileset's tile dimensions
        var tileX = relativeX / tileset.TileWidth;
        var tileY = relativeY / tileset.TileHeight;

        return (tileX, tileY);
    }

    /// <summary>
    /// Renders a single layer to the sprite batch.
    /// </summary>
    private void RenderLayer(SpriteBatch spriteBatch, TileLayer layer, int layerIndex)
    {
        // Get the tileset for this layer (with fallback to default)
        var tileset = GetLayerTileset(layerIndex);

        if (tileset == null)
        {
            return;
        }

        for (var x = 0; x < _surface.Width; x++)
        {
            for (var y = 0; y < _surface.Height; y++)
            {
                var tileIndex = layer.TileIndices[x, y];

                // Skip empty tiles
                if (tileIndex < 0 || tileIndex >= tileset.TileCount)
                {
                    continue;
                }

                // Get the tile from the tileset
                var tile = tileset.GetTile(tileIndex);

                // Convert pixel coordinates to normalized UV coordinates (0-1 range)
                var uvX = (float)tile.SourceRect.Origin.X / tileset.Texture.Width;
                var uvY = (float)tile.SourceRect.Origin.Y / tileset.Texture.Height;
                var uvWidth = (float)tile.SourceRect.Size.X / tileset.Texture.Width;
                var uvHeight = (float)tile.SourceRect.Size.Y / tileset.Texture.Height;

                var sourceRect = new Rectangle<float>(uvX, uvY, uvWidth, uvHeight);

                // Apply layer opacity to the color
                var color = LyColor.White.WithAlpha((byte)(255 * layer.Opacity));

                // Draw the tile at the correct position using the tileset's tile dimensions
                var position = new Vector2(x * tileset.TileWidth, y * tileset.TileHeight);
                spriteBatch.Draw(
                    tileset.Texture,
                    position,
                    new(tileset.TileWidth, tileset.TileHeight),
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

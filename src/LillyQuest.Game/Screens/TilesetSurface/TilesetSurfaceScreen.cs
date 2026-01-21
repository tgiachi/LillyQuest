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
    private readonly TilesetSurface _surface;

    private Tileset _tileset;

    // private int _autoRandomizeEveryFrames;
    // private int _autoRandomizeTileCount;
    // private int _autoRandomizeFrameCounter;
    // private Random _autoRandomizeRandom = Random.Shared;
    // private bool _autoRandomizeEnabled;

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

    private float _tileRenderScale = 1.0f;
    private Vector2 _tileViewSize = new(90, 30);

    /// <summary>
    /// Render scale for tiles (e.g., 2.0 scales 12x12 tiles to 24x24).
    /// </summary>
    public float TileRenderScale
    {
        get => _tileRenderScale;
        set
        {
            _tileRenderScale = value;
            UpdateScreenSizeFromTileView();
        }
    }

    /// <summary>
    /// Number of tiles visible in the screen (columns, rows).
    /// Used to compute Size from the default tileset.
    /// </summary>
    public Vector2 TileViewSize
    {
        get => _tileViewSize;
        set
        {
            _tileViewSize = value;
            UpdateScreenSizeFromTileView();
        }
    }

    public TilesetSurfaceScreen(ITilesetManager tilesetManager)
    {
        _tilesetManager = tilesetManager;

        // Display 90x30 tiles (like sadconsole's 90x30 character grid)
        // With 24px tiles: 90*24=2160w, 30*24=720h
        Size = new(2160, 720);
        IsModal = false;

        // Initialize surface early so it can be populated before OnLoad
        _surface = new();
    }

    /// <summary>
    /// Adds a tile to the surface at the specified coordinates on the selected layer.
    /// </summary>
    public void AddTileToSurface(int x, int y, TileRenderData tileData)
    {
        _surface.SetTile(SelectedLayerIndex, x, y, tileData);
    }

    // public void ConfigureAutoRandomize(int tileCount, int everyFrames, Random? random = null)
    // {
    //     _autoRandomizeTileCount = tileCount;
    //     _autoRandomizeEveryFrames = everyFrames;
    //     _autoRandomizeFrameCounter = 0;
    //     _autoRandomizeRandom = random ?? Random.Shared;
    //     _autoRandomizeEnabled = tileCount > 0 && everyFrames > 0;
    // }

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

    public TileRenderData GetTile(int layerIndex, int x, int y)
        => _surface.GetTile(layerIndex, x, y);

    /// <summary>
    /// Initializes the surface layers. Can be called before OnLoad to pre-populate.
    /// </summary>
    public void InitializeLayers(int layerCount)
    {
        _surface.Initialize(layerCount);
    }

    public override void OnLoad()
    {
        _tileset = _tilesetManager.GetTileset(DefaultTilesetName);

        if (_tileset == null)
        {
            throw new InvalidOperationException($"Default tileset '{DefaultTilesetName}' not found");
        }

        UpdateScreenSizeFromTileView();

        // Initialize layers if they haven't been initialized yet
        if (_surface.Layers.Count == 0)
        {
            _surface.Initialize(LayerCount);
        }

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

        // Create tile render data with the selected tile index and white color
        var tileData = new TileRenderData(SelectedTileIndex, LyColor.White);

        // Draw on the selected layer
        _surface.SetTile(SelectedLayerIndex, tileX, tileY, tileData);

        return true;
    }

    public override void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        // Draw black background
        spriteBatch.DrawRectangle(Position, Size, LyColor.Black);

        // Calculate scissor region with margins applied
        var scissorX = (int)(Position.X + Margin.X);
        var scissorY = (int)(Position.Y + Margin.Y);
        var scissorWidth = (int)(Size.X - Margin.X - Margin.Z);
        var scissorHeight = (int)(Size.Y - Margin.Y - Margin.W);

        // Ensure scissor dimensions are not negative
        scissorWidth = Math.Max(0, scissorWidth);
        scissorHeight = Math.Max(0, scissorHeight);

        spriteBatch.SetScissor(scissorX, scissorY, scissorWidth, scissorHeight);
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
        spriteBatch.DisableScissor();
    }

    public static Vector2 ComputeScreenSizeFromTileView(
        Vector2 tileViewSize,
        int tileWidth,
        int tileHeight,
        float tileRenderScale
    )
    {
        return new Vector2(
            tileViewSize.X * tileWidth * tileRenderScale,
            tileViewSize.Y * tileHeight * tileRenderScale
        );
    }

    public static Vector2 ApplyTileViewSize(
        Vector2 tileViewSize,
        Vector2 currentSize,
        int tileWidth,
        int tileHeight,
        float tileRenderScale
    )
    {
        return ComputeScreenSizeFromTileView(tileViewSize, tileWidth, tileHeight, tileRenderScale);
    }

    private void UpdateScreenSizeFromTileView()
    {
        if (_tileset == null)
        {
            return;
        }

        Size = ComputeScreenSizeFromTileView(_tileViewSize, _tileset.TileWidth, _tileset.TileHeight, _tileRenderScale);
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

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // if (!_autoRandomizeEnabled || _autoRandomizeEveryFrames <= 0 || _autoRandomizeTileCount <= 0)
        // {
        //     return;
        // }
        //
        // _autoRandomizeFrameCounter++;
        //
        // if (_autoRandomizeFrameCounter < _autoRandomizeEveryFrames)
        // {
        //     return;
        // }
        //
        // _autoRandomizeFrameCounter = 0;
        // RandomizeSelectedLayer();
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

        // Calculate scaled tile dimensions
        var scaledTileWidth = tileset.TileWidth * TileRenderScale;
        var scaledTileHeight = tileset.TileHeight * TileRenderScale;

        // Adjust mouse position relative to screen position
        var relativeX = mouseX - (int)Position.X;
        var relativeY = mouseY - (int)Position.Y;

        // Calculate tile coordinates using scaled tile dimensions
        var tileX = (int)(relativeX / scaledTileWidth);
        var tileY = (int)(relativeY / scaledTileHeight);

        return (tileX, tileY);
    }

    // private void RandomizeSelectedLayer()
    // {
    //     var random = _autoRandomizeRandom;
    //
    //     for (var x = 0; x < _surface.Width; x++)
    //     {
    //         for (var y = 0; y < _surface.Height; y++)
    //         {
    //             var tileIndex = random.Next(0, _autoRandomizeTileCount);
    //             var foregroundColor = new LyColor(
    //                 (byte)random.Next(0, 256),
    //                 (byte)random.Next(0, 256),
    //                 (byte)random.Next(0, 256),
    //                 (byte)random.Next(0, 256)
    //             );
    //
    //             LyColor? backgroundColor = null;
    //
    //             if (random.Next(100) < 30)
    //             {
    //                 backgroundColor = new LyColor(
    //                     (byte)random.Next(0, 256),
    //                     (byte)random.Next(0, 256),
    //                     (byte)random.Next(0, 256),
    //                     10
    //                 );
    //             }
    //
    //             var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor);
    //             _surface.SetTile(SelectedLayerIndex, x, y, tileData);
    //         }
    //     }
    // }

    /// <summary>
    /// Renders a single layer to the sprite batch with frustum culling.
    /// Only renders tiles that are visible within the screen bounds.
    /// </summary>
    private void RenderLayer(SpriteBatch spriteBatch, TileLayer layer, int layerIndex)
    {
        // Get the tileset for this layer (with fallback to default)
        var tileset = GetLayerTileset(layerIndex);

        if (tileset == null)
        {
            return;
        }

        // Calculate scaled tile dimensions
        var scaledTileWidth = tileset.TileWidth * TileRenderScale;
        var scaledTileHeight = tileset.TileHeight * TileRenderScale;

        // Calculate visible region accounting for margins (scissor area)
        // Note: Tiles are already in coordinates relative to Position (due to PushTranslation)
        var visibleX = Margin.X;
        var visibleY = Margin.Y;
        var visibleWidth = Size.X - Margin.X - Margin.Z;
        var visibleHeight = Size.Y - Margin.Y - Margin.W;

        // Calculate which tiles are visible based on scissor bounds (using scaled dimensions)
        var minTileX = Math.Max(0, (int)(visibleX / scaledTileWidth));
        var minTileY = Math.Max(0, (int)(visibleY / scaledTileHeight));
        var maxTileX = Math.Min(_surface.Width, (int)((visibleX + visibleWidth) / scaledTileWidth) + 1);
        var maxTileY = Math.Min(_surface.Height, (int)((visibleY + visibleHeight) / scaledTileHeight) + 1);

        for (var x = minTileX; x < maxTileX; x++)
        {
            for (var y = minTileY; y < maxTileY; y++)
            {
                var tileData = layer.Tiles[x, y];

                if (tileData.BackgroundColor.A == 0)
                {
                    continue;
                }

                // Draw background color if present
                var position = new Vector2(x * scaledTileWidth, y * scaledTileHeight);
                spriteBatch.DrawRectangle(position, new(scaledTileWidth, scaledTileHeight), tileData.BackgroundColor);
            }
        }

        for (var x = minTileX; x < maxTileX; x++)
        {
            for (var y = minTileY; y < maxTileY; y++)
            {
                var tileData = layer.Tiles[x, y];

                // Skip empty tiles (tile index -1)
                if (tileData.TileIndex < 0 || tileData.TileIndex >= tileset.TileCount)
                {
                    continue;
                }

                // Get the tile from the tileset
                var tile = tileset.GetTile(tileData.TileIndex);

                // Convert pixel coordinates to normalized UV coordinates (0-1 range)
                var uvX = (float)tile.SourceRect.Origin.X / tileset.Texture.Width;
                var uvY = (float)tile.SourceRect.Origin.Y / tileset.Texture.Height;
                var uvWidth = (float)tile.SourceRect.Size.X / tileset.Texture.Width;
                var uvHeight = (float)tile.SourceRect.Size.Y / tileset.Texture.Height;

                var sourceRect = new Rectangle<float>(uvX, uvY, uvWidth, uvHeight);
                sourceRect = TileRenderData.ApplyFlip(sourceRect, tileData.Flip);

                // Apply layer opacity to the foreground color
                var color = tileData.ForegroundColor.WithAlpha((byte)(tileData.ForegroundColor.A * layer.Opacity));

                // Draw the tile at the correct position using scaled tile dimensions
                var tilePosition = new Vector2(x * scaledTileWidth, y * scaledTileHeight);
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

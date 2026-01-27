using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Extensions.TilesetSurface;
using LillyQuest.Engine.Managers.Screens.Base;
using LillyQuest.Engine.Types;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.TilesetSurface;

/// <summary>
/// Screen for drawing and editing tileset surfaces with multi-layer support.
/// </summary>
public class TilesetSurfaceScreen : BaseScreen
{
    /// <summary>
    /// Fired when the mouse moves over a valid tile.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileX">Tile X.</param>
    /// <param name="tileY">Tile Y.</param>
    /// <param name="mouseX">Mouse X in screen pixels.</param>
    /// <param name="mouseY">Mouse Y in screen pixels.</param>
    public delegate void TileMouseMoveHandler(int layerIndex, int tileX, int tileY, int mouseX, int mouseY);

    /// <summary>
    /// Fired when the mouse moves over a valid tile.
    /// </summary>
    public event TileMouseMoveHandler? TileMouseMove;

    /// <summary>
    /// Fired when the mouse moves over a valid tile on any layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileX">Tile X.</param>
    /// <param name="tileY">Tile Y.</param>
    /// <param name="mouseX">Mouse X in screen pixels.</param>
    /// <param name="mouseY">Mouse Y in screen pixels.</param>
    public delegate void TileMouseMoveAllLayersHandler(int layerIndex, int tileX, int tileY, int mouseX, int mouseY);

    /// <summary>
    /// Fired when the mouse moves over a valid tile on any layer.
    /// </summary>
    public event TileMouseMoveAllLayersHandler? TileMouseMoveAllLayers;

    /// <summary>
    /// Fired when the mouse is pressed over a valid tile.
    /// </summary>
    public delegate void TileMouseDownHandler(int layerIndex, int tileX, int tileY, IReadOnlyList<MouseButton> buttons);

    /// <summary>
    /// Fired when the mouse is pressed over a valid tile.
    /// </summary>
    public event TileMouseDownHandler? TileMouseDown;

    /// <summary>
    /// Fired when the mouse wheel scrolls over a valid tile.
    /// </summary>
    public delegate void TileMouseWheelHandler(int layerIndex, int tileX, int tileY, float delta);

    /// <summary>
    /// Fired when the mouse wheel scrolls over a valid tile.
    /// </summary>
    public event TileMouseWheelHandler? TileMouseWheel;

    private readonly ITilesetManager _tilesetManager;
    private readonly TilesetSurface _surface;

    private Tileset _tileset;
    private readonly Dictionary<int, int> _viewLockMasterByFollower = new();
    private readonly Dictionary<int, HashSet<int>> _viewLockFollowersByMaster = new();

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
    /// When true, the screen size is computed from TileViewSize.
    /// When false, the screen size is considered externally controlled.
    /// </summary>
    public bool AutoSizeFromTileView { get; set; } = true;

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
            UpdateLayerRenderScaleFromTileView(Size, includeMargins: true);
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
            UpdateLayerRenderScaleFromTileView(Size, includeMargins: true);
        }
    }

    /// <summary>
    /// Computes a tile view size that fits within the provided screen size.
    /// </summary>
    /// <param name="screenSize">Target screen size in logical units.</param>
    /// <param name="includeMargins">Whether to subtract margins from the available size.</param>
    public Vector2 ComputeTileViewSizeFromScreen(Vector2 screenSize, bool includeMargins = true)
    {
        if (_tileset == null)
        {
            return _tileViewSize;
        }

        var masterTileset = GetMasterTileset();
        var availableSize = includeMargins
                                ? new Vector2(
                                    MathF.Max(0f, screenSize.X - Margin.X - Margin.Z),
                                    MathF.Max(0f, screenSize.Y - Margin.Y - Margin.W)
                                )
                                : screenSize;

        var tileWidth = masterTileset.TileWidth * TileRenderScale;
        var tileHeight = masterTileset.TileHeight * TileRenderScale;

        var cols = tileWidth > 0 ? MathF.Floor(availableSize.X / tileWidth) : 0f;
        var rows = tileHeight > 0 ? MathF.Floor(availableSize.Y / tileHeight) : 0f;

        return new Vector2(MathF.Max(1f, cols), MathF.Max(1f, rows));
    }

    /// <summary>
    /// Updates the tile view size to fit within the provided screen size.
    /// </summary>
    /// <param name="screenSize">Target screen size in logical units.</param>
    /// <param name="keepScreenSize">Whether to preserve the passed screen size as the screen size.</param>
    /// <param name="includeMargins">Whether to subtract margins from the available size.</param>
    public void FitTileViewToScreen(Vector2 screenSize, bool keepScreenSize = true, bool includeMargins = true)
    {
        TileViewSize = ComputeTileViewSizeFromScreen(screenSize, includeMargins);

        if (keepScreenSize)
        {
            AutoSizeFromTileView = false;
            Size = screenSize;
            UpdateLayerRenderScaleFromTileView(screenSize, includeMargins);

            return;
        }

        AutoSizeFromTileView = true;
        UpdateScreenSizeFromTileView();
    }

    /// <summary>
    /// Updates the screen size and scales layers to match the current tile view size.
    /// </summary>
    /// <param name="screenSize">Target screen size in logical units.</param>
    /// <param name="includeMargins">Whether to subtract margins from the available size.</param>
    public void ApplyTileViewScaleToScreen(Vector2 screenSize, bool includeMargins = true)
    {
        AutoSizeFromTileView = false;
        Size = screenSize;
        UpdateLayerRenderScaleFromTileView(screenSize, includeMargins);
    }

    public TilesetSurfaceScreen(ITilesetManager tilesetManager)
    {
        _tilesetManager = tilesetManager;

        // Display 90x30 tiles (like sadconsole's 90x30 character grid)
        // With 24px tiles: 90*24=2160w, 30*24=720h
        Size = new(2160, 720);
        IsModal = false;

        // Initialize surface early so it can be populated before OnLoad
        _surface = new(300, 300);
    }

    /// <summary>
    /// Adds a tile to the surface at the specified coordinates on the selected layer.
    /// </summary>
    public void AddTileToSurface(int x, int y, TileRenderData tileData)
    {
        _surface.SetTile(SelectedLayerIndex, x, y, tileData);
    }

    /// <summary>
    /// Adds a tile to the surface at the specified coordinates on the given layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="x">Tile X.</param>
    /// <param name="y">Tile Y.</param>
    /// <param name="tileData">Tile render data.</param>
    public void AddTileToSurface(int layerIndex, int x, int y, TileRenderData tileData)
    {
        _surface.SetTile(layerIndex, x, y, tileData);
    }

    public static Vector2 ApplyTileViewSize(
        Vector2 tileViewSize,
        Vector2 currentSize,
        int tileWidth,
        int tileHeight,
        float tileRenderScale
    )
        => tileViewSize.ApplyTileViewSize(currentSize, tileWidth, tileHeight, tileRenderScale);

    /// <summary>
    /// Centers the layer view on the specified tile coordinates.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileX">Tile X to center.</param>
    /// <param name="tileY">Tile Y to center.</param>
    public void CenterViewOnTile(int layerIndex, int tileX, int tileY)
    {
        if (!TryGetLayerInputTileInfo(layerIndex, out var tileWidth, out var tileHeight, out _))
        {
            return;
        }

        var layerScale = _surface.Layers[layerIndex].RenderScale;
        var scaledTileWidth = tileWidth * TileRenderScale * layerScale;
        var scaledTileHeight = tileHeight * TileRenderScale * layerScale;
        var visibleWidth = Size.X - Margin.X - Margin.Z;
        var visibleHeight = Size.Y - Margin.Y - Margin.W;

        var viewTilesX = MathF.Floor(visibleWidth / scaledTileWidth);
        var viewTilesY = MathF.Floor(visibleHeight / scaledTileHeight);

        var viewOffset = new Vector2(
            tileX - viewTilesX / 2f,
            tileY - viewTilesY / 2f
        );

        var layer = _surface.Layers[layerIndex];

        if (layer.SmoothViewEnabled)
        {
            SetLayerViewTileTarget(layerIndex, viewOffset);
            SetLayerViewPixelTarget(layerIndex, Vector2.Zero);
        }
        else
        {
            SetLayerViewTileOffset(layerIndex, viewOffset);
            SetLayerViewPixelOffset(layerIndex, Vector2.Zero);
        }
    }

    /// <summary>
    /// Clears all tiles on the specified layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    public void ClearLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        var emptyTile = new TileRenderData(-1, LyColor.White);

        for (var x = 0; x < _surface.Width; x++)
        {
            for (var y = 0; y < _surface.Height; y++)
            {
                layer.SetTile(x, y, emptyTile);
            }
        }
    }

    public static Vector2 ComputeScreenSizeFromTileView(
        Vector2 tileViewSize,
        int tileWidth,
        int tileHeight,
        float tileRenderScale
    )
        => tileViewSize.ToScreenSize(tileWidth, tileHeight, tileRenderScale);

    public static float ComputeLayerRenderScaleForTileView(
        Vector2 tileViewSize,
        Vector2 screenSize,
        int tileWidth,
        int tileHeight,
        float tileRenderScale,
        Vector4 margin,
        bool includeMargins = true
    )
    {
        if (tileViewSize.X <= 0f || tileViewSize.Y <= 0f || tileWidth <= 0 || tileHeight <= 0 || tileRenderScale <= 0f)
        {
            return 0f;
        }

        var availableSize = includeMargins
                                ? new Vector2(
                                    MathF.Max(0f, screenSize.X - margin.X - margin.Z),
                                    MathF.Max(0f, screenSize.Y - margin.Y - margin.W)
                                )
                                : screenSize;

        if (availableSize.X <= 0f || availableSize.Y <= 0f)
        {
            return 0f;
        }

        var widthScale = availableSize.X / (tileViewSize.X * tileWidth * tileRenderScale);
        var heightScale = availableSize.Y / (tileViewSize.Y * tileHeight * tileRenderScale);

        return MathF.Max(widthScale, heightScale);
    }

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
    /// Gets the pixel offset for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <returns>Pixel offset for the layer.</returns>
    public Vector2 GetLayerPixelOffset(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return Vector2.Zero;
        }

        return _surface.Layers[layerIndex].PixelOffset;
    }

    /// <summary>
    /// Gets the render scale multiplier for a specific layer.
    /// </summary>
    public float GetLayerRenderScale(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return 1f;
        }

        return _surface.Layers[layerIndex].RenderScale;
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

    /// <summary>
    /// Gets the view pixel offset for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <returns>View offset in pixels.</returns>
    public Vector2 GetLayerViewPixelOffset(int layerIndex)
        => TryGetLayerViewOffsets(layerIndex, out _, out var pixelOffset)
               ? pixelOffset
               : Vector2.Zero;

    /// <summary>
    /// Gets the target view offset for a specific layer in tile coordinates.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <returns>Target view offset in tile coordinates.</returns>
    public Vector2 GetLayerViewTileTarget(int layerIndex)
    {
        if (!IsValidLayerIndex(layerIndex))
        {
            return Vector2.Zero;
        }

        return _surface.Layers[layerIndex].ViewTileOffsetTarget;
    }

    /// <summary>
    /// Gets the target view offset for a specific layer in pixels.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <returns>Target view offset in pixels.</returns>
    public Vector2 GetLayerViewPixelTarget(int layerIndex)
    {
        if (!IsValidLayerIndex(layerIndex))
        {
            return Vector2.Zero;
        }

        return _surface.Layers[layerIndex].ViewPixelOffsetTarget;
    }

    /// <summary>
    /// Gets the view tile offset for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <returns>View offset in tile coordinates.</returns>
    public Vector2 GetLayerViewTileOffset(int layerIndex)
        => TryGetLayerViewOffsets(layerIndex, out var tileOffset, out _)
               ? tileOffset
               : Vector2.Zero;

    /// <summary>
    /// Gets the interpolated tile position for a movement based on its elapsed time.
    /// </summary>
    public Vector2 GetMovementTilePosition(TileMovement movement)
    {
        var duration = movement.DurationSeconds <= 0f ? 0.0001f : movement.DurationSeconds;
        var progress = Math.Clamp(movement.ElapsedSeconds / duration, 0f, 1f);

        if (movement.Bounce && movement.State == TileMovementState.Returning)
        {
            return Vector2.Lerp(movement.DestinationTile, movement.SourceTile, progress);
        }

        return Vector2.Lerp(movement.SourceTile, movement.DestinationTile, progress);
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

        // Initialize layers if they haven't been initialized yet
        if (_surface.Layers.Count == 0)
        {
            _surface.Initialize(LayerCount);
        }

        UpdateScreenSizeFromTileView();
        UpdateLayerRenderScaleFromTileView(Size, includeMargins: true);

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
            var (tileX, tileY) = GetInputTileCoordinates(x, y);
            TileMouseDown?.Invoke(SelectedLayerIndex, tileX, tileY, buttons);

            return true;
        }

        var (leftTileX, leftTileY) = GetInputTileCoordinates(x, y);

        // Create tile render data with the selected tile index and white color
        var tileData = new TileRenderData(SelectedTileIndex, LyColor.White);

        // Draw on the selected layer
        _surface.SetTile(SelectedLayerIndex, leftTileX, leftTileY, tileData);

        TileMouseDown?.Invoke(SelectedLayerIndex, leftTileX, leftTileY, buttons);

        return true;
    }

    public override bool OnMouseMove(int x, int y)
    {
        if (!HitTest(x, y))
        {
            return false;
        }

        var (tileX, tileY) = GetInputTileCoordinates(SelectedLayerIndex, x, y);
        TileMouseMove?.Invoke(SelectedLayerIndex, tileX, tileY, x, y);

        if (TileMouseMoveAllLayers is not null)
        {
            for (var layerIndex = 0; layerIndex < _surface.Layers.Count; layerIndex++)
            {
                if (!TryGetLayerInputTileInfo(layerIndex, out _, out _, out _))
                {
                    continue;
                }

                var (layerTileX, layerTileY) = GetInputTileCoordinates(layerIndex, x, y);
                TileMouseMoveAllLayers.Invoke(layerIndex, layerTileX, layerTileY, x, y);
            }
        }

        return true;
    }

    public override bool OnMouseWheel(int x, int y, float delta)
    {
        if (!HitTest(x, y))
        {
            return false;
        }

        var (tileX, tileY) = GetInputTileCoordinates(SelectedLayerIndex, x, y);
        var handledDelta = _surface.HandleMouseWheel(SelectedLayerIndex, tileX, tileY, delta);

        if (handledDelta != 0f)
        {
            TileMouseWheel?.Invoke(SelectedLayerIndex, tileX, tileY, handledDelta);
        }

        return true;
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

                var destinationX = (int)movement.DestinationTile.X;
                var destinationY = (int)movement.DestinationTile.Y;
                _surface.SetTile(layerIndex, destinationX, destinationY, movement.TileData);
                movement.State = TileMovementState.Completed;
                active.RemoveAt(i);
            }
        }
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

    /// <summary>
    /// Sets an input tile size override for the specified layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileSize">Tile size in pixels, or null to clear.</param>
    public void SetLayerInputTileSizeOverride(int layerIndex, Vector2? tileSize)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].InputTileSizeOverride = tileSize;
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
    /// Sets the pixel offset for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="pixelOffset">Pixel offset to apply.</param>
    public void SetLayerPixelOffset(int layerIndex, Vector2 pixelOffset)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].PixelOffset = pixelOffset;
    }

    /// <summary>
    /// Sets the render scale multiplier for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="scale">Render scale multiplier.</param>
    public void SetLayerRenderScale(int layerIndex, float scale)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.RenderScale = scale;
        layer.RenderScaleTarget = scale;
        layer.SmoothRenderScaleEnabled = false;
    }

    /// <summary>
    /// Enables or disables smooth render scale transitions for a layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="enabled">Whether smoothing is enabled.</param>
    /// <param name="speed">Zoom speed in units per second.</param>
    public void SetLayerRenderScaleSmoothing(int layerIndex, bool enabled, float speed)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.SmoothRenderScaleEnabled = enabled;
        layer.SmoothRenderScaleSpeed = speed;
    }

    /// <summary>
    /// Sets the render scale target for smooth zoom on a layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="targetScale">Target render scale multiplier.</param>
    /// <param name="speed">Zoom speed in units per second.</param>
    public void SetLayerRenderScaleTarget(int layerIndex, float targetScale, float speed)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.RenderScaleTarget = targetScale;
        layer.SmoothRenderScaleSpeed = speed;
        layer.SmoothRenderScaleEnabled = true;
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
        UpdateScreenSizeFromTileView();
        UpdateLayerRenderScaleFromTileView(Size, includeMargins: true);
    }

    /// <summary>
    /// Sets the view offset for a specific layer in pixels.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="pixelOffset">View offset in pixels.</param>
    public void SetLayerViewPixelOffset(int layerIndex, Vector2 pixelOffset)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.ViewPixelOffset = pixelOffset;
        layer.ViewPixelOffsetTarget = pixelOffset;
        PropagateViewTargets(layerIndex);
    }

    /// <summary>
    /// Sets the target view offset for a specific layer in pixels.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="pixelOffset">Target view offset in pixels.</param>
    public void SetLayerViewPixelTarget(int layerIndex, Vector2 pixelOffset)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].ViewPixelOffsetTarget = pixelOffset;
        PropagateViewTargets(layerIndex);
    }

    /// <summary>
    /// Enables or disables smooth view scrolling for the specified layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="enabled">True to enable smoothing.</param>
    /// <param name="speed">Smoothing speed per second.</param>
    public void SetLayerViewSmoothing(int layerIndex, bool enabled, float speed = 10f)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.SmoothViewEnabled = enabled;
        layer.SmoothViewSpeed = speed;
    }

    /// <summary>
    /// Sets the view offset for a specific layer in tile coordinates.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileOffset">View offset in tile coordinates.</param>
    public void SetLayerViewTileOffset(int layerIndex, Vector2 tileOffset)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        var layer = _surface.Layers[layerIndex];
        layer.ViewTileOffset = tileOffset;
        layer.ViewTileOffsetTarget = tileOffset;
        PropagateViewTargets(layerIndex);
    }

    /// <summary>
    /// Sets the target view offset for a specific layer in tile coordinates.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileOffset">Target view offset in tile coordinates.</param>
    public void SetLayerViewTileTarget(int layerIndex, Vector2 tileOffset)
    {
        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return;
        }

        _surface.Layers[layerIndex].ViewTileOffsetTarget = tileOffset;
        PropagateViewTargets(layerIndex);
    }

    /// <summary>
    /// Locks a follower layer's view targets to a master layer.
    /// </summary>
    public void SetLayerViewLock(int masterIndex, int followerIndex)
    {
        if (masterIndex == followerIndex)
        {
            return;
        }

        if (!IsValidLayerIndex(masterIndex) || !IsValidLayerIndex(followerIndex))
        {
            return;
        }

        if (_viewLockMasterByFollower.ContainsKey(masterIndex))
        {
            return;
        }

        if (_viewLockFollowersByMaster.TryGetValue(followerIndex, out var existingFollowers) &&
            existingFollowers.Count > 0)
        {
            return;
        }

        if (_viewLockMasterByFollower.TryGetValue(followerIndex, out var oldMaster))
        {
            if (_viewLockFollowersByMaster.TryGetValue(oldMaster, out var oldFollowers))
            {
                oldFollowers.Remove(followerIndex);

                if (oldFollowers.Count == 0)
                {
                    _viewLockFollowersByMaster.Remove(oldMaster);
                }
            }
        }

        _viewLockMasterByFollower[followerIndex] = masterIndex;

        if (!_viewLockFollowersByMaster.TryGetValue(masterIndex, out var followers))
        {
            followers = new HashSet<int>();
            _viewLockFollowersByMaster[masterIndex] = followers;
        }

        followers.Add(followerIndex);
        SyncViewFromMaster(masterIndex, followerIndex);
    }

    /// <summary>
    /// Clears a follower layer's view lock.
    /// </summary>
    public void ClearLayerViewLock(int followerIndex)
    {
        if (!IsValidLayerIndex(followerIndex))
        {
            return;
        }

        if (!_viewLockMasterByFollower.TryGetValue(followerIndex, out var masterIndex))
        {
            return;
        }

        _viewLockMasterByFollower.Remove(followerIndex);

        if (_viewLockFollowersByMaster.TryGetValue(masterIndex, out var followers))
        {
            followers.Remove(followerIndex);

            if (followers.Count == 0)
            {
                _viewLockFollowersByMaster.Remove(masterIndex);
            }
        }
    }

    /// <summary>
    /// Gets the master layer index for a follower, or null if not locked.
    /// </summary>
    public int? GetLayerViewLockMaster(int followerIndex)
    {
        if (!IsValidLayerIndex(followerIndex))
        {
            return null;
        }

        return _viewLockMasterByFollower.TryGetValue(followerIndex, out var masterIndex)
                   ? masterIndex
                   : null;
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
    /// Gets tile size and pixel offset for the specified layer using input overrides.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileWidth">Tile width in pixels.</param>
    /// <param name="tileHeight">Tile height in pixels.</param>
    /// <param name="pixelOffset">Layer pixel offset.</param>
    /// <returns>True if the layer and tileset are valid.</returns>
    public bool TryGetLayerInputTileInfo(int layerIndex, out int tileWidth, out int tileHeight, out Vector2 pixelOffset)
    {
        tileWidth = 0;
        tileHeight = 0;
        pixelOffset = Vector2.Zero;

        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return false;
        }

        var layer = _surface.Layers[layerIndex];

        if (layer.InputTileSizeOverride.HasValue)
        {
            var overrideSize = layer.InputTileSizeOverride.Value;
            tileWidth = (int)overrideSize.X;
            tileHeight = (int)overrideSize.Y;
            pixelOffset = layer.PixelOffset;

            return tileWidth > 0 && tileHeight > 0;
        }

        return TryGetLayerTileInfo(layerIndex, out tileWidth, out tileHeight, out pixelOffset);
    }

    /// <summary>
    /// Gets tile size and pixel offset for the specified layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileWidth">Tile width in pixels.</param>
    /// <param name="tileHeight">Tile height in pixels.</param>
    /// <param name="pixelOffset">Layer pixel offset.</param>
    /// <returns>True if the layer and tileset are valid.</returns>
    public bool TryGetLayerTileInfo(int layerIndex, out int tileWidth, out int tileHeight, out Vector2 pixelOffset)
    {
        tileWidth = 0;
        tileHeight = 0;
        pixelOffset = Vector2.Zero;

        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return false;
        }

        var tileset = GetLayerTileset(layerIndex);

        if (tileset == null)
        {
            return false;
        }

        tileWidth = tileset.TileWidth;
        tileHeight = tileset.TileHeight;
        pixelOffset = _surface.Layers[layerIndex].PixelOffset;

        return true;
    }

    /// <summary>
    /// Gets the view offsets for a specific layer.
    /// </summary>
    /// <param name="layerIndex">Layer index.</param>
    /// <param name="tileOffset">View offset in tile coordinates.</param>
    /// <param name="pixelOffset">View offset in pixels.</param>
    /// <returns>True if the layer index is valid.</returns>
    public bool TryGetLayerViewOffsets(int layerIndex, out Vector2 tileOffset, out Vector2 pixelOffset)
    {
        tileOffset = Vector2.Zero;
        pixelOffset = Vector2.Zero;

        if (layerIndex < 0 || layerIndex >= _surface.Layers.Count)
        {
            return false;
        }

        var layer = _surface.Layers[layerIndex];
        tileOffset = layer.ViewTileOffset;
        pixelOffset = layer.ViewPixelOffset;

        return true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UpdateViewSmoothing(gameTime);
        UpdateRenderScaleSmoothing(gameTime);
        ProcessMovements(gameTime);

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
    /// Converts mouse coordinates to tile coordinates based on the selected layer's tileset.
    /// </summary>
    private (int x, int y) GetInputTileCoordinates(int mouseX, int mouseY)
        => GetInputTileCoordinates(SelectedLayerIndex, mouseX, mouseY);

    /// <summary>
    /// Converts mouse coordinates to tile coordinates based on the specified layer's tileset.
    /// </summary>
    private (int x, int y) GetInputTileCoordinates(int layerIndex, int mouseX, int mouseY)
    {
        if (!TryGetLayerInputTileInfo(layerIndex, out var tileWidth, out var tileHeight, out var layerOffset))
        {
            return (0, 0);
        }

        var layerScale = _surface.Layers[layerIndex].RenderScale;
        var scaledTileWidth = tileWidth * TileRenderScale * layerScale;
        var scaledTileHeight = tileHeight * TileRenderScale * layerScale;

        // Adjust mouse position relative to screen position
        TryGetLayerViewOffsets(layerIndex, out var viewTileOffset, out var viewPixelOffset);
        var viewOffsetPx = new Vector2(
                               viewTileOffset.X * scaledTileWidth,
                               viewTileOffset.Y * scaledTileHeight
                           ) +
                           viewPixelOffset;

        var relativeX = mouseX - Position.X - layerOffset.X + viewOffsetPx.X;
        var relativeY = mouseY - Position.Y - layerOffset.Y + viewOffsetPx.Y;

        // Calculate tile coordinates using scaled tile dimensions
        var tileX = (int)MathF.Floor(relativeX / scaledTileWidth);
        var tileY = (int)MathF.Floor(relativeY / scaledTileHeight);

        return (tileX, tileY);
    }

    /// <summary>
    /// Gets the tileset for a specific layer, with fallback to default.
    /// </summary>
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

    private Tileset GetMasterTileset()
    {
        if (_surface.Layers.Count > 0)
        {
            var layer0TilesetName = _surface.Layers[0].TilesetName;

            if (!string.IsNullOrEmpty(layer0TilesetName) &&
                _tilesetManager.TryGetTileset(layer0TilesetName, out var layerTileset))
            {
                return layerTileset;
            }
        }

        return _tileset;
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
        var layerScale = layer.RenderScale;
        var scaledTileWidth = tileset.TileWidth * TileRenderScale * layerScale;
        var scaledTileHeight = tileset.TileHeight * TileRenderScale * layerScale;
        var layerOffset = layer.PixelOffset;
        var viewOffsetPx = new Vector2(
                               layer.ViewTileOffset.X * scaledTileWidth,
                               layer.ViewTileOffset.Y * scaledTileHeight
                           ) +
                           layer.ViewPixelOffset;

        // Calculate visible region accounting for margins (scissor area)
        // Note: Tiles are already in coordinates relative to Position (due to PushTranslation)
        var visibleX = Margin.X - layerOffset.X + viewOffsetPx.X;
        var visibleY = Margin.Y - layerOffset.Y + viewOffsetPx.Y;
        var visibleWidth = Size.X - Margin.X - Margin.Z;
        var visibleHeight = Size.Y - Margin.Y - Margin.W;

        // Calculate which tiles are visible based on scissor bounds (using scaled dimensions)
        var minTileX = Math.Max(0, (int)MathF.Floor(visibleX / scaledTileWidth));
        var minTileY = Math.Max(0, (int)MathF.Floor(visibleY / scaledTileHeight));
        var maxTileX = Math.Min(_surface.Width, (int)MathF.Floor((visibleX + visibleWidth) / scaledTileWidth) + 1);
        var maxTileY = Math.Min(_surface.Height, (int)MathF.Floor((visibleY + visibleHeight) / scaledTileHeight) + 1);

        if (minTileX >= maxTileX || minTileY >= maxTileY)
        {
            return;
        }

        var maxTileXInclusive = maxTileX - 1;
        var maxTileYInclusive = maxTileY - 1;

        foreach (var (chunkX, chunkY, chunk) in layer.EnumerateChunksInRange(
                     minTileX,
                     minTileY,
                     maxTileXInclusive,
                     maxTileYInclusive
                 ))
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

            var tilePosition = GetMovementTilePosition(movement);
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

    private void UpdateRenderScaleSmoothing(GameTime gameTime)
    {
        if (gameTime.Elapsed.TotalSeconds <= 0)
        {
            return;
        }

        var dt = (float)gameTime.Elapsed.TotalSeconds;

        for (var i = 0; i < _surface.Layers.Count; i++)
        {
            var layer = _surface.Layers[i];

            if (!layer.SmoothRenderScaleEnabled)
            {
                continue;
            }

            var t = 1f - MathF.Exp(-layer.SmoothRenderScaleSpeed * dt);
            layer.RenderScale += (layer.RenderScaleTarget - layer.RenderScale) * t;
        }
    }

    private void UpdateScreenSizeFromTileView()
    {
        if (!AutoSizeFromTileView)
        {
            return;
        }

        if (_tileset == null)
        {
            return;
        }

        var masterTileset = GetMasterTileset();
        Size = ComputeScreenSizeFromTileView(
            _tileViewSize,
            masterTileset.TileWidth,
            masterTileset.TileHeight,
            _tileRenderScale
        );
    }

    private void UpdateLayerRenderScaleFromTileView(Vector2 screenSize, bool includeMargins)
    {
        if (_surface.Layers.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _surface.Layers.Count; i++)
        {
            if (!TryGetLayerInputTileInfo(i, out var tileWidth, out var tileHeight, out _))
            {
                continue;
            }

            var scale = ComputeLayerRenderScaleForTileView(
                _tileViewSize,
                screenSize,
                tileWidth,
                tileHeight,
                _tileRenderScale,
                Margin,
                includeMargins
            );

            if (scale <= 0f)
            {
                continue;
            }

            SetLayerRenderScale(i, scale);
        }
    }

    private void UpdateViewSmoothing(GameTime gameTime)
    {
        if (gameTime.Elapsed.TotalSeconds <= 0)
        {
            return;
        }

        var dt = (float)gameTime.Elapsed.TotalSeconds;

        for (var i = 0; i < _surface.Layers.Count; i++)
        {
            if (_viewLockMasterByFollower.ContainsKey(i))
            {
                continue;
            }

            var layer = _surface.Layers[i];

            if (!layer.SmoothViewEnabled)
            {
                continue;
            }

            if (!TryGetLayerInputTileInfo(i, out var tileWidth, out var tileHeight, out _))
            {
                continue;
            }

            var renderScale = layer.RenderScale;
            var scaledTileWidth = tileWidth * TileRenderScale * renderScale;
            var scaledTileHeight = tileHeight * TileRenderScale * renderScale;

            var currentPx = new Vector2(
                                layer.ViewTileOffset.X * scaledTileWidth,
                                layer.ViewTileOffset.Y * scaledTileHeight
                            ) +
                            layer.ViewPixelOffset;

            var targetPx = new Vector2(
                               layer.ViewTileOffsetTarget.X * scaledTileWidth,
                               layer.ViewTileOffsetTarget.Y * scaledTileHeight
                           ) +
                           layer.ViewPixelOffsetTarget;

            var t = 1f - MathF.Exp(-layer.SmoothViewSpeed * dt);
            var nextPx = Vector2.Lerp(currentPx, targetPx, t);

            var nextTileOffset = new Vector2(
                MathF.Floor(nextPx.X / scaledTileWidth),
                MathF.Floor(nextPx.Y / scaledTileHeight)
            );
            var nextPixelOffset = nextPx -
                                  new Vector2(
                                      nextTileOffset.X * scaledTileWidth,
                                      nextTileOffset.Y * scaledTileHeight
                                  );

            layer.ViewTileOffset = nextTileOffset;
            layer.ViewPixelOffset = nextPixelOffset;

            if (_viewLockFollowersByMaster.ContainsKey(i))
            {
                PropagateViewTargets(i);
            }
        }
    }

    private void PropagateViewTargets(int masterIndex)
    {
        if (!_viewLockFollowersByMaster.TryGetValue(masterIndex, out var followers))
        {
            return;
        }

        foreach (var followerIndex in followers)
        {
            if (!IsValidLayerIndex(followerIndex))
            {
                continue;
            }

            SyncViewFromMaster(masterIndex, followerIndex);
        }
    }

    private void SyncViewFromMaster(int masterIndex, int followerIndex)
    {
        if (!IsValidLayerIndex(masterIndex) || !IsValidLayerIndex(followerIndex))
        {
            return;
        }

        var master = _surface.Layers[masterIndex];
        var follower = _surface.Layers[followerIndex];
        follower.ViewTileOffset = master.ViewTileOffset;
        follower.ViewPixelOffset = master.ViewPixelOffset;
        follower.ViewTileOffsetTarget = master.ViewTileOffsetTarget;
        follower.ViewPixelOffsetTarget = master.ViewPixelOffsetTarget;
    }

    private bool IsValidLayerIndex(int layerIndex)
        => layerIndex >= 0 && layerIndex < _surface.Layers.Count;
}

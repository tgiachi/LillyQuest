using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using Silk.NET.Input;

namespace LillyQuest.Engine.Screens.UI;

/// <summary>
/// UI control backed by a tile surface screen.
/// </summary>
public sealed class UITileSurfaceControl : UIScreenControl
{
    private readonly ITilesetManager _tilesetManager;
    private bool _surfaceLoaded;

    public TilesetSurfaceScreen Surface { get; }
    public bool AutoSizeFromTileView { get; set; } = true;

    public UITileSurfaceControl(ITilesetManager tilesetManager, int width, int height)
    {
        _tilesetManager = tilesetManager;
        Surface = new(tilesetManager)
        {
            LayerCount = 1,
            TileViewSize = new(width, height)
        };
        Surface.InitializeLayers(1);
    }

    public override bool HandleMouseDown(Vector2 point, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncSurfaceLayout();

        return Surface.OnMouseDown((int)point.X, (int)point.Y, buttons);
    }

    public override bool HandleMouseMove(Vector2 point)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncSurfaceLayout();

        return Surface.OnMouseMove((int)point.X, (int)point.Y);
    }

    public override bool HandleMouseUp(Vector2 point, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncSurfaceLayout();

        return Surface.OnMouseUp((int)point.X, (int)point.Y, buttons);
    }

    public override bool HandleMouseWheel(Vector2 point, float delta)
    {
        if (!IsEnabled || !IsVisible)
        {
            return false;
        }

        SyncSurfaceLayout();

        return Surface.OnMouseWheel((int)point.X, (int)point.Y, delta);
    }

    public override void Render(SpriteBatch? spriteBatch, EngineRenderContext? renderContext)
    {
        if (spriteBatch == null || renderContext == null)
        {
            return;
        }

        SyncSurfaceLayout();
        Surface.Render(spriteBatch, renderContext);
    }

    public override void Update(GameTime gameTime)
    {
        SyncSurfaceLayout();
        Surface.Update(gameTime);
    }

    private void EnsureSurfaceLoaded()
    {
        if (_surfaceLoaded)
        {
            return;
        }

        if (!_tilesetManager.TryGetTileset(Surface.DefaultTilesetName, out _))
        {
            return;
        }

        Surface.OnLoad();
        _surfaceLoaded = true;
    }

    private void SyncSurfaceLayout()
    {
        if (AutoSizeFromTileView)
        {
            EnsureSurfaceLoaded();
            Size = Surface.Size;
        }
        else
        {
            Surface.Size = Size;
        }

        Surface.Position = GetWorldPosition();
    }
}

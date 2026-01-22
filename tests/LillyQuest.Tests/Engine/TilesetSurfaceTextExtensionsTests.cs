using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Extensions.TilesetSurface;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceTextExtensionsTests
{
    private sealed class StubTilesetManager : LillyQuest.Core.Interfaces.Assets.ITilesetManager
    {
        public void Dispose() { }
        public LillyQuest.Core.Data.Assets.Tiles.Tileset GetTileset(string name) => throw new NotSupportedException();
        public bool HasTileset(string name) => false;
        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();
        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();
        public bool TryGetTileset(string name, out LillyQuest.Core.Data.Assets.Tiles.Tileset tileset)
        {
            tileset = null!;
            return false;
        }
        public IReadOnlyDictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset> GetAllTilesets()
            => new Dictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset>();
        public void UnloadTileset(string name) => throw new NotSupportedException();
    }

    [Test]
    public void FillRectangle_FillsAreaWithTile()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.FillRectangle(1, 1, 2, 2, 42, LyColor.White);

        Assert.That(screen.GetTile(0, 1, 1).TileIndex, Is.EqualTo(42));
        Assert.That(screen.GetTile(0, 2, 1).TileIndex, Is.EqualTo(42));
        Assert.That(screen.GetTile(0, 1, 2).TileIndex, Is.EqualTo(42));
        Assert.That(screen.GetTile(0, 2, 2).TileIndex, Is.EqualTo(42));
    }

    [Test]
    public void ClearArea_SetsTilesToEmpty()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.FillRectangle(0, 0, 2, 2, 9, LyColor.White);
        screen.ClearArea(0, 0, 2, 2);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(-1));
        Assert.That(screen.GetTile(0, 1, 1).TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void DrawLine_DrawsHorizontalLine()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawLine(0, 0, 2, 0, 7, LyColor.White);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(7));
        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(7));
        Assert.That(screen.GetTile(0, 2, 0).TileIndex, Is.EqualTo(7));
    }

    [Test]
    public void DrawRectangle_DrawsBorder()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawRectangle(0, 0, 3, 2, 11, LyColor.White);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(11));
        Assert.That(screen.GetTile(0, 2, 0).TileIndex, Is.EqualTo(11));
        Assert.That(screen.GetTile(0, 0, 1).TileIndex, Is.EqualTo(11));
        Assert.That(screen.GetTile(0, 2, 1).TileIndex, Is.EqualTo(11));
    }

    [Test]
    public void DrawCircle_DrawsCardinalPoints()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawCircle(1, 1, 1, 5, LyColor.White);

        Assert.That(screen.GetTile(0, 2, 1).TileIndex, Is.EqualTo(5));
        Assert.That(screen.GetTile(0, 0, 1).TileIndex, Is.EqualTo(5));
        Assert.That(screen.GetTile(0, 1, 2).TileIndex, Is.EqualTo(5));
        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(5));
    }

    [Test]
    public void FloodFill_ReplacesRegion()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.FillRectangle(0, 0, 2, 2, 1, LyColor.White);
        screen.FillRectangle(2, 0, 1, 1, 2, LyColor.White);
        screen.FloodFill(0, 0, 3, LyColor.White);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(3));
        Assert.That(screen.GetTile(0, 1, 1).TileIndex, Is.EqualTo(3));
        Assert.That(screen.GetTile(0, 2, 0).TileIndex, Is.EqualTo(2));
    }

    [Test]
    public void DrawBox_UsesCp437LineDrawing()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawBox(0, 0, 3, 3, LyColor.White);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(218));
        Assert.That(screen.GetTile(0, 2, 0).TileIndex, Is.EqualTo(191));
        Assert.That(screen.GetTile(0, 0, 2).TileIndex, Is.EqualTo(192));
        Assert.That(screen.GetTile(0, 2, 2).TileIndex, Is.EqualTo(217));
        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(196));
        Assert.That(screen.GetTile(0, 0, 1).TileIndex, Is.EqualTo(179));
    }

    [Test]
    public void DrawText_UsesCp437Mapping()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawText("A", 0, 0, LyColor.White);

        var tile = screen.GetTile(0, 0, 0);
        Assert.That(tile.TileIndex, Is.EqualTo(65));
    }

    [Test]
    public void DrawText_WithLayerIndex_WritesToSpecifiedLayer()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(2);

        screen.DrawText(1, "B", 0, 0, LyColor.White);

        Assert.That(screen.GetTile(1, 0, 0).TileIndex, Is.EqualTo(66));
        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void DrawTextPixel_WithLayerIndex_WritesToSpecifiedLayer()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(2);
        screen.SetLayerInputTileSizeOverride(1, new System.Numerics.Vector2(10, 10));

        screen.DrawTextPixel(1, "C", 15, 5, LyColor.White);

        Assert.That(screen.GetTile(1, 1, 0).TileIndex, Is.EqualTo(67));
        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void DrawTextPixel_AccountsForScreenPosition()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = new System.Numerics.Vector2(100, 0)
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));

        screen.DrawTextPixel(0, "D", 110, 5, LyColor.White);

        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(68));
    }

    [Test]
    public void DrawTextPixelScreen_UsesScreenCoordinates()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = new System.Numerics.Vector2(50, 0)
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));

        screen.DrawTextPixelScreen(0, "E", 60, 5, LyColor.White);

        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(69));
    }

    [Test]
    public void DrawTextPixelLocal_UsesLocalCoordinates()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = new System.Numerics.Vector2(50, 0)
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));

        screen.DrawTextPixelLocal(0, "F", 10, 5, LyColor.White);

        Assert.That(screen.GetTile(0, 1, 0).TileIndex, Is.EqualTo(70));
    }

    [Test]
    public void DrawText_NewLineAdvancesRow()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawText("A\nB", 0, 0, LyColor.White);

        var tileA = screen.GetTile(0, 0, 0);
        var tileB = screen.GetTile(0, 0, 1);

        Assert.That(tileA.TileIndex, Is.EqualTo(65));
        Assert.That(tileB.TileIndex, Is.EqualTo(66));
    }

    [Test]
    public void ComputeTileCoordinatesFromPixel_AccountsForOffsetAndScale()
    {
        var result = TilesetSurfaceTextExtensions.ComputeTileCoordinatesFromPixel(
            29,
            55,
            12,
            12,
            2.0f,
            new System.Numerics.Vector2(5, 7),
            System.Numerics.Vector2.Zero,
            System.Numerics.Vector2.Zero
        );

        Assert.That(result.x, Is.EqualTo(1));
        Assert.That(result.y, Is.EqualTo(2));
    }

    [Test]
    public void MouseCallbacks_ReportTileCoordinates()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = System.Numerics.Vector2.Zero,
            Size = new System.Numerics.Vector2(200, 200),
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));
        screen.SetLayerPixelOffset(0, new System.Numerics.Vector2(5, 5));
        screen.SetLayerViewTileOffset(0, System.Numerics.Vector2.Zero);
        screen.SetLayerViewPixelOffset(0, System.Numerics.Vector2.Zero);

        var moveResult = (-1, -1);
        var movePixels = (-1, -1);
        screen.TileMouseMove += (_, x, y, mouseX, mouseY) =>
        {
            moveResult = (x, y);
            movePixels = (mouseX, mouseY);
        };

        var downResult = (-1, -1);
        screen.TileMouseDown += (_, x, y, _) => downResult = (x, y);

        screen.OnMouseMove(25, 35);
        screen.OnMouseDown(25, 35, new[] { Silk.NET.Input.MouseButton.Left });

        Assert.That(moveResult, Is.EqualTo((2, 3)));
        Assert.That(movePixels, Is.EqualTo((25, 35)));
        Assert.That(downResult, Is.EqualTo((2, 3)));
    }

    [Test]
    public void MouseCallbacks_BroadcastTileCoordinatesForAllLayers()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = System.Numerics.Vector2.Zero,
            Size = new System.Numerics.Vector2(200, 200),
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(2);
        screen.SelectedLayerIndex = 0;
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));
        screen.SetLayerInputTileSizeOverride(1, new System.Numerics.Vector2(20, 20));
        screen.SetLayerPixelOffset(0, System.Numerics.Vector2.Zero);
        screen.SetLayerPixelOffset(1, System.Numerics.Vector2.Zero);
        screen.SetLayerViewTileOffset(0, System.Numerics.Vector2.Zero);
        screen.SetLayerViewTileOffset(1, System.Numerics.Vector2.Zero);
        screen.SetLayerViewPixelOffset(0, System.Numerics.Vector2.Zero);
        screen.SetLayerViewPixelOffset(1, System.Numerics.Vector2.Zero);

        var layer0Result = (-1, -1);
        var layer1Result = (-1, -1);
        var layer0Pixels = (-1, -1);
        var layer1Pixels = (-1, -1);
        screen.TileMouseMoveAllLayers += (layerIndex, x, y, mouseX, mouseY) =>
        {
            if (layerIndex == 0)
            {
                layer0Result = (x, y);
                layer0Pixels = (mouseX, mouseY);
            }
            else if (layerIndex == 1)
            {
                layer1Result = (x, y);
                layer1Pixels = (mouseX, mouseY);
            }
        };

        screen.OnMouseMove(25, 35);

        Assert.That(layer0Result, Is.EqualTo((2, 3)));
        Assert.That(layer1Result, Is.EqualTo((1, 1)));
        Assert.That(layer0Pixels, Is.EqualTo((25, 35)));
        Assert.That(layer1Pixels, Is.EqualTo((25, 35)));
    }

    [Test]
    public void LayerRenderScale_AffectsMouseSelection()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = System.Numerics.Vector2.Zero,
            Size = new System.Numerics.Vector2(200, 200),
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));
        screen.SetLayerRenderScale(0, 2f);

        var moveResult = (-1, -1);
        screen.TileMouseMove += (_, x, y, _, _) => moveResult = (x, y);

        screen.OnMouseMove(25, 35);

        Assert.That(moveResult, Is.EqualTo((1, 1)));
    }

    [Test]
    public void LayerRenderScaleSmooth_MovesTowardsTarget()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);

        screen.SetLayerRenderScale(0, 1f);
        screen.SetLayerRenderScaleTarget(0, 2f, speed: 10f);

        var gameTime = new LillyQuest.Core.Primitives.GameTime();
        gameTime.Update(0.1);
        screen.Update(gameTime);

        var current = screen.GetLayerRenderScale(0);
        Assert.That(current, Is.GreaterThan(1f));
        Assert.That(current, Is.LessThan(2f));
    }

    [Test]
    public void LayerRenderScaleSmoothing_DisablesSmoothUpdates()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);

        screen.SetLayerRenderScale(0, 1f);
        screen.SetLayerRenderScaleTarget(0, 2f, speed: 10f);
        screen.SetLayerRenderScaleSmoothing(0, enabled: false, speed: 1f);

        var gameTime = new LillyQuest.Core.Primitives.GameTime();
        gameTime.Update(0.1);
        screen.Update(gameTime);

        var current = screen.GetLayerRenderScale(0);
        Assert.That(current, Is.EqualTo(1f));
    }

    [Test]
    public void ClearLayer_EmptiesAllTiles()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.FillRectangle(0, 0, 2, 2, 9, LyColor.White);
        screen.ClearLayer(0);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(-1));
        Assert.That(screen.GetTile(0, 1, 1).TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void RightClick_CentersViewOnTile()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = System.Numerics.Vector2.Zero,
            Size = new System.Numerics.Vector2(200, 200),
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));
        screen.SetLayerPixelOffset(0, System.Numerics.Vector2.Zero);

        screen.TileMouseDown += (_, x, y, buttons) =>
        {
            if (buttons.Contains(Silk.NET.Input.MouseButton.Right))
            {
                screen.CenterViewOnTile(0, x, y);
            }
        };

        screen.OnMouseDown(125, 105, new[] { Silk.NET.Input.MouseButton.Right });

        var viewOffset = screen.GetLayerViewTileOffset(0);
        Assert.That(viewOffset.X, Is.EqualTo(2));
        Assert.That(viewOffset.Y, Is.EqualTo(0));
    }

    [Test]
    public void SmoothView_MovesTowardsTarget()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            TileRenderScale = 1.0f
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new System.Numerics.Vector2(10, 10));
        screen.SetLayerViewSmoothing(0, true, speed: 10f);

        screen.SetLayerViewTileOffset(0, System.Numerics.Vector2.Zero);
        screen.SetLayerViewTileTarget(0, new System.Numerics.Vector2(4, 0));

        var gameTime = new LillyQuest.Core.Primitives.GameTime();
        gameTime.Update(0.1);
        screen.Update(gameTime);

        var tileOffset = screen.GetLayerViewTileOffset(0);
        var pixelOffset = screen.GetLayerViewPixelOffset(0);

        Assert.That(tileOffset.X, Is.EqualTo(2));
        Assert.That(pixelOffset.X, Is.GreaterThan(0f));
    }
}

using LillyQuest.Core.Primitives;
using LillyQuest.Game.Screens.TilesetSurface;

namespace LillyQuest.Tests;

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
        screen.TileMouseMove += (_, x, y) => moveResult = (x, y);

        var downResult = (-1, -1);
        screen.TileMouseDown += (_, x, y, _) => downResult = (x, y);

        screen.OnMouseMove(25, 35);
        screen.OnMouseDown(25, 35, new[] { Silk.NET.Input.MouseButton.Left });

        Assert.That(moveResult, Is.EqualTo((2, 3)));
        Assert.That(downResult, Is.EqualTo((2, 3)));
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
}

using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TilesetSurfaceScreenMouseWheelTests
{
    private sealed class StubTilesetManager : ITilesetManager
    {
        public void Dispose() { }
        public Tileset GetTileset(string name) => throw new NotSupportedException();
        public bool HasTileset(string name) => false;
        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();
        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();
        public bool TryGetTileset(string name, out Tileset tileset)
        {
            tileset = null!;
            return false;
        }
        public IReadOnlyDictionary<string, Tileset> GetAllTilesets() => new Dictionary<string, Tileset>();
        public void UnloadTileset(string name) => throw new NotSupportedException();
    }

    [Test]
    public void OnMouseWheel_InsideScreen_ReturnsTrue()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 100),
            SelectedLayerIndex = 0
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));

        var result = screen.OnMouseWheel(5, 5, 1.5f);

        Assert.That(result, Is.True);
    }

    [Test]
    public void OnMouseWheel_InsideScreen_FiresTileMouseWheel()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 100),
            SelectedLayerIndex = 0
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));

        var calls = 0;
        var lastDelta = 0f;
        var lastX = -1;
        var lastY = -1;
        screen.TileMouseWheel += (layerIndex, tileX, tileY, delta) =>
                                 {
                                     calls++;
                                     lastX = tileX;
                                     lastY = tileY;
                                     lastDelta = delta;
                                 };

        screen.OnMouseWheel(5, 5, 1.5f);

        Assert.That(calls, Is.EqualTo(1));
        Assert.That(lastX, Is.EqualTo(0));
        Assert.That(lastY, Is.EqualTo(0));
        Assert.That(lastDelta, Is.EqualTo(1.5f));
    }

    [Test]
    public void OnMouseWheel_OutsideScreen_ReturnsFalse()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 100),
            SelectedLayerIndex = 0
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));

        var result = screen.OnMouseWheel(150, 5, 1.5f);

        Assert.That(result, Is.False);
    }
}

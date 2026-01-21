using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TilesetSurfaceScreenRenderScaleInputTests
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
    public void OnMouseMove_UsesLayerRenderScaleForInputCoordinates()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 100),
            SelectedLayerIndex = 0
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));
        screen.SetLayerRenderScale(0, 2f);

        var lastX = -1;
        var lastY = -1;
        screen.TileMouseMove += (_, x, y, _, _) =>
                                {
                                    lastX = x;
                                    lastY = y;
                                };

        screen.OnMouseMove(15, 5);

        Assert.That(lastX, Is.EqualTo(0));
        Assert.That(lastY, Is.EqualTo(0));
    }
}

using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceScreenCenterViewTests
{
    private sealed class StubTilesetManager : ITilesetManager
    {
        public void Dispose() { }

        public IReadOnlyDictionary<string, Tileset> GetAllTilesets()
            => new Dictionary<string, Tileset>();

        public Tileset GetTileset(string name)
            => throw new NotSupportedException();

        public bool HasTileset(string name)
            => false;

        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public bool TryGetTileset(string name, out Tileset tileset)
        {
            tileset = null!;

            return false;
        }

        public void UnloadTileset(string name)
            => throw new NotSupportedException();
    }

    [Test]
    public void CenterViewOnTile_UsesLayerRenderScale()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Size = new(100, 100),
            SelectedLayerIndex = 0,
            TileRenderScale = 1f
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));
        screen.SetLayerRenderScale(0, 2f);

        screen.CenterViewOnTile(0, 10, 10);

        var offset = screen.GetLayerViewTileOffset(0);
        Assert.That(offset.X, Is.EqualTo(7.5f));
        Assert.That(offset.Y, Is.EqualTo(7.5f));
    }
}

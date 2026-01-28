using System.Numerics;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Systems;
using NUnit.Framework;

namespace LillyQuest.Tests.Game.Systems;

public class ViewportUpdateSystemTests
{
    [Test]
    public void GetViewportBounds_UsesTileViewSizeAndOffset()
    {
        var screen = BuildTestSurface();
        screen.TileViewSize = new Vector2(10, 6);
        screen.SetLayerViewTileOffset(0, new Vector2(3, 4));

        var bounds = ViewportUpdateSystem.GetViewportBounds(screen, layerIndex: 0);

        Assert.That(bounds.MinX, Is.EqualTo(3));
        Assert.That(bounds.MinY, Is.EqualTo(4));
        Assert.That(bounds.MaxX, Is.EqualTo(12)); // 3 + 10 - 1
        Assert.That(bounds.MaxY, Is.EqualTo(9));  // 4 + 6 - 1
    }

    private static TilesetSurfaceScreen BuildTestSurface()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = 1
        };

        surface.InitializeLayers(surface.LayerCount);

        return surface;
    }

    private sealed class FakeTilesetManager : ITilesetManager
    {
        public void Dispose() { }

        public IReadOnlyDictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset> GetAllTilesets()
            => new Dictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset>();

        public LillyQuest.Core.Data.Assets.Tiles.Tileset GetTileset(string name)
            => throw new KeyNotFoundException();

        public bool HasTileset(string name)
            => false;

        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public bool TryGetTileset(string name, out LillyQuest.Core.Data.Assets.Tiles.Tileset tileset)
        {
            tileset = null!;
            return false;
        }

        public void UnloadTileset(string name)
            => throw new NotSupportedException();
    }
}

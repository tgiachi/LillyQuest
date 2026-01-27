using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceScreenViewLockTests
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
    public void SetLayerViewLock_PropagatesViewTargetsToFollower()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(2);

        screen.SetLayerViewLock(0, 1);
        screen.SetLayerViewTileTarget(0, new Vector2(5, 7));
        screen.SetLayerViewPixelTarget(0, new Vector2(2, 3));

        Assert.That(screen.GetLayerViewLockMaster(1), Is.EqualTo(0));
        Assert.That(screen.GetLayerViewTileTarget(1), Is.EqualTo(new Vector2(5, 7)));
        Assert.That(screen.GetLayerViewPixelTarget(1), Is.EqualTo(new Vector2(2, 3)));
    }

    [Test]
    public void SetLayerViewLock_RejectsChain()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(3);

        screen.SetLayerViewLock(0, 1);
        screen.SetLayerViewLock(1, 2);

        Assert.That(screen.GetLayerViewLockMaster(2), Is.Null);
    }
}

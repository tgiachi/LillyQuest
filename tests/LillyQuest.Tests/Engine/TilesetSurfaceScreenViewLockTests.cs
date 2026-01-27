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
    public void SetLayerViewLock_UpdatesFollowerOffsetsWhenSmoothingDisabled()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(2);

        screen.SetLayerViewLock(0, 1);
        screen.SetLayerViewTileOffset(0, new Vector2(4, 6));
        screen.SetLayerViewPixelOffset(0, new Vector2(3, 5));

        Assert.That(screen.GetLayerViewTileOffset(1), Is.EqualTo(new Vector2(4, 6)));
        Assert.That(screen.GetLayerViewPixelOffset(1), Is.EqualTo(new Vector2(3, 5)));
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

    [Test]
    public void CenterViewOnTile_PropagatesTargetsToLockedFollowers()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Size = new Vector2(100, 100),
            TileRenderScale = 1f
        };
        screen.InitializeLayers(2);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));
        screen.SetLayerInputTileSizeOverride(1, new Vector2(10, 10));
        screen.SetLayerViewSmoothing(0, true);
        screen.SetLayerViewLock(0, 1);

        screen.CenterViewOnTile(0, 10, 10);

        Assert.That(screen.GetLayerViewTileTarget(1), Is.EqualTo(screen.GetLayerViewTileTarget(0)));
        Assert.That(screen.GetLayerViewPixelTarget(1), Is.EqualTo(screen.GetLayerViewPixelTarget(0)));
    }

    [Test]
    public void UpdateViewSmoothing_KeepsFollowersInLockstepWithMaster()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Size = new Vector2(100, 100),
            TileRenderScale = 1f
        };
        screen.InitializeLayers(2);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));
        screen.SetLayerInputTileSizeOverride(1, new Vector2(10, 10));
        screen.SetLayerViewSmoothing(0, true, 10f);
        screen.SetLayerViewLock(0, 1);

        screen.CenterViewOnTile(0, 10, 10);

        var gameTime = new LillyQuest.Core.Primitives.GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));
        screen.Update(gameTime);

        Assert.That(screen.GetLayerViewTileOffset(1), Is.EqualTo(screen.GetLayerViewTileOffset(0)));
        Assert.That(screen.GetLayerViewPixelOffset(1), Is.EqualTo(screen.GetLayerViewPixelOffset(0)));
    }
}

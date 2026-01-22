using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TileMovementTests
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
    public void EnqueueMove_AllowsOccupiedDestination_ReplacesOnComplete()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.AddTileToSurface(0, 1, 1, new(5, LyColor.White));
        screen.AddTileToSurface(0, 2, 2, new(9, LyColor.White));

        var result = screen.EnqueueMove(0, new(1, 1), new(2, 2), 0.1f);

        var gameTime = new GameTime();
        gameTime.Update(0.2);
        screen.Update(gameTime);

        Assert.That(result, Is.True);
        Assert.That(screen.GetTile(0, 2, 2).TileIndex, Is.EqualTo(5));
    }

    [Test]
    public void EnqueueMove_BounceRestoresDestinationTile()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.AddTileToSurface(0, 1, 1, new(5, LyColor.White));
        screen.AddTileToSurface(0, 2, 2, new(9, LyColor.White));

        var result = screen.EnqueueMove(0, new(1, 1), new(2, 2), 1.0f, true);

        var gameTime = new GameTime();
        gameTime.Update(0.1);
        screen.Update(gameTime);
        Assert.That(screen.GetTile(0, 2, 2).TileIndex, Is.EqualTo(-1));

        gameTime.Update(1.0);
        screen.Update(gameTime);
        gameTime.Update(1.0);
        screen.Update(gameTime);

        Assert.That(result, Is.True);
        Assert.That(screen.GetTile(0, 1, 1).TileIndex, Is.EqualTo(5));
        Assert.That(screen.GetTile(0, 2, 2).TileIndex, Is.EqualTo(9));
    }
}

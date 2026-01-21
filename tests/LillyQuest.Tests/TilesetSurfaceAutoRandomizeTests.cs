using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests;

public class TilesetSurfaceAutoRandomizeTests
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
    public void AutoRandomize_EveryTenFrames_RandomizesTiles()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        dynamic dynScreen = screen;
        dynScreen.ConfigureAutoRandomize(tileCount: 5, everyFrames: 10, random: new Random(123));

        var gameTime = new GameTime();

        for (var i = 0; i < 9; i++)
        {
            gameTime.Update(1.0 / 60.0);
            screen.Update(gameTime);
        }

        var before = dynScreen.GetTile(0, 0, 0);
        Assert.That(before.TileIndex, Is.EqualTo(-1));

        gameTime.Update(1.0 / 60.0);
        screen.Update(gameTime);

        var after = dynScreen.GetTile(0, 0, 0);
        Assert.That(after.TileIndex, Is.GreaterThanOrEqualTo(0));
    }
}

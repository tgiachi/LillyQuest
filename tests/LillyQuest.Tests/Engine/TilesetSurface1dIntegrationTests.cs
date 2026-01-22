using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Extensions.TilesetSurface;
using LillyQuest.Engine.Screens.TilesetSurface;

namespace LillyQuest.Tests.Engine;

public class TilesetSurface1dIntegrationTests
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
    public void DrawText_StillWritesTilesCorrectly()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager());
        screen.InitializeLayers(1);
        screen.SelectedLayerIndex = 0;

        screen.DrawText("A", 0, 0, LyColor.White);

        Assert.That(screen.GetTile(0, 0, 0).TileIndex, Is.EqualTo(65));
    }
}

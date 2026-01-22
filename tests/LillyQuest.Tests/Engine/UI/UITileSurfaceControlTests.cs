using LillyQuest.Engine.Screens.UI;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.UI;

public class UITileSurfaceControlTests
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
    public void TileSurfaceControl_RendersWithoutException()
    {
        var control = new UITileSurfaceControl(new StubTilesetManager(), 10, 10);

        Assert.DoesNotThrow(() => control.Render(null, null));
    }
}

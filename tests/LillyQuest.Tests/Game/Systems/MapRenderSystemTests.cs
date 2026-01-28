using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Game.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.Engine.Screens.TilesetSurface;
using NUnit.Framework;

namespace LillyQuest.Tests.Game.Systems;

public class MapRenderSystemTests
{
    [Test]
    public void RegisterMap_InitializesDirtyTracker()
    {
        var system = new MapRenderSystem(chunkSize: 16);
        var map = new LyQuestMap(32, 32);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var fovService = new FOVService();

        system.RegisterMap(map, surface, fovService);

        Assert.That(system.HasMap(map), Is.True);
    }

    private sealed class FakeTilesetManager : ITilesetManager
    {
        public void Dispose() { }

        public IReadOnlyDictionary<string, Tileset> GetAllTilesets()
            => new Dictionary<string, Tileset>();

        public Tileset GetTileset(string name)
            => throw new KeyNotFoundException();

        public bool HasTileset(string name)
            => false;

        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotImplementedException();

        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotImplementedException();

        public bool TryGetTileset(string name, out Tileset tileset)
        {
            tileset = null!;
            return false;
        }

        public void UnloadTileset(string name)
            => throw new NotImplementedException();
    }
}

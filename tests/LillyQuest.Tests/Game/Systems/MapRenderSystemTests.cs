using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.Game.Systems;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Types;
using NUnit.Framework;
using SadRogue.Primitives;

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

    [Test]
    public void OnObjectMoved_MarksOldAndNewChunksDirty()
    {
        var system = new MapRenderSystem(chunkSize: 16);
        var map = new LyQuestMap(32, 32);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());

        system.RegisterMap(map, surface, fovService: null);

        system.HandleObjectMoved(map, new Point(1, 1), new Point(17, 1));

        var dirty = system.GetDirtyChunks(map);
        Assert.That(dirty, Does.Contain(new ChunkCoord(0, 0)));
        Assert.That(dirty, Does.Contain(new ChunkCoord(1, 0)));
    }

    [Test]
    public void Update_RebuildsDirtyChunks()
    {
        var system = new MapRenderSystem(chunkSize: 4);
        var map = BuildSmallTestMap();
        var surface = BuildTestSurface();

        system.RegisterMap(map, surface, fovService: null);

        surface.AddTileToSurface(0, 0, 0, new TileRenderData(-1, LyColor.White));
        system.MarkDirtyForTile(map, 0, 0);

        system.Update(new GameTime());

        Assert.That(surface.GetTile(0, 0, 0).TileIndex, Is.Not.EqualTo(-1));
    }

    private static LyQuestMap BuildSmallTestMap()
    {
        var map = new LyQuestMap(4, 4);
        var terrain = new TerrainGameObject(new Point(0, 0))
        {
            Tile = new VisualTile("floor", "A", LyColor.Black, LyColor.White)
        };

        map.SetTerrain(terrain);

        return map;
    }

    private static TilesetSurfaceScreen BuildTestSurface()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };

        surface.InitializeLayers(surface.LayerCount);

        return surface;
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

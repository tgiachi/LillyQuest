using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Rendering;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.Game.Systems;

public class MapRenderSystemTests
{
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

    [Test]
    public void ChangingEntityTile_MarksChunkDirty()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, null);
        var map = new LyQuestMap(8, 8);
        var creature = new CreatureGameObject(new(1, 1))
        {
            Tile = new("creature", "@", LyColor.Black, LyColor.White)
        };

        map.AddEntity(creature);
        system.OnMapRegistered(map);

        Assert.That(system.GetDirtyChunks(map), Is.Empty);

        creature.Tile = new("creature_alt", "&", LyColor.Black, LyColor.White);

        var dirty = system.GetDirtyChunks(map);
        Assert.That(dirty, Does.Contain(new ChunkCoord(0, 0)));
    }

    [Test]
    public void ChangingTerrainTile_MarksChunkDirty()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, null);
        var map = new LyQuestMap(8, 8);
        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = new("floor", ".", LyColor.Black, LyColor.White)
        };

        map.SetTerrain(terrain);
        system.OnMapRegistered(map);

        Assert.That(system.GetDirtyChunks(map), Is.Empty);

        terrain.Tile = new("floor_alt", ",", LyColor.Black, LyColor.White);

        var dirty = system.GetDirtyChunks(map);
        Assert.That(dirty, Does.Contain(new ChunkCoord(0, 0)));
    }

    [Test]
    public void OnObjectMoved_MarksOldAndNewChunksDirty()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var system = new MapRenderSystem(16, new MapTileBuilder(), surface, null);
        var map = new LyQuestMap(32, 32);

        system.OnMapRegistered(map);

        system.HandleObjectMoved(map, new(1, 1), new(17, 1));

        var dirty = system.GetDirtyChunks(map);
        Assert.That(dirty, Does.Contain(new ChunkCoord(0, 0)));
        Assert.That(dirty, Does.Contain(new ChunkCoord(1, 0)));
    }

    [Test]
    public void ReassigningSameTerrainTileReference_DoesNotMarkDirty()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, null);
        var map = new LyQuestMap(8, 8);
        var tile = new VisualTile("floor", ".", LyColor.Black, LyColor.White);
        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = tile
        };

        map.SetTerrain(terrain);
        system.OnMapRegistered(map);

        Assert.That(system.GetDirtyChunks(map), Is.Empty);

        terrain.Tile = tile;

        Assert.That(system.GetDirtyChunks(map), Is.Empty);
    }

    [Test]
    public void RegisterMap_InitializesDirtyTracker()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager());
        var fovSystem = new FovSystem();
        var system = new MapRenderSystem(16, new MapTileBuilder(), surface, fovSystem);
        var map = new LyQuestMap(32, 32);

        system.OnMapRegistered(map);

        Assert.That(system.HasMap(map), Is.True);
    }

    [Test]
    public void Update_RebuildsDirtyChunks()
    {
        var surface = BuildTestSurface();
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, null);
        var map = BuildSmallTestMap();

        system.OnMapRegistered(map);

        surface.AddTileToSurface(0, 0, 0, new(-1, LyColor.White));
        system.MarkDirtyForTile(map, 0, 0);

        system.Update(new());

        Assert.That(surface.GetTile(0, 0, 0).TileIndex, Is.Not.EqualTo(-1));
    }

    [Test]
    public void Update_RendersItemLayerTile()
    {
        var surface = BuildTestSurface();
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, null);
        var map = BuildSmallTestMap();
        var item = new ItemGameObject(new(1, 1))
        {
            Tile = new("torch", "t", LyColor.Black, LyColor.Yellow)
        };

        map.AddEntity(item);
        system.OnMapRegistered(map);

        surface.AddTileToSurface((int)MapLayer.Items, 1, 1, new(-1, LyColor.White));
        system.MarkDirtyForTile(map, 1, 1);

        system.Update(new());

        Assert.That(surface.GetTile((int)MapLayer.Items, 1, 1).TileIndex, Is.EqualTo('t'));
    }

    [Test]
    public void Update_WithFovFalloff_DarkensVisibleItemTile()
    {
        var surface = BuildTestSurface();
        var fovSystem = new FovSystem(5);
        var system = new MapRenderSystem(4, new MapTileBuilder(), surface, fovSystem);
        var map = new LyQuestMap(12, 12);
        fovSystem.RegisterMap(map);
        var floorTile = new VisualTile("floor", ".", LyColor.Black, LyColor.White);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                map.SetTerrain(
                    new TerrainGameObject(new(x, y))
                    {
                        Tile = floorTile
                    }
                );
            }
        }

        var item = new ItemGameObject(new(6, 11))
        {
            Tile = new("torch", "t", LyColor.Yellow, LyColor.Black)
        };
        map.AddEntity(item);

        fovSystem.UpdateFov(map, new(6, 6));
        system.OnMapRegistered(map);

        system.MarkDirtyForTile(map, 6, 11);
        system.Update(new());

        var rendered = surface.GetTile((int)MapLayer.Items, 6, 11);
        Assert.That(rendered.ForegroundColor, Is.Not.EqualTo(item.Tile.ForegroundColor));
    }

    private static LyQuestMap BuildSmallTestMap()
    {
        var map = new LyQuestMap(4, 4);
        var terrain = new TerrainGameObject(new(0, 0))
        {
            Tile = new("floor", "A", LyColor.Black, LyColor.White)
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
}

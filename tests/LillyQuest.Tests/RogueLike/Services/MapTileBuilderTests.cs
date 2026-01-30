using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Rendering;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using SadRogue.Primitives;

namespace LillyQuest.Tests.RogueLike.Services;

public class MapTileBuilderTests
{
    [Test]
    public void BuildCreatureTile_WithoutFov_ReturnsCreatureTile()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var creature = new CreatureGameObject(new(2, 2))
        {
            Tile = new("creature", "@", LyColor.Red, LyColor.Black)
        };
        map.AddEntity(creature);

        var tile = builder.BuildCreatureTile(map, null, new(2, 2));

        Assert.That(tile.TileIndex, Is.EqualTo('@'));
        Assert.That(tile.ForegroundColor, Is.EqualTo(LyColor.Red));
        Assert.That(tile.BackgroundColor, Is.EqualTo(LyColor.Black));
    }

    [Test]
    public void BuildCreatureTile_WithVisibleCreature_ReturnsTile()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var creature = new CreatureGameObject(new(2, 2))
        {
            Tile = new("creature", "@", LyColor.Red, LyColor.Black)
        };
        map.AddEntity(creature);

        var fov = new FovSystem(3);
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(2, 2));  // Center FOV on creature

        var tile = builder.BuildCreatureTile(map, fov, new(2, 2));

        Assert.That(tile.TileIndex, Is.EqualTo('@'));
    }

    [Test]
    public void BuildCreatureTile_WithInvisibleCreature_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var creature = new CreatureGameObject(new(2, 2))
        {
            Tile = new("creature", "@", LyColor.Red, LyColor.Black)
        };
        map.AddEntity(creature);

        var fov = new FovSystem(1);  // Small radius
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(6, 6));  // FOV far from creature

        var tile = builder.BuildCreatureTile(map, fov, new(2, 2));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void BuildCreatureTile_NoCreature_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);

        var tile = builder.BuildCreatureTile(map, null, new(2, 2));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void BuildItemTile_WithoutFov_ReturnsItemTile()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var item = new ItemGameObject(new(3, 3))
        {
            Tile = new("item", "*", LyColor.Yellow, LyColor.Black)
        };
        map.AddEntity(item);

        var tile = builder.BuildItemTile(map, null, new(3, 3));

        Assert.That(tile.TileIndex, Is.EqualTo('*'));
        Assert.That(tile.ForegroundColor, Is.EqualTo(LyColor.Yellow));
    }

    [Test]
    public void BuildItemTile_WithVisibleItem_ReturnsTile()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var item = new ItemGameObject(new(3, 3))
        {
            Tile = new("item", "*", LyColor.Yellow, LyColor.Black)
        };
        map.AddEntity(item);

        var fov = new FovSystem(3);
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(3, 3));

        var tile = builder.BuildItemTile(map, fov, new(3, 3));

        Assert.That(tile.TileIndex, Is.EqualTo('*'));
    }

    [Test]
    public void BuildItemTile_WithInvisibleItem_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var item = new ItemGameObject(new(3, 3))
        {
            Tile = new("item", "*", LyColor.Yellow, LyColor.Black)
        };
        map.AddEntity(item);

        var fov = new FovSystem(1);
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(6, 6));  // FOV far from item

        var tile = builder.BuildItemTile(map, fov, new(3, 3));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void BuildItemTile_NoItem_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);

        var tile = builder.BuildItemTile(map, null, new(3, 3));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void BuildTerrainTile_WithoutFov_ReturnsTerrain()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var terrain = new TerrainGameObject(new(1, 1))
        {
            Tile = new("floor", ".", LyColor.Gray, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var tile = builder.BuildTerrainTile(map, null, new(1, 1));

        Assert.That(tile.TileIndex, Is.EqualTo('.'));
        Assert.That(tile.ForegroundColor, Is.EqualTo(LyColor.Gray));
    }

    [Test]
    public void BuildTerrainTile_WithoutFov_NoTerrain_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);

        var tile = builder.BuildTerrainTile(map, null, new(1, 1));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }

    [Test]
    public void BuildTerrainTile_WithExploredVisible_ReturnsTerrain()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = new("floor", ".", LyColor.Gray, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var fov = new FovSystem(3);
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(2, 2));

        var tile = builder.BuildTerrainTile(map, fov, new(2, 2));

        Assert.That(tile.TileIndex, Is.EqualTo('.'));
    }

    [Test]
    public void BuildTerrainTile_WithExploredNotVisible_ReturnsDarkened()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var terrain = new TerrainGameObject(new(3, 3))
        {
            Tile = new("floor", ".", LyColor.Gray, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var fov = new FovSystem(1);  // Small radius
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(3, 3));  // Center on (3,3) to make it explored
        // Now move FOV away to make (3,3) explored but not visible
        fov.UpdateFov(map, new(6, 6));

        var tile = builder.BuildTerrainTile(map, fov, new(3, 3));

        // Should still see terrain since it was explored
        Assert.That(tile.TileIndex, Is.EqualTo('.'));
        // Should be darkened since not currently visible
        Assert.That(tile.ForegroundColor.R, Is.LessThan(LyColor.Gray.R));
    }

    [Test]
    public void BuildTerrainTile_WithUnexplored_ReturnsEmpty()
    {
        var builder = new MapTileBuilder();
        var map = new LyQuestMap(8, 8);
        var terrain = new TerrainGameObject(new(4, 4))
        {
            Tile = new("floor", ".", LyColor.Gray, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var fov = new FovSystem(1);
        fov.RegisterMap(map);
        fov.UpdateFov(map, new(0, 0));  // FOV far from (4,4)

        var tile = builder.BuildTerrainTile(map, fov, new(4, 4));

        Assert.That(tile.TileIndex, Is.EqualTo(-1));
    }
}

using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.Game.Systems;

public class LightOverlaySystemTests
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
    public void MarkDirtyForRadius_WithFlickerRadiusJitter_MarksExpandedRange()
    {
        var map = new LyQuestMap(10, 10);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var terrain = new TerrainGameObject(new(5, 5))
        {
            Tile = new("floor", ".", LyColor.White, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var torch = new ItemGameObject(new(5, 5))
        {
            Tile = new("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(2, LyColor.Yellow, LyColor.Black));
        torch.GoRogueComponents.Add(
            new LightFlickerComponent(
                LightFlickerMode.Random,
                0.5f,
                2f,
                8f
            )
        );
        map.AddEntity(torch);

        var system = new LightOverlaySystem(4);
        system.Configure(surface, null);
        system.OnMapRegistered(map);

        system.MarkDirtyForRadius(map, torch.Position, 4);

        system.Update(new());

        Assert.That(surface.GetTile((int)MapLayer.Effects, 5, 5).TileIndex, Is.EqualTo('.'));
    }

    [Test]
    public void Update_RendersLightBackgroundToEffectsLayer_WhenVisible()
    {
        var map = new LyQuestMap(8, 8);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);

        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = new("floor", ".", LyColor.White, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var torch = new ItemGameObject(new(2, 2))
        {
            Tile = new("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(3, LyColor.Yellow, LyColor.Black));
        torch.GoRogueComponents.Add(new LightBackgroundComponent(LyColor.Orange, LyColor.Transparent));
        map.AddEntity(torch);

        fovSystem.UpdateFov(map, torch.Position);

        var system = new LightOverlaySystem(4);
        system.Configure(surface, fovSystem);
        system.OnMapRegistered(map);

        system.MarkDirtyForRadius(map, torch.Position, 3);
        system.Update(new());

        // Alpha is capped at MaxBackgroundAlpha (128) by LightOverlaySystem
        var expectedColor = LyColor.Orange.WithAlpha(128);
        Assert.That(surface.GetTile((int)MapLayer.Effects, 2, 2).BackgroundColor, Is.EqualTo(expectedColor));
    }

    [Test]
    public void Update_RendersLightBackgroundWithDistanceAlphaFalloff()
    {
        var map = new LyQuestMap(8, 8);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);

        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = new("floor", ".", LyColor.White, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var torch = new ItemGameObject(new(2, 2))
        {
            Tile = new("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(3, LyColor.Yellow, LyColor.Black));
        torch.GoRogueComponents.Add(new LightBackgroundComponent(LyColor.Orange, LyColor.Transparent));
        map.AddEntity(torch);

        fovSystem.UpdateFov(map, torch.Position);

        var system = new LightOverlaySystem(4);
        system.Configure(surface, fovSystem);
        system.OnMapRegistered(map);

        system.MarkDirtyForRadius(map, torch.Position, 3);
        system.Update(new());

        var background = surface.GetTile((int)MapLayer.Effects, 2, 2).BackgroundColor;
        Assert.That(background.A, Is.EqualTo(128));
    }

    [Test]
    public void Update_RendersLightToEffectsLayer_WhenVisible()
    {
        var map = new LyQuestMap(8, 8);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);

        var terrain = new TerrainGameObject(new(2, 2))
        {
            Tile = new("floor", ".", LyColor.White, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var torch = new ItemGameObject(new(2, 2))
        {
            Tile = new("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(3, LyColor.Yellow, LyColor.Black));
        map.AddEntity(torch);

        fovSystem.UpdateFov(map, torch.Position);

        var system = new LightOverlaySystem(4);
        system.Configure(surface, fovSystem);
        system.OnMapRegistered(map);

        system.MarkDirtyForRadius(map, torch.Position, 3);
        system.Update(new());

        Assert.That(surface.GetTile((int)MapLayer.Effects, 2, 2).TileIndex, Is.EqualTo('.'));
        Assert.That(surface.GetTile((int)MapLayer.Effects, 2, 2).ForegroundColor, Is.EqualTo(LyColor.Yellow));
    }

    [Test]
    public void Update_WithFlickerComponent_ChangesRenderedColor()
    {
        var map = new LyQuestMap(10, 10);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var fovSystem = new FovSystem();
        fovSystem.RegisterMap(map);

        var terrain = new TerrainGameObject(new(5, 5))
        {
            Tile = new("floor", ".", LyColor.White, LyColor.Black)
        };
        map.SetTerrain(terrain);

        var torch = new ItemGameObject(new(5, 5))
        {
            Tile = new("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(3, LyColor.Yellow, LyColor.Black));
        torch.GoRogueComponents.Add(
            new LightFlickerComponent(
                LightFlickerMode.Deterministic,
                0.5f,
                0f,
                8f,
                42
            )
        );
        map.AddEntity(torch);

        fovSystem.UpdateFov(map, torch.Position);

        var system = new LightOverlaySystem(4);
        system.Configure(surface, fovSystem);
        system.OnMapRegistered(map);

        system.MarkDirtyForRadius(map, torch.Position, 3);
        system.Update(new(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1)));
        var first = surface.GetTile((int)MapLayer.Effects, 5, 5).ForegroundColor;

        system.MarkDirtyForRadius(map, torch.Position, 3);
        system.Update(new(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.1)));
        var second = surface.GetTile((int)MapLayer.Effects, 5, 5).ForegroundColor;

        Assert.That(second, Is.Not.EqualTo(first));
    }
}

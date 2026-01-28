using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Systems;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Types;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.Game.Systems;

public class LightOverlaySystemTests
{
    [Test]
    public void Update_RendersLightToEffectsLayer_WhenVisible()
    {
        var map = new LyQuestMap(8, 8);
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = Enum.GetNames<MapLayer>().Length
        };
        surface.InitializeLayers(surface.LayerCount);

        var fov = new FOVService();
        fov.Initialize(map);

        var torch = new ItemGameObject(new Point(2, 2))
        {
            Tile = new VisualTile("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        torch.GoRogueComponents.Add(new LightSourceComponent(radius: 3, startColor: LyColor.Yellow, endColor: LyColor.Black));
        map.AddEntity(torch);

        fov.UpdateFOV(torch.Position);

        var system = new LightOverlaySystem(chunkSize: 4);
        system.RegisterMap(map, surface, fov);

        system.MarkDirtyForRadius(map, center: torch.Position, radius: 3);
        system.Update(new GameTime());

        Assert.That(surface.GetTile((int)MapLayer.Effects, 2, 2).TileIndex, Is.EqualTo(0));
        Assert.That(surface.GetTile((int)MapLayer.Effects, 2, 2).ForegroundColor, Is.EqualTo(LyColor.Yellow));
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

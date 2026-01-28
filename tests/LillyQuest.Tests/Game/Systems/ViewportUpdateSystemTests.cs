using System.Numerics;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.Game.Systems;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.GameObjects;
using LillyQuest.RogueLike.Maps;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.Game.Systems;

public class ViewportUpdateSystemTests
{
    [Test]
    public void GetViewportBounds_UsesTileViewSizeAndOffset()
    {
        var screen = BuildTestSurface();
        screen.TileViewSize = new Vector2(10, 6);
        screen.SetLayerViewTileOffset(0, new Vector2(3, 4));

        var bounds = ViewportUpdateSystem.GetViewportBounds(screen, layerIndex: 0);

        Assert.That(bounds.MinX, Is.EqualTo(3));
        Assert.That(bounds.MinY, Is.EqualTo(4));
        Assert.That(bounds.MaxX, Is.EqualTo(12)); // 3 + 10 - 1
        Assert.That(bounds.MaxY, Is.EqualTo(9));  // 4 + 6 - 1
    }

    [Test]
    public void Update_OnlyUpdatesObjectsInsideViewport_AndMarksDirty()
    {
        var map = BuildTestMap();
        var screen = BuildTestSurface();
        screen.TileViewSize = new Vector2(4, 4);
        screen.SetLayerViewTileOffset(0, new Vector2(0, 0));

        var renderSystem = new MapRenderSystem(chunkSize: 4);
        renderSystem.RegisterMap(map, screen, fovService: null);

        var system = new ViewportUpdateSystem(layerIndex: 0);
        system.RegisterMap(map, screen, renderSystem);

        var inside = new TestViewportObject(new Point(1, 1));
        var outside = new TestViewportObject(new Point(10, 10));
        map.AddEntity(inside);
        map.AddEntity(outside);

        system.Update(new GameTime());

        Assert.That(inside.UpdateCount, Is.EqualTo(1));
        Assert.That(outside.UpdateCount, Is.EqualTo(0));
        Assert.That(renderSystem.GetDirtyChunks(map), Does.Contain(new ChunkCoord(0, 0)));
    }

    private static LyQuestMap BuildTestMap()
        => new(20, 20);

    private static TilesetSurfaceScreen BuildTestSurface()
    {
        var surface = new TilesetSurfaceScreen(new FakeTilesetManager())
        {
            LayerCount = 1
        };

        surface.InitializeLayers(surface.LayerCount);

        return surface;
    }

    private sealed class TestViewportObject : CreatureGameObject, IViewportUpdateable
    {
        public int UpdateCount { get; private set; }

        public TestViewportObject(Point position) : base(position) { }

        public void Update(GameTime gameTime)
            => UpdateCount++;
    }

    private sealed class FakeTilesetManager : ITilesetManager
    {
        public void Dispose() { }

        public IReadOnlyDictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset> GetAllTilesets()
            => new Dictionary<string, LillyQuest.Core.Data.Assets.Tiles.Tileset>();

        public LillyQuest.Core.Data.Assets.Tiles.Tileset GetTileset(string name)
            => throw new KeyNotFoundException();

        public bool HasTileset(string name)
            => false;

        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public bool TryGetTileset(string name, out LillyQuest.Core.Data.Assets.Tiles.Tileset tileset)
        {
            tileset = null!;
            return false;
        }

        public void UnloadTileset(string name)
            => throw new NotSupportedException();
    }
}

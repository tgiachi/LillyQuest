using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Systems;

namespace LillyQuest.Tests.Game.Systems;

public class ViewportUpdateSystemTests
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
    public void GetViewportBounds_UsesTileViewSizeAndOffset()
    {
        var screen = BuildTestSurface();
        screen.TileViewSize = new(10, 6);
        screen.SetLayerViewTileOffset(0, new(3, 4));

        var bounds = ViewportUpdateSystem.GetViewportBounds(screen, 0);

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
        screen.TileViewSize = new(4, 4);
        screen.SetLayerViewTileOffset(0, new(0, 0));

        var renderSystem = new MapRenderSystem(4);
        renderSystem.Configure(screen, null);
        renderSystem.OnMapRegistered(map);

        var system = new ViewportUpdateSystem(0);
        system.Configure(screen, renderSystem);
        system.OnMapRegistered(map);

        var insideUpdateCount = 0;
        var outsideUpdateCount = 0;

        var inside = new CreatureGameObject(new(1, 1));
        inside.GoRogueComponents.Add(
            new AnimationComponent(
                0.1,
                () => insideUpdateCount++
            )
        );

        var outside = new CreatureGameObject(new(10, 10));
        outside.GoRogueComponents.Add(
            new AnimationComponent(
                0.1,
                () => outsideUpdateCount++
            )
        );

        map.AddEntity(inside);
        map.AddEntity(outside);

        // Update with enough elapsed time to trigger the animation
        system.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)));

        Assert.That(insideUpdateCount, Is.EqualTo(1));
        Assert.That(outsideUpdateCount, Is.EqualTo(0));
        Assert.That(renderSystem.GetDirtyChunks(map).Count, Is.GreaterThan(0));
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
}

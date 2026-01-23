using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;

namespace LillyQuest.Tests.Engine.UI;

public class UITileSurfaceControlTests
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
    public void AutoSizeFromTileView_Disabled_WritesSurfaceSize()
    {
        var control = new UITileSurfaceControl(new StubTilesetManager(), 10, 10)
        {
            AutoSizeFromTileView = false,
            Size = new(200, 80)
        };

        control.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));

        Assert.That(control.Surface.Size, Is.EqualTo(new Vector2(200, 80)));
    }

    [Test]
    public void AutoSizeFromTileView_UpdatesControlSize()
    {
        var control = new UITileSurfaceControl(new StubTilesetManager(), 10, 10)
        {
            AutoSizeFromTileView = true
        };
        control.Surface.Size = new(123, 45);

        control.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));

        Assert.That(control.Size, Is.EqualTo(new Vector2(123, 45)));
    }

    [Test]
    public void MouseDown_Forwards_To_Surface()
    {
        var control = new UITileSurfaceControl(new StubTilesetManager(), 10, 10)
        {
            AutoSizeFromTileView = false,
            Position = new(10, 20),
            Size = new(100, 100)
        };
        var invoked = false;
        control.Surface.TileMouseDown += (_, _, _, _) => invoked = true;

        var handled = control.HandleMouseDown(new(15, 25), new[] { MouseButton.Left });

        Assert.That(handled, Is.True);
        Assert.That(invoked, Is.True);
    }

    [Test]
    public void TileSurfaceControl_RendersWithoutException()
    {
        var control = new UITileSurfaceControl(new StubTilesetManager(), 10, 10);

        Assert.DoesNotThrow(() => control.Render(null, null));
    }
}

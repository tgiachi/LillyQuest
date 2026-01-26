using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.TilesetSurface;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceScreenAutoScaleTests
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
    public void ComputeLayerRenderScaleForTileView_UsesMaxAxisForCrop()
    {
        var scale = TilesetSurfaceScreen.ComputeLayerRenderScaleForTileView(
            new Vector2(80, 25),
            new Vector2(1024, 768),
            12,
            12,
            1f,
            Vector4.Zero
        );

        Assert.That(scale, Is.EqualTo(2.56f).Within(0.0001f));
    }

    [Test]
    public void ComputeLayerRenderScaleForTileView_AccountsForMargins()
    {
        var scale = TilesetSurfaceScreen.ComputeLayerRenderScaleForTileView(
            new Vector2(80, 25),
            new Vector2(1024, 768),
            12,
            12,
            1f,
            new Vector4(10, 20, 30, 40)
        );

        var expectedWidthScale = (1024f - 10f - 30f) / (80f * 12f);
        var expectedHeightScale = (768f - 20f - 40f) / (25f * 12f);
        var expected = MathF.Max(expectedWidthScale, expectedHeightScale);

        Assert.That(scale, Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void ApplyTileViewScaleToScreen_UpdatesLayerRenderScale()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            TileViewSize = new Vector2(80, 25),
            TileRenderScale = 1f
        };
        screen.InitializeLayers(1);
        screen.SetLayerInputTileSizeOverride(0, new Vector2(10, 10));

        screen.ApplyTileViewScaleToScreen(new Vector2(1024, 768), includeMargins: true);

        var expected = MathF.Max(1024f / (80f * 10f), 768f / (25f * 10f));
        Assert.That(screen.GetLayerRenderScale(0), Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void TileViewSize_DoesNotOverrideSize_WhenSizeAlreadySet()
    {
        var screen = new TilesetSurfaceScreen(new StubTilesetManager())
        {
            Size = new Vector2(1604, 1350),
            TileRenderScale = 1f,
            AutoSizeFromTileView = false
        };
        SetPrivateTileset(screen, CreateTilesetStub(12, 12));

        screen.TileViewSize = new Vector2(80, 30);

        Assert.That(screen.Size, Is.EqualTo(new Vector2(1604, 1350)));
    }

    private static void SetPrivateTileset(TilesetSurfaceScreen screen, Tileset tileset)
    {
        var field = typeof(TilesetSurfaceScreen).GetField("_tileset", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(screen, tileset);
    }

    private static Tileset CreateTilesetStub(int tileWidth, int tileHeight)
    {
        var tileset = (Tileset)FormatterServices.GetUninitializedObject(typeof(Tileset));
        SetTilesetField(tileset, "<TileWidth>k__BackingField", tileWidth);
        SetTilesetField(tileset, "<TileHeight>k__BackingField", tileHeight);
        return tileset;
    }

    private static void SetTilesetField(Tileset tileset, string fieldName, int value)
    {
        var field = typeof(Tileset).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(tileset, value);
    }
}

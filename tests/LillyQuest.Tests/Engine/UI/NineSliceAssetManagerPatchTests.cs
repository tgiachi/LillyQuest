using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;

namespace LillyQuest.Tests.Engine.UI;

public class NineSliceAssetManagerPatchTests
{
    private sealed class FakeTextureManager : ITextureManager
    {
        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

        public void Dispose() { }

        public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
            => throw new NotSupportedException();

        public Texture2D GetTexture(string assetName)
            => throw new NotSupportedException();

        public bool HasTexture(string assetName)
            => false;

        public void LoadTexture(string assetName, string filePath)
            => throw new NotSupportedException();

        public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
            => throw new NotSupportedException();

        public void LoadTextureFromPng(string assetName, Span<byte> pngData)
            => throw new NotSupportedException();

        public void LoadTextureFromPngWithChromaKey(string assetName, Span<byte> pngData, byte tolerance = 0)
            => throw new NotSupportedException();

        public void LoadTextureWithChromaKey(string assetName, string filePath, byte tolerance = 0)
            => throw new NotSupportedException();

        public bool TryGetTexture(string assetName, out Texture2D texture)
        {
            texture = null!;

            return false;
        }

        public void UnloadTexture(string assetName)
            => throw new NotSupportedException();
    }

    [Test]
    public void RegisterTexturePatches_StoresAndRetrieves()
    {
        var textureManager = new FakeTextureManager();
        var manager = new NineSliceAssetManager(textureManager);
        var patches = new[]
        {
            new TexturePatchDefinition("scroll.track", new(0, 0, 16, 64)),
            new TexturePatchDefinition("scroll.thumb", new(16, 0, 16, 32))
        };

        manager.RegisterTexturePatches("ui_atlas", patches);

        var track = manager.GetTexturePatch("ui_atlas", "scroll.track");
        var thumb = manager.GetTexturePatch("ui_atlas", "scroll.thumb");

        Assert.That(track.TextureName, Is.EqualTo("ui_atlas"));
        Assert.That(track.ElementName, Is.EqualTo("scroll.track"));
        Assert.That(track.Section.Size.X, Is.EqualTo(16));
        Assert.That(track.Section.Size.Y, Is.EqualTo(64));

        Assert.That(thumb.TextureName, Is.EqualTo("ui_atlas"));
        Assert.That(thumb.ElementName, Is.EqualTo("scroll.thumb"));
        Assert.That(thumb.Section.Origin.X, Is.EqualTo(16));
        Assert.That(thumb.Section.Size.Y, Is.EqualTo(32));
    }
}

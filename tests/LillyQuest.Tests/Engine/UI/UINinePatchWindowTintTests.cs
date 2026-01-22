using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UINinePatchWindowTintTests
{
    [Test]
    public void TintDefaults_AreWhite()
    {
        var textureManager = new FakeTextureManager();
        var nineSliceManager = new NineSliceAssetManager(textureManager);
        var window = new UINinePatchWindow(nineSliceManager, textureManager);

        Assert.That(window.BorderTint, Is.EqualTo(LyColor.White));
        Assert.That(window.CenterTint, Is.EqualTo(LyColor.White));
    }

    private sealed class FakeTextureManager : ITextureManager
    {
        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

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

        public void Dispose()
        {
        }
    }
}

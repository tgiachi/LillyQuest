using FontStashSharp;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Utils;

namespace LillyQuest.Tests.Core;

public class FontManagerMeasureTextTests
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
    public void MeasureText_UsesDefaultDrawScale()
    {
        var textureManager = new FakeTextureManager();
        var fontManager = new FontManager(textureManager);
        var data = ResourceUtils.GetEmbeddedResourceContent("Assets/Fonts/default_font.ttf", typeof(ResourceUtils).Assembly);
        fontManager.LoadFont("default_font", data);

        var settings = new FontSystemSettings
        {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2
        };
        var fontSystem = new FontSystem(settings);
        using var stream = new MemoryStream(data.ToArray());
        fontSystem.AddFont(stream);
        var font = fontSystem.GetFont(14);
        var baseSize = font.MeasureString("Hello");
        var expected = baseSize * 2f;

        var fontRef = new FontRef("default_font", 14, FontKind.TrueType);
        var handle = fontManager.GetFontHandle(fontRef);
        var measured = handle.MeasureText("Hello");

        Assert.Throws<KeyNotFoundException>(() => fontManager.GetFontHandle(new("missing", 12, FontKind.TrueType)));

        Assert.That(measured.X, Is.EqualTo(expected.X).Within(0.01f));
        Assert.That(measured.Y, Is.EqualTo(expected.Y).Within(0.01f));
    }
}

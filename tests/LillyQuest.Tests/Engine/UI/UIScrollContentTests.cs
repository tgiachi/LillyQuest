using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class UIScrollContentTests
{
    [Test]
    public void ViewportAndThumb_ComputeExpectedSizes()
    {
        var control = new UIScrollContent(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Size = new Vector2(200, 100),
            ContentSize = new Vector2(400, 300),
            EnableVerticalScroll = true,
            EnableHorizontalScroll = true,
            ScrollbarThickness = 10f,
            MinThumbSize = 16f
        };

        var viewport = control.GetViewportBounds();
        Assert.That(viewport.Size.X, Is.EqualTo(190f));
        Assert.That(viewport.Size.Y, Is.EqualTo(90f));

        var vThumb = control.GetVerticalThumbRect();
        Assert.That(vThumb.Size.Y, Is.GreaterThanOrEqualTo(16f));
        var hThumb = control.GetHorizontalThumbRect();
        Assert.That(hThumb.Size.X, Is.GreaterThanOrEqualTo(16f));
    }

    private sealed class FakeNineSliceManager : INineSliceAssetManager
    {
        public void RegisterNineSlice(string key, string textureName, Rectangle<int> sourceRect, Vector4D<float> margins)
            => throw new NotSupportedException();

        public void LoadNineSlice(string key, string textureName, string filePath, Rectangle<int> sourceRect, Vector4D<float> margins)
            => throw new NotSupportedException();

        public void LoadNineSlice(
            string key,
            string textureName,
            Span<byte> data,
            uint width,
            uint height,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
            => throw new NotSupportedException();

        public void LoadNineSliceFromPng(
            string key,
            string textureName,
            Span<byte> pngData,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
            => throw new NotSupportedException();

        public void RegisterTexturePatches(string textureName, IReadOnlyList<TexturePatchDefinition> patches)
            => throw new NotSupportedException();

        public TexturePatch GetTexturePatch(string textureName, string elementName)
            => throw new NotSupportedException();

        public bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch)
        {
            patch = default;
            return false;
        }

        public NineSliceDefinition GetNineSlice(string key)
            => throw new NotSupportedException();

        public bool TryGetNineSlice(string key, out NineSliceDefinition definition)
        {
            definition = default;
            return false;
        }
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

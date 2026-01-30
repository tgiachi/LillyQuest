using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class UIProgressBarTests
{
    private sealed class FakeNineSliceManager : INineSliceAssetManager
    {
        public NineSliceDefinition GetNineSlice(string key)
            => throw new NotSupportedException();

        public TexturePatch GetTexturePatch(string textureName, string elementName)
            => throw new NotSupportedException();

        public void LoadNineSlice(
            string key,
            string textureName,
            string filePath,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
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

        public void RegisterNineSlice(string key, string textureName, Rectangle<int> sourceRect, Vector4D<float> margins)
            => throw new NotSupportedException();

        public void RegisterTexturePatches(string textureName, IReadOnlyList<TexturePatchDefinition> patches)
            => throw new NotSupportedException();

        public bool TryGetNineSlice(string key, out NineSliceDefinition definition)
        {
            definition = default;

            return false;
        }

        public bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch)
        {
            patch = default;

            return false;
        }
    }

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
    public void ProgressBar_Clamps_Value_To_Min_Max()
    {
        var bar = new UIProgressBar(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Min = 0f,
            Max = 100f,
            Value = 150f,
            Size = new(100, 10)
        };

        Assert.That(bar.NormalizedValue, Is.EqualTo(1f));
    }

    [Test]
    public void ProgressBar_Computes_Fill_For_Vertical_BottomUp()
    {
        var bar = new UIProgressBar(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Min = 0f,
            Max = 1f,
            Value = 0.5f,
            Orientation = ProgressOrientation.Vertical,
            Size = new(20, 100)
        };

        Assert.That(bar.GetFillSize(), Is.EqualTo(new Vector2(20, 50)));
        Assert.That(bar.GetFillOrigin().Y, Is.EqualTo(bar.GetWorldPosition().Y + 50));
    }

    [Test]
    public void ProgressBar_Text_Shows_Percent_When_Enabled()
    {
        var bar = new UIProgressBar(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Min = 0f,
            Max = 1f,
            Value = 0.75f,
            ShowText = true
        };

        Assert.That(bar.GetDisplayText(), Is.EqualTo("75%"));
    }
}

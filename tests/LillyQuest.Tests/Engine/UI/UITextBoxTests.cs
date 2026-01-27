using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class UITextBoxTests
{
    [Test]
    public void TextBox_Defaults_To_NineSliceScale_2()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager());
        Assert.That(box.NineSliceScale, Is.EqualTo(2f));
    }

    [Test]
    public void TextBox_Defaults_CenterTint_To_White()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager());
        Assert.That(box.CenterTint, Is.EqualTo(LyColor.White));
    }

    [Test]
    public void TextBox_Defaults_CursorColor_To_White()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager());
        Assert.That(box.CursorColor, Is.EqualTo(LyColor.White));
    }

    [Test]
    public void TextBox_Computes_Smaller_NineSlice_Scale_When_Height_Is_Tight()
    {
        var scale = UITextBox.ComputeNineSliceAxisScale(2f, 10f, 4f, 4f);
        Assert.That(scale, Is.EqualTo(1.25f));
    }

    [Test]
    public void TextBox_AutoHeight_Uses_Font_Measure()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Font = new("default_font", 14, FontKind.TrueType),
            AutoHeightEnabled = true,
            VerticalPadding = 1f
        };

        box.ApplyAutoHeight(new Vector2(10, 18));
        Assert.That(box.Size.Y, Is.EqualTo(20));
    }

    [Test]
    public void TextBox_Inserts_Characters_And_Backspace_Works()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager());

        box.HandleTextInput('a');
        box.HandleTextInput('b');
        Assert.That(box.Text, Is.EqualTo("ab"));

        box.HandleBackspace();
        Assert.That(box.Text, Is.EqualTo("a"));
    }

    [Test]
    public void TextBox_ArrowKeys_Move_Cursor()
    {
        var box = new UITextBox(new FakeNineSliceManager(), new FakeTextureManager())
        {
            Text = "abc",
            CursorIndex = 3
        };

        box.HandleKeyPress(KeyModifierType.None, new List<Key> { Key.Left });
        Assert.That(box.CursorIndex, Is.EqualTo(2));

        box.HandleKeyPress(KeyModifierType.None, new List<Key> { Key.Right });
        Assert.That(box.CursorIndex, Is.EqualTo(3));
    }

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
}

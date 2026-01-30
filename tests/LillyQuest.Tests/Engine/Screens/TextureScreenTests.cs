using System.Numerics;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.Textures;

namespace LillyQuest.Tests.Engine.Screens;

public class TextureScreenTests
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

    private sealed class TestTextureScreen : TextureScreen
    {
        private readonly Vector2 _textureSize;

        public TestTextureScreen(ITextureManager textureManager, Vector2 textureSize)
            : base(textureManager)
            => _textureSize = textureSize;

        public (Vector2 position, Vector2 size) GetPlacement()
            => ComputeTexturePlacement(_textureSize);

        protected override Vector2 GetTextureSize()
            => _textureSize;
    }

    [Test]
    public void ComputeTexturePlacement_Centers_Texture_Without_Stretch()
    {
        var screen = new TestTextureScreen(new FakeTextureManager(), new(40, 20))
        {
            Size = new(100, 60)
        };

        var placement = screen.GetPlacement();

        Assert.That(placement.position, Is.EqualTo(new Vector2(30, 20)));
        Assert.That(placement.size, Is.EqualTo(new Vector2(40, 20)));
    }

    [Test]
    public void ComputeTexturePlacement_DoesNot_Resize_Large_Texture()
    {
        var screen = new TestTextureScreen(new FakeTextureManager(), new(120, 80))
        {
            Size = new(100, 60)
        };

        var placement = screen.GetPlacement();

        Assert.That(placement.size, Is.EqualTo(new Vector2(120, 80)));
        Assert.That(placement.position, Is.EqualTo(new Vector2(-10, -10)));
    }
}

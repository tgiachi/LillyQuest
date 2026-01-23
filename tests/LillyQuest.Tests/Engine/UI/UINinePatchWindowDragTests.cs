using System.Numerics;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UINinePatchWindowDragTests
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
    public void HandleMouseDrag_MovesWindow_WhenEnabled()
    {
        var textureManager = new FakeTextureManager();
        var nineSliceManager = new NineSliceAssetManager(textureManager);
        var window = new UINinePatchWindow(nineSliceManager, textureManager)
        {
            Position = new(10, 10),
            Size = new(100, 100),
            IsTitleBarEnabled = true,
            IsWindowMovable = true,
            TitleBarHeight = 10f
        };

        var pressed = window.HandleMouseDown(new(15, 15));
        var moved = window.HandleMouseMove(new(30, 40));
        var released = window.HandleMouseUp(new(30, 40));

        Assert.That(pressed, Is.True);
        Assert.That(moved, Is.True);
        Assert.That(released, Is.True);
        Assert.That(window.Position, Is.EqualTo(new Vector2(25, 35)));

        window.HandleMouseMove(new(60, 60));
        Assert.That(window.Position, Is.EqualTo(new Vector2(25, 35)));
    }
}

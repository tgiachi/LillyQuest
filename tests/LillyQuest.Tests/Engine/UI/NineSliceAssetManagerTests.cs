using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using NUnit.Framework;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class NineSliceAssetManagerTests
{
    [Test]
    public void RegisterNineSlice_ComputesRects()
    {
        var manager = new NineSliceAssetManager(new FakeTextureManager());
        manager.RegisterNineSlice(
            "window",
            "ui",
            new Rectangle<int>(0, 0, 32, 32),
            new Vector4D<float>(8, 8, 8, 8)
        );

        var def = manager.GetNineSlice("window");

        Assert.That(def.Center.Size.X, Is.EqualTo(16));
        Assert.That(def.Center.Size.Y, Is.EqualTo(16));
    }

    [Test]
    public void LoadNineSlice_FilePath_LoadsTextureAndRegisters()
    {
        var textureManager = new FakeTextureManager();
        var manager = new NineSliceAssetManager(textureManager);

        manager.LoadNineSlice(
            "window",
            "ui",
            "path/to/texture.png",
            new Rectangle<int>(0, 0, 8, 8),
            new Vector4D<float>(2, 2, 2, 2)
        );

        Assert.That(textureManager.LastFilePath, Is.EqualTo("path/to/texture.png"));
        Assert.That(textureManager.LastTextureName, Is.EqualTo("ui"));
        Assert.That(manager.TryGetNineSlice("window", out _), Is.True);
    }

    [Test]
    public void LoadNineSlice_RawData_LoadsTextureAndRegisters()
    {
        var textureManager = new FakeTextureManager();
        var manager = new NineSliceAssetManager(textureManager);
        var data = new byte[4];

        manager.LoadNineSlice(
            "window",
            "ui",
            data,
            1,
            1,
            new Rectangle<int>(0, 0, 1, 1),
            new Vector4D<float>(0, 0, 0, 0)
        );

        Assert.That(textureManager.LastTextureName, Is.EqualTo("ui"));
        Assert.That(textureManager.LastWidth, Is.EqualTo(1u));
        Assert.That(textureManager.LastHeight, Is.EqualTo(1u));
        Assert.That(textureManager.LastDataLength, Is.EqualTo(4));
        Assert.That(manager.TryGetNineSlice("window", out _), Is.True);
    }

    private sealed class FakeTextureManager : ITextureManager
    {
        public string? LastTextureName { get; private set; }
        public string? LastFilePath { get; private set; }
        public uint LastWidth { get; private set; }
        public uint LastHeight { get; private set; }
        public int LastDataLength { get; private set; }

        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

        public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
            => throw new NotSupportedException();

        public Texture2D GetTexture(string assetName)
            => throw new NotSupportedException();

        public bool HasTexture(string assetName)
            => false;

        public void LoadTexture(string assetName, string filePath)
        {
            LastTextureName = assetName;
            LastFilePath = filePath;
        }

        public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
        {
            LastTextureName = assetName;
            LastWidth = width;
            LastHeight = height;
            LastDataLength = data.Length;
        }

        public void LoadTextureFromPng(string assetName, Span<byte> pngData)
        {
            LastTextureName = assetName;
            LastDataLength = pngData.Length;
        }

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

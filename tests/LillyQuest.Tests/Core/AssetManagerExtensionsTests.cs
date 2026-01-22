using System.Reflection;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Utils;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Core;

public class AssetManagerExtensionsTests
{
    [Test]
    public void LoadNineSliceFromEmbeddedResource_RegistersDefinitionAndLoadsTexture()
    {
        var textureManager = new FakeTextureManager();
        var nineSliceManager = new NineSliceAssetManager();
        var assetManager = new FakeAssetManager(textureManager, nineSliceManager);

        assetManager.LoadNineSliceFromEmbeddedResource(
            "ui_panel",
            "ui_panel_texture",
            "Assets/_9patch/simple_ui.png",
            new Rectangle<int>(0, 0, 32, 32),
            new Vector4D<float>(4f, 4f, 4f, 4f),
            typeof(ResourceUtils).Assembly
        );

        Assert.That(textureManager.LoadedTextureNames, Does.Contain("ui_panel_texture"));
        Assert.That(nineSliceManager.TryGetNineSlice("ui_panel", out var definition), Is.True);
        Assert.That(definition.TextureName, Is.EqualTo("ui_panel_texture"));
        Assert.That(definition.Center.Size.X, Is.EqualTo(24));
        Assert.That(definition.Center.Size.Y, Is.EqualTo(24));
    }

    private sealed class FakeAssetManager : IAssetManager
    {
        public FakeAssetManager(ITextureManager textureManager, INineSliceAssetManager nineSliceManager)
        {
            TextureManager = textureManager;
            NineSliceManager = nineSliceManager;
        }

        public ITextureManager TextureManager { get; }
        public IFontManager FontManager => throw new NotSupportedException();
        public IShaderManager ShaderManager => throw new NotSupportedException();
        public IAudioManager AudioManager => throw new NotSupportedException();
        public ITilesetManager TilesetManager => throw new NotSupportedException();
        public INineSliceAssetManager NineSliceManager { get; }

        public void Dispose()
        {
        }
    }

    private sealed class FakeTextureManager : ITextureManager
    {
        private readonly HashSet<string> _loadedNames = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<string> LoadedTextureNames => _loadedNames;

        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

        public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
            => throw new NotSupportedException();

        public Texture2D GetTexture(string assetName)
            => throw new NotSupportedException();

        public bool HasTexture(string assetName)
            => _loadedNames.Contains(assetName);

        public void LoadTexture(string assetName, string filePath)
            => throw new NotSupportedException();

        public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
            => throw new NotSupportedException();

        public void LoadTextureFromPng(string assetName, Span<byte> pngData)
        {
            _loadedNames.Add(assetName);
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

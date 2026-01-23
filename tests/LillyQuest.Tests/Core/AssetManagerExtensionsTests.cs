using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Utils;

namespace LillyQuest.Tests.Core;

public class AssetManagerExtensionsTests
{
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

        public void Dispose() { }
    }

    private sealed class FakeTextureManager : ITextureManager
    {
        private readonly HashSet<string> _loadedNames = new(StringComparer.OrdinalIgnoreCase);

        public uint LastWidth { get; private set; }
        public uint LastHeight { get; private set; }

        public IReadOnlyCollection<string> LoadedTextureNames => _loadedNames;

        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

        public void Dispose() { }

        public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
            => throw new NotSupportedException();

        public Texture2D GetTexture(string assetName)
            => throw new NotSupportedException();

        public bool HasTexture(string assetName)
            => _loadedNames.Contains(assetName);

        public void LoadTexture(string assetName, string filePath)
            => throw new NotSupportedException();

        public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
        {
            _loadedNames.Add(assetName);
            LastWidth = width;
            LastHeight = height;
        }

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
    }

    [Test]
    public void LoadNineSliceFromEmbeddedResource_RegistersDefinitionAndLoadsTexture()
    {
        var textureManager = new FakeTextureManager();
        var nineSliceManager = new NineSliceAssetManager(textureManager);
        var assetManager = new FakeAssetManager(textureManager, nineSliceManager);

        assetManager.LoadNineSliceFromEmbeddedResource(
            "ui_panel",
            "Assets/_9patch/simple_ui.png",
            new(4f, 4f, 4f, 4f),
            typeof(ResourceUtils).Assembly
        );

        Assert.That(textureManager.LoadedTextureNames, Does.Contain("n9_ui_ui_panel"));
        Assert.That(nineSliceManager.TryGetNineSlice("ui_panel", out var definition), Is.True);
        Assert.That(definition.TextureName, Is.EqualTo("n9_ui_ui_panel"));
        Assert.That(textureManager.LastWidth, Is.GreaterThan(0u));
        Assert.That(textureManager.LastHeight, Is.GreaterThan(0u));
        Assert.That(definition.Center.Size.X, Is.EqualTo((int)textureManager.LastWidth - 8));
        Assert.That(definition.Center.Size.Y, Is.EqualTo((int)textureManager.LastHeight - 8));
    }
}

using LillyQuest.Core.Interfaces.Assets;

namespace LillyQuest.Core.Managers.Assets;

/// <summary>
/// Central asset manager facade that aggregates all specialized asset managers.
/// Provides unified access to texture, font, shader, audio, and tileset management.
/// </summary>
public class AssetManager : IAssetManager
{
    public ITextureManager TextureManager { get; }

    public IFontManager FontManager { get; }

    public IShaderManager ShaderManager { get; }

    public IAudioManager AudioManager { get; }

    public ITilesetManager TilesetManager { get; }

    public INineSliceAssetManager NineSliceManager { get; }

    public AssetManager(
        ITextureManager textureManager,
        IFontManager fontManager,
        IShaderManager shaderManager,
        IAudioManager audioManager,
        ITilesetManager tilesetManager,
        INineSliceAssetManager nineSliceManager
    )
    {
        TextureManager = textureManager;
        FontManager = fontManager;
        ShaderManager = shaderManager;
        AudioManager = audioManager;
        TilesetManager = tilesetManager;
        NineSliceManager = nineSliceManager;
    }

    public void Dispose()
    {
        TextureManager?.Dispose();
        FontManager?.Dispose();
        TilesetManager?.Dispose();
        AudioManager?.Shutdown();
        GC.SuppressFinalize(this);
    }
}

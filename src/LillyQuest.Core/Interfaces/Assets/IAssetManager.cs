namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Central asset management facade that provides access to all specialized asset managers.
/// Handles loading, caching, and retrieval of all game assets (textures, fonts, shaders, audio, tilesets).
/// </summary>
public interface IAssetManager : IDisposable
{
    /// <summary>
    /// Gets the texture manager for handling texture assets.
    /// </summary>
    ITextureManager TextureManager { get; }

    /// <summary>
    /// Gets the font manager for handling font assets.
    /// </summary>
    IFontManager FontManager { get; }

    /// <summary>
    /// Gets the shader manager for handling shader assets.
    /// </summary>
    IShaderManager ShaderManager { get; }

    /// <summary>
    /// Gets the audio manager for handling audio assets and playback.
    /// </summary>
    IAudioManager AudioManager { get; }

    /// <summary>
    /// Gets the tileset manager for handling tileset assets.
    /// </summary>
    ITilesetManager TilesetManager { get; }
}

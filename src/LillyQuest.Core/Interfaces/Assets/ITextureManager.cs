using LillyQuest.Core.Graphics.OpenGL.Resources;

namespace LillyQuest.Core.Interfaces.Assets;

public interface ITextureManager : IDisposable
{
    Texture2D DefaultWhiteTexture { get; }
    Texture2D DefaultBlackTexture { get; }

    /// <summary>
    /// Gets all loaded textures.
    /// </summary>
    IReadOnlyDictionary<string, Texture2D> GetAllTextures();

    Texture2D GetTexture(string assetName);
    bool HasTexture(string assetName);

    void LoadTexture(string assetName, string filePath);
    void LoadTexture(string assetName, Span<byte> data, uint width, uint height);
    void LoadTextureFromPng(string assetName, Span<byte> pngData);

    /// <summary>
    /// Loads a texture from PNG data with magenta chroma key replacement.
    /// Magenta pixels (255, 0, 255) are replaced with fully transparent pixels.
    /// </summary>
    /// <param name="assetName">Unique name for the texture</param>
    /// <param name="pngData">PNG image data</param>
    /// <param name="tolerance">Color tolerance for magenta detection (0-255). Default: 0 for exact match</param>
    void LoadTextureFromPngWithChromaKey(string assetName, Span<byte> pngData, byte tolerance = 0);

    /// <summary>
    /// Loads a texture from file with magenta chroma key replacement.
    /// Magenta pixels (255, 0, 255) are replaced with fully transparent pixels.
    /// </summary>
    /// <param name="assetName">Unique name for the texture</param>
    /// <param name="filePath">Path to the texture file</param>
    /// <param name="tolerance">Color tolerance for magenta detection (0-255). Default: 0 for exact match</param>
    void LoadTextureWithChromaKey(string assetName, string filePath, byte tolerance = 0);

    bool TryGetTexture(string assetName, out Texture2D texture);

    void UnloadTexture(string assetName);
}

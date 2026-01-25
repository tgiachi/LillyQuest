using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Defines font loading, caching, and retrieval operations.
/// </summary>
public interface IFontManager : IDisposable
{
    /// <summary>
    /// Returns a font handle for the requested font reference.
    /// </summary>
    IFontHandle GetFontHandle(FontRef fontRef);

    /// <summary>
    /// Returns true when the font is already loaded.
    /// </summary>
    bool HasFont(string assetName);

    /// <summary>
    /// Loads a bitmap font from disk and stores it with the given asset name.
    /// </summary>
    void LoadBmpFont(
        string assetName,
        string filePath,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        LyColor? transparentColor = null,
        string? characterMap = null
    );

    /// <summary>
    /// Loads a bitmap font from a stream and stores it with the given asset name.
    /// </summary>
    void LoadBmpFont(
        string assetName,
        Span<byte> data,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        LyColor? transparentColor = null,
        string? characterMap = null
    );

    /// <summary>
    /// Loads a font from disk and stores it with the given asset name.
    /// </summary>
    void LoadFont(string assetName, string filePath);

    /// <summary>
    /// Loads a font from raw data and stores it with the given asset name.
    /// </summary>
    void LoadFont(string assetName, Span<byte> data);

    /// <summary>
    /// Attempts to retrieve a font handle for the requested font reference.
    /// </summary>
    bool TryGetFontHandle(FontRef fontRef, out IFontHandle handle);

    /// <summary>
    /// Unloads a font by its asset name.
    /// </summary>
    void UnloadFont(string assetName);
}

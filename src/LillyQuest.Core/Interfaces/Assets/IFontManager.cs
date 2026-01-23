using System.Numerics;
using FontStashSharp;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Defines font loading, caching, and retrieval operations.
/// </summary>
public interface IFontManager : IDisposable
{
    /// <summary>
    /// Returns a loaded bitmap font by its asset name.
    /// </summary>
    BitmapFont GetBitmapFont(string assetName);

    /// <summary>
    /// Returns a loaded font by its asset name.
    /// </summary>
    DynamicSpriteFont GetFont(string assetName, int size);

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
    /// Attempts to retrieve a bitmap font by its asset name.
    /// </summary>
    bool TryGetBitmapFont(string assetName, out BitmapFont font);

    /// <summary>
    /// Attempts to retrieve a font handle by its asset name.
    /// </summary>
    bool TryGetFont(string assetName, out DynamicSpriteFont font);

    /// <summary>
    /// Unloads a font by its asset name.
    /// </summary>
    void UnloadFont(string assetName);

    /// <summary>
    ///  Measures the rendered size of the given text using the specified font asset and size.
    /// </summary>
    /// <param name="fontAssetName"></param>
    /// <param name="fontSize"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    Vector2 MeasureText(string fontAssetName, int fontSize, string text);
}

using System.Numerics;
using FontStashSharp;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LillyQuest.Core.Managers.Assets;

public class FontManager : IFontManager
{
    private readonly ILogger _logger = Log.ForContext<FontManager>();
    private readonly ITextureManager _textureManager;

    private static readonly int[] _defaultFontsSizes = [12, 14, 16, 18, 20, 24, 30, 36, 48, 60, 72, 96];

    private readonly FontSystemSettings _fontSettings = new()
    {
        FontResolutionFactor = 2,
        KernelWidth = 2,
        KernelHeight = 2
    };

    private readonly Dictionary<string, FontSystem> _fonts = new();
    private readonly Dictionary<string, DynamicSpriteFont> _loadedFonts = new();
    private readonly Dictionary<string, BitmapFont> _bitmapFonts = new();

    public FontManager(ITextureManager textureManager)
        => _textureManager = textureManager;

    public void Dispose()
    {
        foreach (var fontSystem in _fonts.Values)
        {
            fontSystem.Dispose();
        }

        _fonts.Clear();
        _loadedFonts.Clear();

        foreach (var bitmapFont in _bitmapFonts.Values)
        {
            _textureManager.UnloadTexture(bitmapFont.Name);
        }

        _bitmapFonts.Clear();

        GC.SuppressFinalize(this);
    }

    public BitmapFont GetBitmapFont(string assetName)
        => _bitmapFonts.TryGetValue(assetName, out var font)
               ? font
               : throw new KeyNotFoundException($"Bitmap font not found: {assetName}");

    public DynamicSpriteFont GetFont(string assetName, int size)
    {
        var key = $"{assetName}_{size}";

        if (_loadedFonts.TryGetValue(key, out var font))
        {
            return font;
        }

        var generatedFont = _fonts.GetValueOrDefault(assetName)?.GetFont(size);

        if (generatedFont != null)
        {
            _loadedFonts[key] = generatedFont;

            return generatedFont;
        }

        throw new KeyNotFoundException($"Font not found: {assetName} with size {size}");
    }

    public bool HasFont(string assetName)
        => _fonts.ContainsKey(assetName);

    public void LoadBmpFont(
        string assetName,
        string filePath,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        LyColor? transparentColor = null,
        string? characterMap = null
    )
    {
        ArgumentNullException.ThrowIfNull(assetName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileHeight);

        if (!File.Exists(filePath))
        {
            _logger.Error("Bitmap font file not found: {FontPath}", filePath);

            throw new FileNotFoundException("Bitmap font file not found", filePath);
        }

        var data = File.ReadAllBytes(filePath);
        LoadBmpFont(assetName, data, tileWidth, tileHeight, spacing, transparentColor, characterMap);
    }

    public void LoadBmpFont(
        string assetName,
        Span<byte> data,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        LyColor? transparentColor = null,
        string? characterMap = null
    )
    {
        ArgumentNullException.ThrowIfNull(assetName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileHeight);

        if (data.Length == 0)
        {
            throw new ArgumentException("Bitmap font data cannot be empty.", nameof(data));
        }

        if (_bitmapFonts.ContainsKey(assetName))
        {
            _logger.Warning("Bitmap font with name {FontName} already loaded.", assetName);

            return;
        }

        using var image = Image.Load<Rgba32>(data);
        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        var transparent = transparentColor ?? LyColor.Magenta;

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var r = pixelData[i];
            var g = pixelData[i + 1];
            var b = pixelData[i + 2];

            if (r == transparent.R && g == transparent.G && b == transparent.B)
            {
                pixelData[i + 3] = 0;
            }
        }

        _textureManager.LoadTexture(assetName, pixelData, (uint)image.Width, (uint)image.Height);
        var texture = _textureManager.GetTexture(assetName);
        var bitmapFont = new BitmapFont(assetName, texture, tileWidth, tileHeight, spacing, characterMap);

        _bitmapFonts[assetName] = bitmapFont;

        _logger.Information("Bitmap font {FontName} loaded successfully", assetName);
    }

    public void LoadFont(string assetName, string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("Font file not found: {FontPath}", filePath);

            throw new FileNotFoundException("Font file not found", filePath);
        }

        using var fontStream = File.OpenRead(filePath);
        LoadFont(assetName, fontStream);
    }

    public void LoadFont(string assetName, Span<byte> data)
    {
        var fontSystem = new FontSystem(_fontSettings);

        using var stream = new MemoryStream(data.ToArray());
        fontSystem.AddFont(stream);

        _fonts[assetName] = fontSystem;

        foreach (var size in _defaultFontsSizes)
        {
            var fntWithSize = fontSystem.GetFont(size);
            _loadedFonts[$"{assetName}_{size}"] = fntWithSize;
        }

        _fonts[assetName] = fontSystem;

        _logger.Information("Font {FontName} loaded successfully", assetName);
    }

    public void LoadFont(string name, Stream fontStream)
    {
        var fontSystem = new FontSystem(_fontSettings);

        fontSystem.AddFont(fontStream);

        _fonts[name] = fontSystem;

        foreach (var size in _defaultFontsSizes)
        {
            var fntWithSize = fontSystem.GetFont(size);
            _loadedFonts[$"{name}_{size}"] = fntWithSize;
        }

        _fonts[name] = fontSystem;

        _logger.Information("Font {FontName} loaded successfully", name);
    }

    public Vector2 MeasureText(string fontAssetName, int fontSize, string text)
    {
        var font = GetFont(fontAssetName, fontSize);
        var dimensions = font.MeasureString(text);

        return new Vector2(dimensions.X, dimensions.Y) * 2f;
    }

    public bool TryGetBitmapFont(string assetName, out BitmapFont font)
        => _bitmapFonts.TryGetValue(assetName, out font);

    public bool TryGetFont(string assetName, out DynamicSpriteFont font)
        => _loadedFonts.TryGetValue(assetName, out font);

    public void UnloadFont(string assetName)
    {
        if (_fonts.Remove(assetName, out var fontSystem))
        {
            fontSystem.Dispose();

            var keysToRemove = _loadedFonts.Keys
                                           .Where(key => key.StartsWith($"{assetName}_"))
                                           .ToList();

            foreach (var key in keysToRemove)
            {
                _loadedFonts.Remove(key);
            }

            _logger.Information("Font {FontName} unloaded successfully", assetName);
        }
        else
        {
            _logger.Warning("Font {FontName} not found for unloading", assetName);
        }
    }
}

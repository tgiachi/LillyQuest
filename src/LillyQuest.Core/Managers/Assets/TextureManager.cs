using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using Serilog;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LillyQuest.Core.Managers.Assets;

public class TextureManager : ITextureManager
{
    private readonly ILogger _logger = Log.ForContext<TextureManager>();

    private readonly GL _gl;

    private readonly Dictionary<string, Texture2D> _textures = new();

    public TextureManager(EngineRenderContext context)
    {
        _gl = context.Gl;

        PrepareWhiteAndBlackTextures();
    }

    public Texture2D DefaultWhiteTexture { get; private set; }
    public Texture2D DefaultBlackTexture { get; private set; }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
        => _textures.AsReadOnly();

    public Texture2D GetTexture(string assetName)
        => _textures.TryGetValue(assetName, out var texture)
               ? texture
               : throw new KeyNotFoundException($"Texture with asset name {assetName} not found.");

    public bool HasTexture(string assetName)
        => _textures.ContainsKey(assetName);

    public void LoadTexture(string assetName, string filePath)
    {
        if (_textures.ContainsKey(assetName))
        {
            _logger.Warning("Texture with asset name {AssetName} already loaded.", assetName);

            return;
        }

        var texture = new Texture2D(_gl, filePath);
        _textures[assetName] = texture;
        _logger.Information("Texture {AssetName} loaded from {FilePath}.", assetName, filePath);
    }

    public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
    {
        if (_textures.ContainsKey(assetName))
        {
            _logger.Warning("Texture with asset name {AssetName} already loaded.", assetName);

            return;
        }

        var texture = new Texture2D(_gl, data, width, height);
        _textures[assetName] = texture;
        _logger.Information(
            "Texture {AssetName} loaded from data with dimensions {Width}x{Height}.",
            assetName,
            width,
            height
        );
    }

    public void LoadTextureFromPng(string assetName, Span<byte> pngData)
    {
        if (_textures.ContainsKey(assetName))
        {
            _logger.Warning("Texture with asset name {AssetName} already loaded.", assetName);

            return;
        }

        if (pngData.Length == 0)
        {
            throw new ArgumentException("PNG data cannot be empty.", nameof(pngData));
        }

        using var image = Image.Load<Rgba32>(pngData);
        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        var texture = new Texture2D(_gl, pixelData, (uint)image.Width, (uint)image.Height);
        _textures[assetName] = texture;

        _logger.Information(
            "Texture {AssetName} loaded from PNG data with dimensions {Width}x{Height}.",
            assetName,
            image.Width,
            image.Height
        );
    }

    public void LoadTextureFromPngWithChromaKey(string assetName, Span<byte> pngData, byte tolerance = 0)
    {
        if (_textures.ContainsKey(assetName))
        {
            _logger.Warning("Texture with asset name {AssetName} already loaded.", assetName);

            return;
        }

        if (pngData.Length == 0)
        {
            throw new ArgumentException("PNG data cannot be empty.", nameof(pngData));
        }

        using var image = Image.Load<Rgba32>(pngData);
        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        // Replace magenta (255, 0, 255) with transparent
        ReplaceChromaKey(pixelData, tolerance);

        var texture = new Texture2D(_gl, pixelData, (uint)image.Width, (uint)image.Height);
        _textures[assetName] = texture;

        _logger.Information(
            "Texture {AssetName} loaded from PNG data with chroma key replacement and dimensions {Width}x{Height}.",
            assetName,
            image.Width,
            image.Height
        );
    }

    public void LoadTextureWithChromaKey(string assetName, string filePath, byte tolerance = 0)
    {
        if (_textures.ContainsKey(assetName))
        {
            _logger.Warning("Texture with asset name {AssetName} already loaded.", assetName);

            return;
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Texture file not found: {filePath}");
        }

        using var image = Image.Load<Rgba32>(filePath);
        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        // Replace magenta (255, 0, 255) with transparent
        ReplaceChromaKey(pixelData, tolerance);

        var texture = new Texture2D(_gl, pixelData, (uint)image.Width, (uint)image.Height);
        _textures[assetName] = texture;

        _logger.Information(
            "Texture {AssetName} loaded from {FilePath} with chroma key replacement.",
            assetName,
            filePath
        );
    }

    public bool TryGetTexture(string assetName, out Texture2D texture)
        => _textures.TryGetValue(assetName, out texture);

    public void UnloadTexture(string assetName)
    {
        if (_textures.Remove(assetName, out var texture))
        {
            texture.Dispose();
            _logger.Information("Texture {AssetName} unloaded.", assetName);
        }
        else
        {
            _logger.Warning("Texture with asset name {AssetName} not found.", assetName);
        }
    }

    private void PrepareWhiteAndBlackTextures()
    {
        var whitePixel = new byte[] { 255, 255, 255, 255 };
        var blackPixel = new byte[] { 0, 0, 0, 255 };

        DefaultWhiteTexture = new(_gl, whitePixel, 1, 1);
        DefaultBlackTexture = new(_gl, blackPixel, 1, 1);

        _logger.Debug("Generated black and white textures");
    }

    private static void ReplaceChromaKey(byte[] pixelData, byte tolerance)
    {
        // Pixel format is RGBA (4 bytes per pixel)
        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var r = pixelData[i];
            var g = pixelData[i + 1];
            var b = pixelData[i + 2];

            // Check if pixel is magenta (255, 0, 255) within tolerance
            if (r >= 255 - tolerance &&
                g <= tolerance &&
                b >= 255 - tolerance)
            {
                // Set alpha to 0 (fully transparent) and clear RGB to avoid fringe bleeding
                pixelData[i] = 0;
                pixelData[i + 1] = 0;
                pixelData[i + 2] = 0;
                pixelData[i + 3] = 0;
            }
        }
    }
}

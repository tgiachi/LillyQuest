using System.Reflection;
using LillyQuest.Core.Utils;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Extension methods for asset managers to load embedded resources.
/// Provides convenient overloads for loading assets directly from embedded resources.
/// </summary>
public static class AssetManagerExtensions
{
    /// <summary>
    /// Loads a bitmap font from an embedded resource.
    /// </summary>
    /// <param name="manager">The font manager instance</param>
    /// <param name="assetName">Unique name for the font asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Fonts/default.png")</param>
    /// <param name="tileWidth">Width of each character tile in pixels</param>
    /// <param name="tileHeight">Height of each character tile in pixels</param>
    /// <param name="spacing">Space between tiles in pixels</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadBmpFontFromEmbeddedResource(
        this IFontManager manager,
        string assetName,
        string resourcePath,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadBmpFont(assetName, data, tileWidth, tileHeight, spacing);
    }

    /// <summary>
    /// Loads a font from an embedded resource.
    /// </summary>
    /// <param name="manager">The font manager instance</param>
    /// <param name="assetName">Unique name for the font asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Fonts/arial.ttf")</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadFontFromEmbeddedResource(
        this IFontManager manager,
        string assetName,
        string resourcePath,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadFont(assetName, data);
    }

    /// <summary>
    /// Loads a music track from an embedded resource.
    /// </summary>
    /// <param name="manager">The audio manager instance</param>
    /// <param name="musicName">Unique name for the music asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Music/theme.ogg")</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadMusicFromEmbeddedResource(
        this IAudioManager manager,
        string musicName,
        string resourcePath,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadMusicFromBuffer(musicName, data);
    }

    /// <summary>
    /// Loads a shader from embedded resources.
    /// </summary>
    /// <param name="manager">The shader manager instance</param>
    /// <param name="shaderName">Cache key for the shader</param>
    /// <param name="vertexResourcePath">Path to the vertex shader embedded resource</param>
    /// <param name="fragmentResourcePath">Path to the fragment shader embedded resource</param>
    /// <param name="assembly">The assembly containing the embedded resources. If null, uses the calling assembly</param>
    public static void LoadShaderFromEmbeddedResource(
        this IShaderManager manager,
        string shaderName,
        string vertexResourcePath,
        string fragmentResourcePath,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var vertexData = ResourceUtils.GetEmbeddedResourceContent(vertexResourcePath, assembly);
        var fragmentData = ResourceUtils.GetEmbeddedResourceContent(fragmentResourcePath, assembly);
        manager.LoadShader(shaderName, vertexData, fragmentData);
    }

    /// <summary>
    /// Loads a sound effect from an embedded resource.
    /// </summary>
    /// <param name="manager">The audio manager instance</param>
    /// <param name="soundName">Unique name for the sound asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Sounds/explosion.wav")</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadSoundFromEmbeddedResource(
        this IAudioManager manager,
        string soundName,
        string resourcePath,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadSoundFromBuffer(soundName, data);
    }

    /// <summary>
    /// Loads a texture from an embedded resource.
    /// </summary>
    /// <param name="manager">The texture manager instance</param>
    /// <param name="assetName">Unique name for the texture asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Textures/player.png")</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadTextureFromEmbeddedResource(
        this ITextureManager manager,
        string assetName,
        string resourcePath,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadTextureFromPng(assetName, data);
    }

    /// <summary>
    /// Loads a texture from an embedded resource with chroma key support.
    /// </summary>
    /// <param name="manager">The texture manager instance</param>
    /// <param name="assetName">Unique name for the texture asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Textures/sprite.png")</param>
    /// <param name="tolerance">Color tolerance for magenta detection (0-255)</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadTextureFromEmbeddedResourceWithChromaKey(
        this ITextureManager manager,
        string assetName,
        string resourcePath,
        byte tolerance = 0,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadTextureFromPngWithChromaKey(assetName, data, tolerance);
    }

    /// <summary>
    /// Loads a tileset from an embedded resource.
    /// </summary>
    /// <param name="manager">The tileset manager instance</param>
    /// <param name="tilesetName">Unique name for the tileset asset</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/Tilesets/overworld.png")</param>
    /// <param name="tileWidth">Width of each tile in pixels</param>
    /// <param name="tileHeight">Height of each tile in pixels</param>
    /// <param name="spacing">Space between tiles in pixels</param>
    /// <param name="margin">Margin around tiles in pixels</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly</param>
    public static void LoadTilesetFromEmbeddedResource(
        this ITilesetManager manager,
        string tilesetName,
        string resourcePath,
        int tileWidth,
        int tileHeight,
        int spacing = 0,
        int margin = 0,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        manager.LoadTileset(tilesetName, data, tileWidth, tileHeight, spacing, margin);
    }

    /// <summary>
    /// Loads a nine-slice texture from an embedded resource and registers it.
    /// </summary>
    /// <param name="manager">The asset manager instance.</param>
    /// <param name="key">Unique key for the nine-slice definition.</param>
    /// <param name="resourcePath">Path to the embedded resource (e.g., "Assets/9patch/window.png").</param>
    /// <param name="margins">Pixel margins for the nine-slice (left, top, right, bottom).</param>
    /// <param name="assembly">The assembly containing the embedded resource. If null, uses the calling assembly.</param>
    public static void LoadNineSliceFromEmbeddedResource(
        this IAssetManager manager,
        string key,
        string resourcePath,
        Vector4D<float> margins,
        Assembly? assembly = null
    )
    {
        assembly ??= Assembly.GetCallingAssembly();
        var data = ResourceUtils.GetEmbeddedResourceContent(resourcePath, assembly);
        using var image = Image.Load<Rgba32>(data);
        var pixelData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixelData);

        var textureName = $"n9_ui_{key}";
        manager.NineSliceManager.LoadNineSlice(
            key,
            textureName,
            pixelData,
            (uint)image.Width,
            (uint)image.Height,
            new Rectangle<int>(0, 0, image.Width, image.Height),
            margins
        );
    }
}

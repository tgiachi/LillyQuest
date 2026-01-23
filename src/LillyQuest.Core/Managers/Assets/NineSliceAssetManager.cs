using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Interfaces.Assets;
using Silk.NET.Maths;

namespace LillyQuest.Core.Managers.Assets;

public sealed class NineSliceAssetManager : INineSliceAssetManager
{
    private readonly ITextureManager _textureManager;
    private readonly Dictionary<string, NineSliceDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TexturePatch> _patches = new(StringComparer.OrdinalIgnoreCase);

    public NineSliceAssetManager(ITextureManager textureManager)
        => _textureManager = textureManager;

    public NineSliceDefinition GetNineSlice(string key)
    {
        if (!_definitions.TryGetValue(key, out var definition))
        {
            throw new KeyNotFoundException($"Nine-slice not found: {key}");
        }

        return definition;
    }

    public TexturePatch GetTexturePatch(string textureName, string elementName)
    {
        if (!_patches.TryGetValue(BuildPatchKey(textureName, elementName), out var patch))
        {
            throw new KeyNotFoundException($"Texture patch not found: {textureName}:{elementName}");
        }

        return patch;
    }

    public void LoadNineSlice(
        string key,
        string textureName,
        string filePath,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    )
    {
        _textureManager.LoadTexture(textureName, filePath);
        RegisterNineSlice(key, textureName, sourceRect, margins);
    }

    public void LoadNineSlice(
        string key,
        string textureName,
        Span<byte> data,
        uint width,
        uint height,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    )
    {
        _textureManager.LoadTexture(textureName, data, width, height);
        RegisterNineSlice(key, textureName, sourceRect, margins);
    }

    public void LoadNineSliceFromPng(
        string key,
        string textureName,
        Span<byte> pngData,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    )
    {
        _textureManager.LoadTextureFromPng(textureName, pngData);
        RegisterNineSlice(key, textureName, sourceRect, margins);
    }

    public void RegisterNineSlice(
        string key,
        string textureName,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    )
    {
        var left = (int)margins.X;
        var top = (int)margins.Y;
        var right = (int)margins.Z;
        var bottom = (int)margins.W;

        var centerWidth = sourceRect.Size.X - left - right;
        var centerHeight = sourceRect.Size.Y - top - bottom;

        var x = sourceRect.Origin.X;
        var y = sourceRect.Origin.Y;

        var topLeft = new Rectangle<int>(x, y, left, top);
        var topRect = new Rectangle<int>(x + left, y, centerWidth, top);
        var topRight = new Rectangle<int>(x + left + centerWidth, y, right, top);

        var leftRect = new Rectangle<int>(x, y + top, left, centerHeight);
        var center = new Rectangle<int>(x + left, y + top, centerWidth, centerHeight);
        var rightRect = new Rectangle<int>(x + left + centerWidth, y + top, right, centerHeight);

        var bottomLeft = new Rectangle<int>(x, y + top + centerHeight, left, bottom);
        var bottomRect = new Rectangle<int>(x + left, y + top + centerHeight, centerWidth, bottom);
        var bottomRight = new Rectangle<int>(x + left + centerWidth, y + top + centerHeight, right, bottom);

        _definitions[key] = new(
            textureName,
            topLeft,
            topRect,
            topRight,
            leftRect,
            center,
            rightRect,
            bottomLeft,
            bottomRect,
            bottomRight
        );
    }

    public void RegisterTexturePatches(
        string textureName,
        IReadOnlyList<TexturePatchDefinition> patches
    )
    {
        if (patches == null || patches.Count == 0)
        {
            return;
        }

        foreach (var patch in patches)
        {
            var key = BuildPatchKey(textureName, patch.ElementName);
            _patches[key] = new(textureName, patch.ElementName, patch.Section);
        }
    }

    public bool TryGetNineSlice(string key, out NineSliceDefinition definition)
        => _definitions.TryGetValue(key, out definition);

    public bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch)
        => _patches.TryGetValue(BuildPatchKey(textureName, elementName), out patch);

    private static string BuildPatchKey(string textureName, string elementName)
        => $"{textureName}:{elementName}";
}

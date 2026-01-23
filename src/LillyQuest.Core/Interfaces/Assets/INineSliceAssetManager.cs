using LillyQuest.Core.Data.Assets;
using Silk.NET.Maths;

namespace LillyQuest.Core.Interfaces.Assets;

public interface INineSliceAssetManager
{
    NineSliceDefinition GetNineSlice(string key);

    TexturePatch GetTexturePatch(string textureName, string elementName);

    void LoadNineSlice(
        string key,
        string textureName,
        string filePath,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    );

    void LoadNineSlice(
        string key,
        string textureName,
        Span<byte> data,
        uint width,
        uint height,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    );

    void LoadNineSliceFromPng(
        string key,
        string textureName,
        Span<byte> pngData,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    );

    void RegisterNineSlice(
        string key,
        string textureName,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    );

    void RegisterTexturePatches(
        string textureName,
        IReadOnlyList<TexturePatchDefinition> patches
    );

    bool TryGetNineSlice(string key, out NineSliceDefinition definition);

    bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch);
}

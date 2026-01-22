using LillyQuest.Core.Data.Assets;
using Silk.NET.Maths;

namespace LillyQuest.Core.Interfaces.Assets;

public interface INineSliceAssetManager
{
    void RegisterNineSlice(
        string key,
        string textureName,
        Rectangle<int> sourceRect,
        Vector4D<float> margins
    );

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

    NineSliceDefinition GetNineSlice(string key);

    bool TryGetNineSlice(string key, out NineSliceDefinition definition);
}

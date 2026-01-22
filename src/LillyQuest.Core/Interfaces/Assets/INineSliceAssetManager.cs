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

    NineSliceDefinition GetNineSlice(string key);

    bool TryGetNineSlice(string key, out NineSliceDefinition definition);
}

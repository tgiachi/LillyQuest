using LillyQuest.Core.Graphics.OpenGL.Resources;

namespace LillyQuest.Core.Interfaces.Assets;

public interface ITextureManager : IDisposable
{
    Texture2D DefaultWhiteTexture { get; }
    Texture2D DefaultBlackTexture { get; }

    Texture2D GetTexture(string assetName);
    bool HasTexture(string assetName);

    void LoadTexture(string assetName, string filePath);
    void LoadTexture(string assetName, Span<byte> data, uint width, uint height);
    void LoadTextureFromPng(string assetName, Span<byte> pngData);
    bool TryGetTexture(string assetName, out Texture2D texture);

    void UnloadTexture(string assetName);
}

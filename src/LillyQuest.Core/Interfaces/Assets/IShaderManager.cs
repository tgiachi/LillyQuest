using LillyQuest.Core.Graphics.OpenGL.Resources;

namespace LillyQuest.Core.Interfaces.Assets;

/// <summary>
/// Defines shader loading and caching operations.
/// </summary>
public interface IShaderManager
{
    /// <summary>
    /// Gets a shader by name or throws if not found.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <returns>The cached shader.</returns>
    Shader GetShader(string shaderName);

    /// <summary>
    /// Checks whether a shader exists in the cache.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <returns>True if the shader exists.</returns>
    bool HasShader(string shaderName);

    /// <summary>
    /// Loads a shader from vertex and fragment files.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexPath">Path to the vertex shader file.</param>
    /// <param name="fragmentPath">Path to the fragment shader file.</param>
    void LoadShader(string shaderName, string vertexPath, string fragmentPath);

    /// <summary>
    /// Loads a shader from in-memory vertex and fragment data.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexData">Vertex shader byte data.</param>
    /// <param name="fragmentData">Fragment shader byte data.</param>
    void LoadShader(string shaderName, Span<byte> vertexData, Span<byte> fragmentData);

    /// <summary>
    /// Loads a shader from vertex and fragment streams.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexStream">Vertex shader stream.</param>
    /// <param name="fragmentStream">Fragment shader stream.</param>
    void LoadShader(string shaderName, Stream vertexStream, Stream fragmentStream);

    /// <summary>
    /// Attempts to retrieve a shader by name.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <param name="shader">The resolved shader instance.</param>
    /// <returns>True if found.</returns>
    bool TryGetShader(string shaderName, out Shader shader);

    /// <summary>
    /// Unloads a shader by name.
    /// </summary>
    /// <param name="shaderName">Name of the shader to unload.</param>
    /// <returns>True if the shader was removed.</returns>
    bool UnloadShader(string shaderName);
}

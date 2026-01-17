using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Interfaces.Assets;
using Serilog;
using Silk.NET.OpenGL;
using Shader = LillyQuest.Core.Graphics.OpenGL.Resources.Shader;

namespace LillyQuest.Core.Managers.Assets;

/// <summary>
/// Manages shader loading and caching.
/// </summary>
public class ShaderManager : IShaderManager
{
    private readonly ILogger _logger = Log.ForContext<ShaderManager>();
    private readonly GL _gl;
    private readonly Dictionary<string, Shader> _shaders = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a new shader manager for the provided OpenGL context.
    /// </summary>
    /// <param name= renderContext">Context.
    /// </param>
    public ShaderManager(EngineRenderContext renderContext)
        => _gl = renderContext.Gl;

    /// <summary>
    /// Gets a shader by name or throws if not found.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <returns>The cached shader.</returns>
    public Shader GetShader(string shaderName)
        => _shaders.TryGetValue(shaderName, out var shader)
               ? shader
               : throw new KeyNotFoundException($"Shader with name {shaderName} not found.");

    /// <summary>
    /// Checks whether a shader exists in the cache.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <returns>True if the shader exists.</returns>
    public bool HasShader(string shaderName)
        => _shaders.ContainsKey(shaderName);

    /// <summary>
    /// Loads a shader from vertex and fragment files.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexPath">Path to the vertex shader file.</param>
    /// <param name="fragmentPath">Path to the fragment shader file.</param>
    public void LoadShader(string shaderName, string vertexPath, string fragmentPath)
    {
        if (_shaders.ContainsKey(shaderName))
        {
            _logger.Warning("Shader with name {ShaderName} already loaded.", shaderName);

            return;
        }

        var shader = new Shader(shaderName, _gl, vertexPath, fragmentPath);
        _shaders[shaderName] = shader;
        _logger.Information("Shader {ShaderName} loaded from files.", shaderName);
    }

    /// <summary>
    /// Loads a shader from in-memory vertex and fragment data.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexData">Vertex shader byte data.</param>
    /// <param name="fragmentData">Fragment shader byte data.</param>
    public void LoadShader(string shaderName, Span<byte> vertexData, Span<byte> fragmentData)
    {
        if (_shaders.ContainsKey(shaderName))
        {
            _logger.Warning("Shader with name {ShaderName} already loaded.", shaderName);

            return;
        }

        using var vertexStream = new MemoryStream(vertexData.ToArray(), false);
        using var fragmentStream = new MemoryStream(fragmentData.ToArray(), false);

        LoadShader(shaderName, vertexStream, fragmentStream);
    }

    /// <summary>
    /// Loads a shader from vertex and fragment streams.
    /// </summary>
    /// <param name="shaderName">Cache key for the shader.</param>
    /// <param name="vertexStream">Vertex shader stream.</param>
    /// <param name="fragmentStream">Fragment shader stream.</param>
    public void LoadShader(string shaderName, Stream vertexStream, Stream fragmentStream)
    {
        if (_shaders.ContainsKey(shaderName))
        {
            _logger.Warning("Shader with name {ShaderName} already loaded.", shaderName);

            return;
        }

        var shader = new Shader(shaderName, _gl, vertexStream, fragmentStream);
        _shaders[shaderName] = shader;
        _logger.Information("Shader {ShaderName} loaded from streams.", shaderName);
    }

    /// <summary>
    /// Attempts to retrieve a shader by name.
    /// </summary>
    /// <param name="shaderName">Name of the shader.</param>
    /// <param name="shader">The resolved shader instance.</param>
    /// <returns>True if found.</returns>
    public bool TryGetShader(string shaderName, out Shader shader)
        => _shaders.TryGetValue(shaderName, out shader);

    /// <summary>
    /// Unloads a shader by name.
    /// </summary>
    /// <param name="shaderName">Name of the shader to unload.</param>
    /// <returns>True if the shader was removed.</returns>
    public bool UnloadShader(string shaderName)
    {
        if (_shaders.TryGetValue(shaderName, out var shader))
        {
            shader.Dispose();
            _shaders.Remove(shaderName);
            _logger.Information("Shader {ShaderName} unloaded.", shaderName);

            return true;
        }

        return false;
    }
}

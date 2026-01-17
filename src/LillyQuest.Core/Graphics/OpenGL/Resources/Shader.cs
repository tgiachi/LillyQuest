using System.Numerics;
using LillyQuest.Core.Exceptions.OpenGL;
using Silk.NET.OpenGL;

namespace LillyQuest.Core.Graphics.OpenGL.Resources;

public class Shader : IDisposable
{
    public delegate void ShaderCompileResult(string name, ShaderType shaderType, bool success, Exception? exception);

    public event ShaderCompileResult? OnCompileResult;

    public uint Handle { get; private set; }
    private readonly GL _gl;
    private readonly Dictionary<string, int> _uniformLocations = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _attribLocations = new(StringComparer.Ordinal);

    public string Name { get; }

    public Shader(string name, GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;
        Name = name;

        var vertex = LoadShaderFromFile(ShaderType.VertexShader, vertexPath);
        var fragment = LoadShaderFromFile(ShaderType.FragmentShader, fragmentPath);
        Handle = LinkProgram(vertex, fragment);
        ValidateProgram();
    }

    public Shader(string name, GL gl, Stream vertexStream, Stream fragmentStream)
    {
        _gl = gl;
        Name = name;

        var vertex = LoadShaderFromStream(ShaderType.VertexShader, vertexStream);
        var fragment = LoadShaderFromStream(ShaderType.FragmentShader, fragmentStream);
        Handle = LinkProgram(vertex, fragment);
        ValidateProgram();
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Handle);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns the location of a vertex attribute, using the internal cache.
    /// </summary>
    public int GetAttribLocation(string name)
        => GetAttribLocationCached(name);

    /// <summary>
    /// Returns the location of a uniform, using the internal cache.
    /// </summary>
    public int GetUniformLocation(string name)
        => GetUniformLocationCached(name);

    public void SetUniform(string name, int value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, bool value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform1(location, value ? 1 : 0);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        var location = GetUniformLocationCached(name);
        _gl.UniformMatrix4(location, 1, false, (float*)&value);
    }

    public void SetUniform(string name, float value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            return;
        }

        var uniformName = NormalizeArrayUniformName(name);
        var location = GetUniformLocationCached(uniformName);

        fixed (int* data = values)
        {
            _gl.Uniform1(location, (uint)values.Length, data);
        }
    }

    public unsafe void SetUniform(string name, float[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            return;
        }

        var uniformName = NormalizeArrayUniformName(name);
        var location = GetUniformLocationCached(uniformName);

        fixed (float* data = values)
        {
            _gl.Uniform1(location, (uint)values.Length, data);
        }
    }

    public void SetUniform(string name, Vector2 value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform2(location, value.X, value.Y);
    }

    public void SetUniform(string name, Vector3 value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform3(location, value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        var location = GetUniformLocationCached(name);
        _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, Matrix4x4 value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&value);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, float value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            _gl.Uniform1(location, value);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, int value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            _gl.Uniform1(location, value);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, bool value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            _gl.Uniform1(location, value ? 1 : 0);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, Vector3 value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            _gl.Uniform3(location, value.X, value.Y, value.Z);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to set a uniform without throwing if it doesn't exist.
    /// </summary>
    public bool TrySetUniform(string name, Vector4 value)
    {
        if (TryGetUniformLocation(name, out var location))
        {
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);

            return true;
        }

        return false;
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    private uint CompileShader(ShaderType type, string src, string sourceLabel)
    {
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        _gl.GetShader(handle, ShaderParameterName.CompileStatus, out var status);
        var infoLog = _gl.GetShaderInfoLog(handle);

        if (status == 0)
        {
            var exception = new ShaderCompileException(
                $"Error compiling shader of type {type} ({sourceLabel}): {infoLog}\nSource:\n{src}"
            );
            OnCompileResult?.Invoke(Name, type, false, exception);

            throw exception;
        }
        OnCompileResult?.Invoke(Name, type, true, null);

        return handle;
    }

    private int GetAttribLocationCached(string name)
    {
        if (_attribLocations.TryGetValue(name, out var location))
        {
            return location;
        }

        location = _gl.GetAttribLocation(Handle, name);

        if (location == -1)
        {
            throw new ShaderUniformNotFoundException($"{name} attribute not found on shader.");
        }

        _attribLocations[name] = location;

        return location;
    }

    private int GetUniformLocationCached(string name)
    {
        if (_uniformLocations.TryGetValue(name, out var location))
        {
            return location;
        }

        location = _gl.GetUniformLocation(Handle, name);

        if (location == -1)
        {
            throw new ShaderUniformNotFoundException($"{name} uniform not found on shader.");
        }

        _uniformLocations[name] = location;

        return location;
    }

    private uint LinkProgram(uint vertex, uint fragment)
    {
        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vertex);
        _gl.AttachShader(Handle, fragment);
        _gl.LinkProgram(Handle);
        _gl.GetProgram(Handle, GLEnum.LinkStatus, out var status);

        if (status == 0)
        {
            throw new ShaderLinkException($"Program failed to link with error: {_gl.GetProgramInfoLog(Handle)}");
        }
        _gl.DetachShader(Handle, vertex);
        _gl.DetachShader(Handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);

        return Handle;
    }

    private uint LoadShaderFromFile(ShaderType type, string path)
    {
        var src = File.ReadAllText(path);

        return CompileShader(type, src, path);
    }

    private uint LoadShaderFromStream(ShaderType type, Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var src = reader.ReadToEnd();

        return CompileShader(type, src, "stream");
    }

    private static string NormalizeArrayUniformName(string name)
        => name.Contains('[') ? name : $"{name}[0]";

    /// <summary>
    /// Tries to get a uniform location without throwing if it doesn't exist.
    /// Returns true if uniform exists, false otherwise.
    /// </summary>
    private bool TryGetUniformLocation(string name, out int location)
    {
        if (_uniformLocations.TryGetValue(name, out location))
        {
            return true;
        }

        location = _gl.GetUniformLocation(Handle, name);

        if (location == -1)
        {
            return false;
        }

        _uniformLocations[name] = location;

        return true;
    }

    private void ValidateProgram()
    {
        _gl.ValidateProgram(Handle);
        _gl.GetProgram(Handle, GLEnum.ValidateStatus, out var status);

        if (status == 0)
        {
            throw new ShaderValidationException($"Program failed to validate with error: {_gl.GetProgramInfoLog(Handle)}");
        }
    }
}

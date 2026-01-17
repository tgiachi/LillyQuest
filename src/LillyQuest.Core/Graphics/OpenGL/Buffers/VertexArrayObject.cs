using Silk.NET.OpenGL;

namespace LillyQuest.Core.Graphics.OpenGL.Buffers;

/// <summary>
/// Generic wrapper for OpenGL Vertex Array Objects (VAO) with type-safe index buffer management.
/// </summary>
/// <typeparam name="TIndexType">The type of indices (byte, ushort, or uint)</typeparam>
public class VertexArrayObject<TIndexType> : IDisposable
    where TIndexType : unmanaged
{
    private readonly uint _handle;
    private readonly int _stride;
    private readonly GL _gl;
    private readonly BufferObject<TIndexType> _indexBuffer;

    /// <summary>
    /// Gets the OpenGL DrawElementsType for this VAO's index buffer.
    /// </summary>
    public DrawElementsType IndexType { get; }

    /// <summary>
    /// Creates a new VertexArrayObject with an associated index buffer.
    /// </summary>
    /// <param name="gl">The OpenGL context</param>
    /// <param name="stride">The stride of vertex data in bytes</param>
    /// <param name="indexBuffer">The index buffer to associate with this VAO</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if stride is not positive</exception>
    /// <exception cref="NotSupportedException">Thrown if TIndexType is not byte, ushort, or uint</exception>
    public VertexArrayObject(GL gl, int stride, BufferObject<TIndexType> indexBuffer)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stride);
        ArgumentNullException.ThrowIfNull(indexBuffer);

        _gl = gl;
        _stride = stride;
        _indexBuffer = indexBuffer;
        IndexType = GetIndexType();

        _gl.GenVertexArrays(1, out _handle);
        Bind();

        // Associate the index buffer with this VAO
        _indexBuffer.Bind();
    }

    /// <summary>
    /// Binds this vertex array object to the current OpenGL context.
    /// The associated index buffer is automatically bound.
    /// </summary>
    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Configures a vertex attribute pointer for this VAO.
    /// </summary>
    /// <param name="location">The attribute location in the shader</param>
    /// <param name="size">The number of components (1-4)</param>
    /// <param name="type">The data type of the attribute</param>
    /// <param name="normalized">Whether to normalize the data</param>
    /// <param name="offset">The offset within the vertex structure in bytes</param>
    public unsafe void VertexAttribPointer(int location, int size, VertexAttribPointerType type, bool normalized, int offset)
    {
        _gl.EnableVertexAttribArray((uint)location);
        _gl.VertexAttribPointer((uint)location, size, type, normalized, (uint)_stride, (void*)offset);
    }

    /// <summary>
    /// Maps the generic TIndexType to the appropriate OpenGL DrawElementsType.
    /// </summary>
    /// <returns>The DrawElementsType for the index buffer</returns>
    /// <exception cref="NotSupportedException">Thrown if TIndexType is not a supported index type</exception>
    private static DrawElementsType GetIndexType()
    {
        var indexType = typeof(TIndexType);

        if (indexType == typeof(byte))
        {
            return DrawElementsType.UnsignedByte;
        }

        if (indexType == typeof(short) || indexType == typeof(ushort))
        {
            return DrawElementsType.UnsignedShort;
        }

        if (indexType == typeof(uint))
        {
            return DrawElementsType.UnsignedInt;
        }

        throw new NotSupportedException(
            $"Index type '{indexType.Name}' is not supported. Supported types are: byte, short, ushort, uint"
        );
    }
}

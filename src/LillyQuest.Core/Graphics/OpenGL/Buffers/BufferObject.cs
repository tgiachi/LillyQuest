using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace LillyQuest.Core.Graphics.OpenGL.Buffers;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    public uint Handle { get; }
    private readonly BufferTargetARB _bufferType;
    private readonly GL _gl;

    public int Size { get; }

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;

        Handle = _gl.GenBuffer();
        Bind();

        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public unsafe BufferObject(GL gl, int size, BufferTargetARB bufferType, bool isDynamic)
    {
        _bufferType = bufferType;
        _gl = gl;
        Size = size;

        Handle = _gl.GenBuffer();

        Bind();

        var elementSizeInBytes = Marshal.SizeOf<TDataType>();
        _gl.BufferData(
            bufferType,
            (nuint)(size * elementSizeInBytes),
            null,
            isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw
        );
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, Handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(Handle);
        GC.SuppressFinalize(this);
    }

    public unsafe void SetData(TDataType[] data, int startIndex, int elementCount)
    {
        Bind();

        fixed (TDataType* dataPtr = &data[startIndex])
        {
            var elementSizeInBytes = sizeof(TDataType);

            _gl.BufferSubData(_bufferType, 0, (nuint)(elementCount * elementSizeInBytes), dataPtr);
        }
    }
}

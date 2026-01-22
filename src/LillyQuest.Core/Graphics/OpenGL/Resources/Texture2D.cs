using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Rectangle = System.Drawing.Rectangle;

namespace LillyQuest.Core.Graphics.OpenGL.Resources;

public class Texture2D : IDisposable
{
    public uint Handle { get; }

    public int Width { get; }
    public int Height { get; }

    private readonly GL _gl;

    public unsafe Texture2D(GL gl, string path)
    {
        _gl = gl;

        Handle = _gl.GenTexture();
        Bind();

        using (var img = Image.Load<Rgba32>(path))
        {
            Width = img.Width;
            Height = img.Height;

            gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)Width,
                (uint)Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            img.ProcessPixelRows(
                accessor =>
                {
                    for (var y = 0; y < accessor.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            gl.TexSubImage2D(
                                TextureTarget.Texture2D,
                                0,
                                0,
                                y,
                                (uint)accessor.Width,
                                1,
                                PixelFormat.Rgba,
                                PixelType.UnsignedByte,
                                data
                            );
                        }
                    }
                }
            );
        }

        SetParameters();
    }

    public unsafe Texture2D(GL gl, Span<byte> data, uint width, uint height)
    {
        _gl = gl;
        Width = (int)width;
        Height = (int)height;

        Handle = _gl.GenTexture();
        Bind();

        fixed (void* d = &data[0])
        {
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                (uint)Width,
                (uint)Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                d
            );
            SetParameters();
        }
    }

    public Texture2D(GL gl, int width, int height)
        : this(gl, width, height, true) { }

    public unsafe Texture2D(GL gl, int width, int height, bool useMipmaps)
    {
        _gl = gl;
        Width = width;
        Height = height;

        Handle = _gl.GenTexture();

        Bind();

        //Reserve enough memory from the gpu for the whole image
        _gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba8,
            (uint)width,
            (uint)height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            null
        );

        SetParameters(useMipmaps);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void ConfigureSampling(bool useMipmaps, bool useLinearFiltering, bool clampToEdge)
    {
        Bind();
        ApplyParameters(useMipmaps, useLinearFiltering, clampToEdge);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(Handle);
        GC.SuppressFinalize(this);
    }

    public unsafe void SetData(Rectangle bounds, byte[] data)
    {
        Bind();

        fixed (byte* ptr = data)
        {
            _gl.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                bounds.Left,
                bounds.Top,
                (uint)bounds.Width,
                (uint)bounds.Height,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }
    }

    private void ApplyParameters(bool useMipmaps, bool useLinearFiltering, bool clampToEdge)
    {
        var wrapMode = clampToEdge ? GLEnum.ClampToEdge : GLEnum.Repeat;
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);

        var minFilter = useMipmaps ? useLinearFiltering ? GLEnum.LinearMipmapLinear : GLEnum.NearestMipmapNearest :
                        useLinearFiltering ? GLEnum.Linear : GLEnum.Nearest;
        var magFilter = useLinearFiltering ? GLEnum.Linear : GLEnum.Nearest;

        if (useMipmaps)
        {
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }
        else
        {
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
        }
    }

    private void SetParameters(bool useMipmaps = true)
    {
        ApplyParameters(useMipmaps, true, !useMipmaps);
    }
}

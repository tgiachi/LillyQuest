using Serilog;
using Silk.NET.OpenGL;

namespace LillyQuest.Core.Graphics.OpenGL.Resources;

/// <summary>
/// Framebuffer-backed render target with color texture and depth buffer.
/// </summary>
public sealed class RenderTexture : IDisposable
{
    private readonly GL _gl;
    private readonly ILogger _logger = Log.ForContext<RenderTexture>();
    private uint _framebuffer;
    private uint _depthBuffer;

    public Texture2D ColorTexture { get; }
    public int Width { get; }
    public int Height { get; }

    public RenderTexture(GL gl, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;

        _framebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        ColorTexture = new Texture2D(_gl, width, height, false);
        _gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            ColorTexture.Handle,
            0
        );

        _depthBuffer = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)width, (uint)height);
        _gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _depthBuffer
        );

        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
        {
            _logger.Error("RenderTexture framebuffer incomplete: {Status}", status);
        }

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Bind()
        => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

    public void Unbind()
        => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void Dispose()
    {
        ColorTexture.Dispose();

        if (_depthBuffer != 0)
        {
            _gl.DeleteRenderbuffer(_depthBuffer);
            _depthBuffer = 0;
        }

        if (_framebuffer != 0)
        {
            _gl.DeleteFramebuffer(_framebuffer);
            _framebuffer = 0;
        }

        GC.SuppressFinalize(this);
    }
}

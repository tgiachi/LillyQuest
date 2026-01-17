using System.Drawing;
using FontStashSharp.Interfaces;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using Silk.NET.OpenGL;

namespace LillyQuest.Core.Graphics.Text;

public class FontSharpTexture2DManager : ITexture2DManager
{
    private readonly GL _gl;

    public FontSharpTexture2DManager(GL gl)
        => _gl = gl;

    public object CreateTexture(int width, int height)
        => new Texture2D(_gl, width, height);

    public Point GetTextureSize(object texture)
    {
        var t = (Texture2D)texture;

        return new(t.Width, t.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var t = (Texture2D)texture;

        t.SetData(bounds, data);
    }
}

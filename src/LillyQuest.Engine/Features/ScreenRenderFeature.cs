using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using Silk.NET.OpenGL;

namespace LillyQuest.Engine.Features;

/// <summary>
/// Rendering feature for screens that applies scissor test for clipping.
/// RenderOrder is set to int.MinValue to ensure scissor is applied before other screen features render.
/// </summary>
public class ScreenRenderFeature : IRenderFeature
{
    private readonly Screen _screen;

    public bool IsEnabled { get; set; } = true;
    public int RenderOrder { get; set; } = int.MinValue;

    public ScreenRenderFeature(Screen screen)
    {
        _screen = screen;
    }

    public void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Skip if screen not visible
        if (!_screen.IsVisible)
            return;

        // Get GL context from RenderContext
        var gl = spriteBatch.RenderContext.Gl;

        // End current batch
        spriteBatch.End();

        // Enable scissor test with screen bounds
        gl.Enable(EnableCap.ScissorTest);
        gl.Scissor(
            (int)_screen.Position.X,
            (int)_screen.Position.Y,
            (uint)_screen.Size.X,
            (uint)_screen.Size.Y
        );

        // Begin new batch with scissor enabled
        spriteBatch.Begin();

        // Other screen features with higher RenderOrder will render inside scissor rect
    }
}

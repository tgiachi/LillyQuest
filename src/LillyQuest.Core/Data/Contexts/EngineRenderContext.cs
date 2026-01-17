using LillyQuest.Core.Primitives;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LillyQuest.Core.Data.Contexts;

public class EngineRenderContext
{
    public LyColor ClearColor { get; set; } = LyColor.CornflowerBlue;
    public IWindow Window { get; set; }
    public IInputContext InputContext { get; set; }
    public GL Gl { get; set; }
}

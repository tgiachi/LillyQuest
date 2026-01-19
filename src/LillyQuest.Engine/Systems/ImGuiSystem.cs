using ImGuiNET;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace LillyQuest.Engine.Systems;

public class ImGuiSystem : BaseSystem, IUpdateSystem, IRenderSystem
{
    private ImGuiController _imGuiController;

    public ImGuiSystem(IGameEntityManager entityManager) : base("ImGui System", 1000, entityManager) { }

    protected override void OnInitialize()
    {
        _imGuiController = new ImGuiController(RenderContext.Gl, RenderContext.Window, RenderContext.InputContext);
        base.OnInitialize();
    }

    public void Update(GameTime gameTime)
    {
    //    _imGuiController.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
    public void FixedUpdate(GameTime gameTime) { }

    public void Render(GameTime gameTime)
    {
        _imGuiController.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        ImGui.ShowDemoWindow();

        _imGuiController.Render();

    }

}

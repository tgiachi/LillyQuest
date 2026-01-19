using ImGuiNET;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
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
    }
    public void FixedUpdate(GameTime gameTime) { }

    public void Render(GameTime gameTime)
    {
        _imGuiController.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        ImGui.ShowDemoWindow();

        foreach (var feature in EntityManager.QueryOfType<IImGuiFeature>())
        {
            if (feature.IsOpened)
            {
                ImGui.Begin(feature.WindowTitle);
                feature.DrawImGui();
                ImGui.End();
            }
        }

        _imGuiController.Render();

    }

}

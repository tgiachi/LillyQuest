using ImGuiNET;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Systems.Base;
using LillyQuest.Engine.Types;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace LillyQuest.Engine.Systems;

public class ImGuiSystem : BaseSystem<IIMGuiEntity>
{
    private readonly EngineRenderContext _renderContext;
    private ImGuiController _imguiController;

    public ImGuiSystem(EngineRenderContext renderContext) : base(
        1000,
        "Immediate UI system",
        SystemQueryType.DebugRenderable
    )
    {
        _renderContext = renderContext;
    }

    public override void Initialize()
    {
        _imguiController = new ImGuiController(
            _renderContext.Gl,
            _renderContext.Window,
            _renderContext.InputContext
        );
        base.Initialize();
    }

    protected override void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<IIMGuiEntity> typedEntities
    )
    {
        _imguiController.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        foreach (var entity in typedEntities)
        {
            ImGui.Begin(entity.Name);
            entity.DrawIMGui();
        }

        _imguiController.Render();
    }
}

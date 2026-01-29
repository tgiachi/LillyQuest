using ImGuiNET;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Systems.Base;
using LillyQuest.Engine.Themes;
using LillyQuest.Engine.Types;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System for rendering ImGui UI elements.
/// Applies the Dark Fantasy theme (gold, purple, mystical colors) by default on initialization.
/// </summary>
public class ImGuiSystem : BaseSystem<IIMGuiEntity>, IDisposable
{
    private readonly EngineRenderContext _renderContext;
    private ImGuiController? _imguiController;

    public ImGuiSystem(EngineRenderContext renderContext) : base(
        130,
        "Immediate UI system",
        SystemQueryType.DebugRenderable
    )
        => _renderContext = renderContext;

    public override void Initialize()
    {
        _imguiController = new(
            _renderContext.Gl,
            _renderContext.Window,
            _renderContext.InputContext
        );

        // Apply Dark Fantasy theme (gold, purple, mystical blue)
        ImGuiThemeProvider.ApplyDarkFantasyTheme();

        base.Initialize();
    }

    protected override void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<IIMGuiEntity> typedEntities
    )
    {
        var imguiController = _imguiController ?? throw new InvalidOperationException("ImGuiSystem not initialized.");
        imguiController.Update((float)gameTime.Elapsed.TotalSeconds);

        foreach (var entity in typedEntities)
        {
            var asGameEntity = entity as GameEntity;

            if (asGameEntity is not { IsActive: true })
            {
                continue;
            }

            ImGui.Begin(entity.Name);
            entity.DrawIMGui();
            ImGui.End();
        }

        imguiController.Render();
    }

    public void Dispose()
    {
        _imguiController?.Dispose();
        _imguiController = null;
        GC.SuppressFinalize(this);
    }
}

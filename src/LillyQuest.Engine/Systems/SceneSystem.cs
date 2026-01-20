using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems;

public class SceneSystem : ISystem
{
    public uint Order => 102;
    public string Name => "Scene transition system";
    public SystemQueryType QueryType => SystemQueryType.Renderable;
    private readonly EngineRenderContext _renderContext;

    private readonly ISceneManager _sceneManager;

    public SceneSystem(ISceneManager sceneManager, EngineRenderContext renderContext)
    {
        _sceneManager = sceneManager;
        _renderContext = renderContext;
    }

    public void Initialize() { }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        _sceneManager.RenderFadeOverlay(_renderContext.Window.Size);
    }
}

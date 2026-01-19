using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using Serilog;

namespace LillyQuest.Engine.Managers;

public class SystemManager : ISystemManager
{
    private readonly ILogger _logger = Log.ForContext<SystemManager>();

    private readonly LillyQuestBootstrap _bootstrap;
    private readonly EngineRenderContext _renderContext;

    private readonly Dictionary<uint, IRenderSystem> _renderSystems = new();
    private readonly Dictionary<uint, IUpdateSystem> _updateSystems = new();

    public SystemManager(LillyQuestBootstrap bootstrap, EngineRenderContext renderContext)
    {
        _bootstrap = bootstrap;
        _renderContext = renderContext;

        _bootstrap.Update += OnUpdate;
        _bootstrap.FixedUpdate += OnFixedUpdate;
        _bootstrap.Render += OnRender;
    }

    public void AddRenderSystem(IRenderSystem renderSystem)
    {
        renderSystem.Initialize(_renderContext);
        _renderSystems.Add(renderSystem.Priority, renderSystem);
        _logger.Information(
            "Added render system {Name} with priority: {Priority}",
            renderSystem.Name,
            renderSystem.Priority
        );
    }

    public void AddSystem<T>(T system) where T : ISystem
    {
        switch (system)
        {
            case IRenderSystem renderSystem:
                AddRenderSystem(renderSystem);

                break;
            case IUpdateSystem updateSystem:
                AddUpdateSystem(updateSystem);

                break;
            default:
                _logger.Warning("System {Name} does not implement IRenderSystem or IUpdateSystem", system.Name);

                break;
        }
    }

    public void AddUpdateSystem(IUpdateSystem updateSystem)
    {
        updateSystem.Initialize(_renderContext);
        _updateSystems.Add(updateSystem.Priority, updateSystem);
        _logger.Information("Added update system {Name} priority: {Priority}", updateSystem.Name, updateSystem.Priority);
    }

    private void OnFixedUpdate(GameTime gameTime)
    {
        // Create a snapshot to avoid collection modified exception
        var updateSystemsCopy = _updateSystems.Values.ToList();

        foreach (var updateSystem in updateSystemsCopy)
        {
            updateSystem.FixedUpdate(gameTime);
        }
    }

    private void OnRender(GameTime gameTime)
    {
        // Create a snapshot to avoid collection modified exception
        var renderSystemsCopy = _renderSystems.Values.ToList();

        foreach (var renderSystem in renderSystemsCopy)
        {
            renderSystem.Render(gameTime);
        }
    }

    private void OnUpdate(GameTime gameTime)
    {
        // Create a snapshot to avoid collection modified exception
        var updateSystemsCopy = _updateSystems.Values.ToList();

        foreach (var updateSystem in updateSystemsCopy)
        {
            updateSystem.Update(gameTime);
        }
    }
}

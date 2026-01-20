using System.Diagnostics;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Extensions;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;
using Serilog;

namespace LillyQuest.Engine.Managers.Services;

public class SystemManager : ISystemManager
{
    private readonly ILogger _logger = Log.ForContext<SystemManager>();

    private readonly IGameEntityManager _gameEntityManager;

    private readonly Dictionary<SystemQueryType, SortedList<uint, ISystem>> _systemsByQueryType = new();

    private readonly LillyQuestBootstrap _lillyQuestBootstrap;

    private readonly Dictionary<SystemQueryType, TimeSpan> _systemProcessingTimes = new();

    public SystemManager(LillyQuestBootstrap lillyQuestBootstrap, IGameEntityManager gameEntityManager)
    {
        _lillyQuestBootstrap = lillyQuestBootstrap;
        _gameEntityManager = gameEntityManager;

        _lillyQuestBootstrap.Render += LillyQuestBootstrapOnRender;
        _lillyQuestBootstrap.FixedUpdate += LillyQuestBootstrapOnFixedUpdate;
        _lillyQuestBootstrap.Update += LillyQuestBootstrapOnUpdate;
    }

    public void AddSystem<TSystem>(TSystem system) where TSystem : ISystem
    {
        var queryTypes = system.QueryType.GetFlags();

        foreach (var queryType in queryTypes)
        {
            AddSystemInternal(system, queryType);
        }
    }

    public TimeSpan GetSystemProcessingTime(SystemQueryType queryType)
        => _systemProcessingTimes.TryGetValue(queryType, out var timeSpan)
               ? timeSpan
               : TimeSpan.Zero;

    public void RemoveSystem<TSystem>(TSystem system) where TSystem : ISystem
    {
        var queryTypes = system.QueryType.GetFlags();

        foreach (var queryType in queryTypes)
        {
            RemoveSystemInternal(system, queryType);
        }
    }

    private void AddSystemInternal<TSystem>(TSystem system, SystemQueryType queryType) where TSystem : ISystem
    {
        if (!_systemsByQueryType.TryGetValue(queryType, out var value))
        {
            value = new();
            _systemsByQueryType[queryType] = value;
        }

        value.Add(system.Order, system);

        _logger.Information(
            "Added system [{QueryType}] {Name} with order {Order} ",
            system.QueryType,
            system.Name,
            system.Order
        );
    }

    private void LillyQuestBootstrapOnFixedUpdate(GameTime gameTime)
    {
        ProcessLayers(gameTime, SystemQueryType.FixedUpdateable);
    }

    private void LillyQuestBootstrapOnRender(GameTime gameTime)
    {
        ProcessLayers(gameTime, SystemQueryType.Renderable);

        ProcessLayers(gameTime, SystemQueryType.DebugRenderable);
    }

    private void LillyQuestBootstrapOnUpdate(GameTime gameTime)
    {
        ProcessLayers(gameTime, SystemQueryType.Updateable);
    }

    private void ProcessLayers(GameTime gameTime, SystemQueryType queryType)
    {
        _systemProcessingTimes[queryType] = TimeSpan.Zero;

        var sw = Stopwatch.GetTimestamp();

        if (_systemsByQueryType.TryGetValue(queryType, out var systems))
        {
            foreach (var system in systems.Values)
            {
                system.ProcessEntities(gameTime, _gameEntityManager);
            }
        }

        var elapsed = Stopwatch.GetElapsedTime(sw);
        _systemProcessingTimes[queryType] = elapsed;
    }

    private void RemoveSystemInternal<TSystem>(TSystem system, SystemQueryType queryType) where TSystem : ISystem
    {
        if (_systemsByQueryType.TryGetValue(queryType, out var value))
        {
            value.Remove(system.Order);

            _logger.Information(
                "Removed system [{QueryType}] {Name} with order {Order} ",
                system.QueryType,
                system.Name,
                system.Order
            );
        }
    }
}

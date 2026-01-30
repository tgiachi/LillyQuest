using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class WorldManager : IWorldManager
{
    public event IWorldManager.OnCurrentMapChangedHandler? OnCurrentMapChanged;

    private readonly IMapGenerator _mapGenerator;
    private readonly IJobScheduler _jobScheduler;

    private readonly Dictionary<string, LyQuestMap> _maps = new Dictionary<string, LyQuestMap>();
    private readonly List<IMapHandler> _mapHandlers = new();

    private readonly ILogger _logger = Log.ForContext<WorldManager>();

    private LyQuestMap? _currentMap;

    public LyQuestMap CurrentMap
    {
        get => _currentMap!;
        set
        {
            var oldMap = _currentMap;
            field = value;
            _currentMap = value;

            if (oldMap != null)
            {
                foreach (var handler in _mapHandlers)
                {
                    handler.OnMapUnregistered(oldMap);
                }
            }

            foreach (var handler in _mapHandlers)
            {
                handler.OnMapRegistered(_currentMap);
            }

            foreach (var handler in _mapHandlers)
            {
                handler.OnCurrentMapChanged(oldMap, _currentMap);
            }

            OnCurrentMapChanged?.Invoke(oldMap, _currentMap);
        }
    }

    public LyQuestMap OverworldMap { get; set; }

    public void RegisterMapHandler(IMapHandler handler)
    {
        if (_mapHandlers.Contains(handler))
        {
            return;
        }

        _mapHandlers.Add(handler);
    }

    public void UnregisterMapHandler(IMapHandler handler)
    {
        _mapHandlers.Remove(handler);
    }

    public async Task GenerateMapAsync()
    {
        var map = await _mapGenerator.GenerateMapAsync();
        CurrentMap = map;
    }

    public WorldManager(IMapGenerator mapGenerator, IJobScheduler jobScheduler)
    {
        _mapGenerator = mapGenerator;
        _jobScheduler = jobScheduler;
    }
}

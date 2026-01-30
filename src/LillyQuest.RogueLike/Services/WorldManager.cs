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

    private readonly Dictionary<string, LyQuestMap> _maps = new();
    private readonly List<IMapHandler> _mapHandlers = new();

    private readonly ILogger _logger = Log.ForContext<WorldManager>();

    private LyQuestMap? _currentMap;

    private bool _isGeneratingMap;

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

    public WorldManager(IMapGenerator mapGenerator, IJobScheduler jobScheduler)
    {
        _mapGenerator = mapGenerator;
        _jobScheduler = jobScheduler;
    }

    public async Task GenerateMapAsync()
    {
        _jobScheduler.Enqueue(
            async () =>
            {
                if (_isGeneratingMap)
                {
                    _logger.Warning("Map generation is already in progress. Skipping new generation request.");

                    return;
                }

                _isGeneratingMap = true;

                CurrentMap = await _mapGenerator.GenerateMapAsync();

                CurrentMap.Name = "default_map";

                _maps["default_map"] = CurrentMap;

                var dungeonMap = await _mapGenerator.GenerateDungeonMapAsync(100, 100, 12);

                dungeonMap.Name = "dungeon_map";

                _maps["dungeon_map"] = dungeonMap;
                _logger.Information("Dungeon map generated and added to world manager.");

                _isGeneratingMap = false;
            }
        );
    }

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
        if (_mapHandlers.Remove(handler) && _currentMap != null)
        {
            handler.OnMapUnregistered(_currentMap);
        }
    }

    public void SwitchToMap(string mapId)
    {
        if (_maps.TryGetValue(mapId, out var map))
        {
            CurrentMap = map;
        }
        else
        {
            _logger.Error("Map with ID {MapId} not found. Cannot switch maps.", mapId);
        }
    }
}

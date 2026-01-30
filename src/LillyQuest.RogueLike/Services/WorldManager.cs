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

    private readonly ILogger _logger = Log.ForContext<WorldManager>();

    private LyQuestMap _currentMap;

    public LyQuestMap CurrentMap
    {
        get => _currentMap;
        set
        {
            field = value;
            _currentMap = value;
            OnCurrentMapChanged?.Invoke(_currentMap);
        }
    }

    public LyQuestMap OverworldMap { get; set; }

    public async Task GenerateMapAsync()
    {
        _jobScheduler.Enqueue(
            () =>
            {
                _mapGenerator.GenerateMapAsync();
            });
    }

    public WorldManager(IMapGenerator mapGenerator, IJobScheduler jobScheduler)
    {
        _mapGenerator = mapGenerator;
        _jobScheduler = jobScheduler;
    }
}

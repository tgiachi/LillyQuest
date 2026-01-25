using LillyQuest.RogueLike.Data;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Json.Entities.Base;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class DataLoaderService : IDataLoaderService
{
    private readonly ILogger _logger = Log.ForContext<DataLoaderService>();
    private readonly Dictionary<Type, List<BaseJsonEntity>> _loadedEntities = new();

    private readonly DataLoaderConfig _config;

    public DataLoaderService(DataLoaderConfig config)
    {
        _config = config;
    }

    public Task LoadDataAsync()
    {
        var dataFiles = _config.PluginDirectory.SearchFiles("Data", "json");
        return Task.CompletedTask;
    }
}

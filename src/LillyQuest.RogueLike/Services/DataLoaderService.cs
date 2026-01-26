using Humanizer;
using LillyQuest.Core.Json;
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

    public async Task LoadDataAsync()
    {
        var dataFiles = _config.PluginDirectory.SearchFiles("data", ".json");

        foreach (var dataFile in dataFiles)
        {
            _logger.Information("Loading data file: {DataFile} size: {Size}", dataFile.Name, dataFile.SizeBytes.Bytes());
            await LoadDataFile(dataFile.Path);
        }
    }

    public Task ReloadDataAsync()
    {
        _loadedEntities.Clear();

        return LoadDataAsync();
    }

    public List<TBaseJsonEntity> GetEntities<TBaseJsonEntity>() where TBaseJsonEntity : BaseJsonEntity
        => _loadedEntities.ContainsKey(typeof(TBaseJsonEntity))
               ? _loadedEntities[typeof(TBaseJsonEntity)].Cast<TBaseJsonEntity>().ToList()
               : [];

    private async Task LoadDataFile(string dataFileName)
    {
        var jsonContent = new List<BaseJsonEntity>();

        try
        {
            jsonContent = await JsonUtils.DeserializeFromFileAsync<List<BaseJsonEntity>>(dataFileName);
        }
        catch
        {
            var singleEntity = await JsonUtils.DeserializeFromFileAsync<BaseJsonEntity>(dataFileName);
            jsonContent.Add(singleEntity);
        }

        foreach (var entity in jsonContent)
        {
            if (!_loadedEntities.ContainsKey(entity.GetType()))
            {
                _loadedEntities[entity.GetType()] = [];
            }
            _loadedEntities[entity.GetType()].Add(entity);
        }
    }
}

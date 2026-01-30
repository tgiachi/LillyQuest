using Humanizer;
using LillyQuest.Core.Json;
using LillyQuest.RogueLike.Data.Configs;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Json.Entities.Base;
using Serilog;

namespace LillyQuest.RogueLike.Services.Loader;

public class DataLoaderService : IDataLoaderService
{
    private readonly ILogger _logger = Log.ForContext<DataLoaderService>();
    private readonly Dictionary<Type, List<BaseJsonEntity>> _loadedEntities = new();

    private readonly Dictionary<Type, List<IDataLoaderReceiver>> _loadedReceivers = new();

    private readonly DataLoaderConfig _config;

    public DataLoaderService(DataLoaderConfig config)
        => _config = config;

    public async Task DispatchDataToReceiversAsync()
    {
        foreach (var receiverEntry in _loadedReceivers)
        {
            var type = receiverEntry.Key;
            var receivers = receiverEntry.Value;

            if (_loadedEntities.TryGetValue(type, out var entities))
            {
                foreach (var receiver in receivers)
                {
                    _logger.Information(
                        "Dispatching {EntityCount} entities of type {EntityType} to receiver {Receiver}",
                        entities.Count,
                        type.Name,
                        receiver.GetType().Name
                    );

                    await receiver.LoadDataAsync(entities);
                }
            }
            else
            {
                _logger.Warning("No entities of type {EntityType} found to dispatch to receivers", type.Name);
            }
        }
    }

    public List<TBaseJsonEntity> GetEntities<TBaseJsonEntity>() where TBaseJsonEntity : BaseJsonEntity
        => _loadedEntities.ContainsKey(typeof(TBaseJsonEntity))
               ? _loadedEntities[typeof(TBaseJsonEntity)].Cast<TBaseJsonEntity>().ToList()
               : [];

    public async Task LoadDataAsync()
    {
        var dataFiles = _config.PluginDirectory.SearchFiles("data", ".json");

        foreach (var dataFile in dataFiles)
        {
            _logger.Information("Loading data file: {DataFile} size: {Size}", dataFile.Name, dataFile.SizeBytes.Bytes());
            await LoadDataFile(dataFile.Path);
        }
    }

    public void RegisterDataReceiver(IDataLoaderReceiver receiver)
    {
        foreach (var type in receiver.GetLoadTypes())
        {
            if (!_loadedReceivers.TryGetValue(type, out var value))
            {
                value = [];
                _loadedReceivers[type] = value;
            }

            _logger.Information("Registering data receiver: {Receiver}", receiver.GetType().Name);
            value.Add(receiver);
        }
    }

    public Task ReloadDataAsync()
    {
        _loadedEntities.Clear();

        return LoadDataAsync();
    }

    public async Task VerifyLoadedDataAsync()
    {
        foreach (var receiverEntry in _loadedReceivers)
        {
            var receivers = receiverEntry.Value;

            foreach (var receiver in receivers)
            {
                _logger.Information("Verifying loaded data for receiver: {Receiver}", receiver.GetType().Name);
                receiver.VerifyLoadedData();
            }
        }
    }

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

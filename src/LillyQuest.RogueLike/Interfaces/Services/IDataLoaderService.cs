using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IDataLoaderService
{
    Task DispatchDataToReceiversAsync();

    List<TBaseJsonEntity> GetEntities<TBaseJsonEntity>() where TBaseJsonEntity : BaseJsonEntity;
    Task LoadDataAsync();

    void RegisterDataReceiver(IDataLoaderReceiver receiver);

    Task ReloadDataAsync();

    Task VerifyLoadedDataAsync();
}

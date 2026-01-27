using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IDataLoaderService
{
    Task LoadDataAsync();

    Task ReloadDataAsync();

    Task DispatchDataToReceiversAsync();

    List<TBaseJsonEntity> GetEntities<TBaseJsonEntity>() where TBaseJsonEntity : BaseJsonEntity;

    void RegisterDataReceiver(IDataLoaderReceiver receiver);


    Task VerifyLoadedDataAsync();


}

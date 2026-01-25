using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IDataLoaderService
{
    Task LoadDataAsync();

    Task ReloadDataAsync();

    List<TBaseJsonEntity> GetEntities<TBaseJsonEntity>() where TBaseJsonEntity : BaseJsonEntity;
}

using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Interfaces;

public interface IDataLoaderReceiver
{
    Type[] GetLoadTypes();

    Task LoadDataAsync(List<BaseJsonEntity> entities);

    void ClearData();

    bool VerifyLoadedData();
}

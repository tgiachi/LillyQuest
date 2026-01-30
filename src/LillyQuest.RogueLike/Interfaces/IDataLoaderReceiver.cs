using LillyQuest.RogueLike.Json.Entities.Base;

namespace LillyQuest.RogueLike.Interfaces;

public interface IDataLoaderReceiver
{
    void ClearData();
    Type[] GetLoadTypes();

    Task LoadDataAsync(List<BaseJsonEntity> entities);

    bool VerifyLoadedData();
}

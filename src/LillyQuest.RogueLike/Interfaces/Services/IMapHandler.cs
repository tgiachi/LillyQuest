using LillyQuest.RogueLike.Maps;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IMapHandler
{
    void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap);
    void OnMapRegistered(LyQuestMap map);
    void OnMapUnregistered(LyQuestMap map);
}

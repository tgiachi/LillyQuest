using LillyQuest.RogueLike.Maps;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IMapHandler
{
    void OnMapRegistered(LyQuestMap map);
    void OnMapUnregistered(LyQuestMap map);
    void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap);
}

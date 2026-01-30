using LillyQuest.RogueLike.Maps;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IWorldManager
{
    delegate void OnCurrentMapChangedHandler(LyQuestMap? oldMap, LyQuestMap newMap);

    event OnCurrentMapChangedHandler OnCurrentMapChanged;

    LyQuestMap CurrentMap { get; set; }

    LyQuestMap OverworldMap { get; set; }

    void RegisterMapHandler(IMapHandler handler);

    void UnregisterMapHandler(IMapHandler handler);

    Task GenerateMapAsync();

}

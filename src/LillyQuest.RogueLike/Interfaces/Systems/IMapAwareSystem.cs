using LillyQuest.RogueLike.Maps;

namespace LillyQuest.RogueLike.Interfaces.Systems;

/// <summary>
/// Interface for systems that track registered maps and need cleanup when maps are unloaded.
/// </summary>
public interface IMapAwareSystem
{
    /// <summary>
    /// Unregisters a map from this system, releasing any associated resources.
    /// </summary>
    /// <param name="map">The map to unregister.</param>
    void UnregisterMap(LyQuestMap map);
}

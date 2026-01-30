using LillyQuest.RogueLike.Maps;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IMapGenerator
{
    Task<LyQuestMap> GenerateMapAsync();

    Task<LyQuestMap> GenerateDungeonMapAsync(int width, int height, int roomCount);
}

using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IMapGenerator
{
    Task<LyQuestMap> GenerateMapAsync();

    Task<LyQuestMap> GenerateDungeonMapAsync(int width, int height, int roomCount);

    Task<LyQuestMap> GenerateMapAsync(int width, int height, MapGeneratorType type);
}

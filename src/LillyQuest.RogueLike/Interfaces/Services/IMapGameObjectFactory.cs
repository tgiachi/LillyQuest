using LillyQuest.RogueLike.GameObjects;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface IMapGameObjectFactory
{
    CreatureGameObject CreateCreature(string creatureIdOrTag, int x, int y);

    TerrainGameObject CreateTerrain(string terrainIdOrTag, int x, int y);
}

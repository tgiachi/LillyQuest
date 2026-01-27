using GoRogue.GameFramework;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects;

public class TerrainGameObject : GameObject
{
    public TerrainGameObject(Point position,  bool isWalkable = true, bool isTransparent = true) : base(position, (int)MapLayer.Terrain, isWalkable, isTransparent) { }
}

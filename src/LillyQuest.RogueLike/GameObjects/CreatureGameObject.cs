using LillyQuest.RogueLike.GameObjects.Base;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects;

public class CreatureGameObject : BaseGameObject
{
    public CreatureGameObject(Point position, bool isWalkable = false, bool isTransparent = true) : base(
        position,
        (int)MapLayer.Creatures,
        isWalkable,
        isTransparent
    )
    {
    }
}

using LillyQuest.RogueLike.GameObjects.Base;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects;

public class ItemGameObject : BaseGameObject
{
    public ItemGameObject(Point position, bool isWalkable = true, bool isTransparent = false) : base(
        position,
        (int)MapLayer.Items,
        isWalkable,
        isTransparent
    ) { }
}

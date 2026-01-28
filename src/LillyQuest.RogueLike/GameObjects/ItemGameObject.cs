using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.GameObjects.Base;
using LillyQuest.RogueLike.Interfaces.GameObjects;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects;

public class ItemGameObject : BaseGameObject, IViewportUpdateable
{
    private double accomulatedTime;
    public ItemGameObject(Point position, bool isWalkable = true, bool isTransparent = false) : base(
        position,
        (int)MapLayer.Items,
        isWalkable,
        isTransparent
    )
    {
    }

    public void Update(GameTime gameTime)
    {
        accomulatedTime += gameTime.Elapsed.TotalSeconds;
        if (accomulatedTime >= 1.0)
        {
            accomulatedTime = 0;

            Tile.Symbol = Tile.Symbol == "*" ? "o" : "*";
        }

    }
}

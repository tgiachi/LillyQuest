using GoRogue.Components;
using GoRogue.GameFramework;
using LillyQuest.RogueLike.Maps.Tiles;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects.Base;

public class BaseGameObject : GameObject
{
    public VisualTile Tile { get; set; }


    public BaseGameObject(
        Point position,
        int layer,
        bool isWalkable = true,
        bool isTransparent = true,
        Func<uint>? idGenerator = null,
        IComponentCollection? customComponentCollection = null
    ) : base(position, layer, isWalkable, isTransparent, idGenerator, customComponentCollection)

    {

    }

    public BaseGameObject(
        int layer,
        bool isWalkable = true,
        bool isTransparent = true,
        Func<uint>? idGenerator = null,
        IComponentCollection? customComponentCollection = null
    ) : base(layer, isWalkable, isTransparent, idGenerator, customComponentCollection)
    {

    }

}

using GoRogue.Components;
using GoRogue.GameFramework;
using LillyQuest.RogueLike.Maps.Tiles;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects.Base;

public class BaseGameObject : GameObject
{
    private VisualTile _tile = null!;

    public event Action<BaseGameObject>? VisualTileChanged;

    public VisualTile Tile
    {
        get => _tile;
        set
        {
            if (ReferenceEquals(_tile, value))
            {
                return;
            }

            _tile = value;
            VisualTileChanged?.Invoke(this);
        }
    }


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

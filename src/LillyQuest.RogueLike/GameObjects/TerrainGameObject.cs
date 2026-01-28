using LillyQuest.RogueLike.GameObjects.Base;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.GameObjects;

public class TerrainGameObject : BaseGameObject
{
    public TerrainGameObject(Point position, bool isWalkable = true, bool isTransparent = true) : base(
        position,
        (int)MapLayer.Terrain,
        isWalkable,
        isTransparent
    ) { }

    public override string ToString()
        => $"TerrainGameObject: position: {Position}, TileId: {Tile.Id} isWalkable: {IsWalkable}, isTransparent: {IsTransparent}";
}

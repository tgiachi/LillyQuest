using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Screens.TilesetSurface;

public sealed class TileChunk
{
    public const int Size = 32;
    public TileRenderData[] Tiles { get; }

    public bool IsEmpty { get; private set; } = true;

    public TileChunk()
    {
        Tiles = new TileRenderData[Size * Size];

        for (var i = 0; i < Tiles.Length; i++)
        {
            Tiles[i] = new(-1, LyColor.White);
        }
    }

    public TileRenderData GetTile(int x, int y)
        => Tiles[x + y * Size];

    public void SetTile(int x, int y, TileRenderData tileData)
    {
        Tiles[x + y * Size] = tileData;

        if (tileData.TileIndex >= 0)
        {
            IsEmpty = false;
        }
    }
}

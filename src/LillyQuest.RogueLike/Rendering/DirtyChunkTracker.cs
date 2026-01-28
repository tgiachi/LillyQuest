namespace LillyQuest.RogueLike.Rendering;

public readonly record struct ChunkCoord(int X, int Y);

public class DirtyChunkTracker
{
    private readonly int _chunkSize;

    public DirtyChunkTracker(int chunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        _chunkSize = chunkSize;
    }

    public HashSet<ChunkCoord> DirtyChunks { get; } = new();

    public ChunkCoord GetChunkCoord(int x, int y)
        => new(x / _chunkSize, y / _chunkSize);

    public void MarkDirtyForTile(int x, int y)
        => DirtyChunks.Add(GetChunkCoord(x, y));
}

using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.Game.Systems;

public sealed class MapRenderSystem : GameEntity, IUpdateableEntity
{
    private readonly int _chunkSize;
    private readonly Dictionary<LyQuestMap, MapRenderState> _states = new();

    public MapRenderSystem(int chunkSize)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        _chunkSize = chunkSize;
        Name = nameof(MapRenderSystem);
    }

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen surface, IFOVService? fovService)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        var state = new MapRenderState(map, surface, fovService, new DirtyChunkTracker(_chunkSize));
        _states[map] = state;

        map.ObjectMoved += (_, args) => HandleObjectMoved(map, args.OldPosition, args.NewPosition);
    }

    public void UnregisterMap(LyQuestMap map)
        => _states.Remove(map);

    public bool HasMap(LyQuestMap map)
        => _states.ContainsKey(map);

    public IReadOnlyCollection<ChunkCoord> GetDirtyChunks(LyQuestMap map)
        => _states.TryGetValue(map, out var state)
            ? state.DirtyTracker.DirtyChunks
            : Array.Empty<ChunkCoord>();

    public void MarkDirtyForTile(LyQuestMap map, int x, int y)
    {
        if (_states.TryGetValue(map, out var state))
        {
            state.DirtyTracker.MarkDirtyForTile(x, y);
        }
    }

    public void HandleObjectMoved(LyQuestMap map, Point oldPosition, Point newPosition)
    {
        if (_states.TryGetValue(map, out var state))
        {
            state.DirtyTracker.MarkDirtyForTile(oldPosition.X, oldPosition.Y);
            state.DirtyTracker.MarkDirtyForTile(newPosition.X, newPosition.Y);
        }
    }

    public void Update(GameTime gameTime)
    {
        // No-op for now; implemented in later task.
    }

    private sealed record MapRenderState(
        LyQuestMap Map,
        TilesetSurfaceScreen Surface,
        IFOVService? FovService,
        DirtyChunkTracker DirtyTracker
    );
}

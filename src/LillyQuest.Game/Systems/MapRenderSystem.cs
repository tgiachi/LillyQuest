using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;

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

        _states[map] = new MapRenderState(map, surface, fovService, new DirtyChunkTracker(_chunkSize));
    }

    public void UnregisterMap(LyQuestMap map)
        => _states.Remove(map);

    public bool HasMap(LyQuestMap map)
        => _states.ContainsKey(map);

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

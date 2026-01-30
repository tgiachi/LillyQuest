using GoRogue.GameFramework;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.RogueLike.Systems;

public sealed class ViewportUpdateSystem : GameEntity, IUpdateableEntity, IMapAwareSystem, IMapHandler
{
    private readonly int _layerIndex;
    private readonly Dictionary<LyQuestMap, ViewportUpdateState> _states = new();
    private TilesetSurfaceScreen? _screen;
    private MapRenderSystem? _renderSystem;

    public ViewportUpdateSystem(int layerIndex)
    {
        _layerIndex = layerIndex;
        Name = nameof(ViewportUpdateSystem);
    }

    private sealed class ViewportUpdateState
    {
        public LyQuestMap Map { get; }
        public TilesetSurfaceScreen Screen { get; }
        public MapRenderSystem RenderSystem { get; }
        public TileViewportBounds? LastBounds { get; set; }

        public ViewportUpdateState(LyQuestMap map, TilesetSurfaceScreen screen, MapRenderSystem renderSystem)
        {
            Map = map;
            Screen = screen;
            RenderSystem = renderSystem;
        }
    }

    public void Configure(TilesetSurfaceScreen screen, MapRenderSystem renderSystem)
    {
        _screen = screen;
        _renderSystem = renderSystem;
    }

    public static TileViewportBounds GetViewportBounds(TilesetSurfaceScreen screen, int layerIndex)
    {
        var offset = screen.GetLayerViewTileOffset(layerIndex);
        var minX = (int)MathF.Floor(offset.X);
        var minY = (int)MathF.Floor(offset.Y);
        var maxX = minX + (int)screen.TileViewSize.X - 1;
        var maxY = minY + (int)screen.TileViewSize.Y - 1;

        return new(minX, minY, maxX, maxY);
    }

    public void OnCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap)
    {
        if (oldMap != null)
        {
            UnregisterMap(oldMap);
        }

        OnMapRegistered(newMap);
    }

    public void OnMapRegistered(LyQuestMap map)
    {
        if (_screen == null || _renderSystem == null)
        {
            throw new InvalidOperationException("ViewportUpdateSystem.Configure must be called before registering a map.");
        }

        RegisterMap(map, _screen, _renderSystem);
    }

    public void OnMapUnregistered(LyQuestMap map)
        => UnregisterMap(map);

    public void RegisterMap(LyQuestMap map, TilesetSurfaceScreen screen, MapRenderSystem renderSystem)
    {
        if (_states.ContainsKey(map))
        {
            return;
        }

        _states[map] = new(map, screen, renderSystem);
    }

    public void UnregisterMap(LyQuestMap map)
    {
        _states.Remove(map);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var state in _states.Values)
        {
            var bounds = GetViewportBounds(state.Screen, _layerIndex);

            // Early exit if viewport hasn't changed
            if (state.LastBounds == bounds)
            {
                continue;
            }

            state.LastBounds = bounds;

            var minX = Math.Max(0, bounds.MinX);
            var minY = Math.Max(0, bounds.MinY);
            var maxX = Math.Min(state.Map.Width - 1, bounds.MaxX);
            var maxY = Math.Min(state.Map.Height - 1, bounds.MaxY);

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var position = new Point(x, y);

                    foreach (var obj in state.Map.GetObjectsAt(position))
                    {
                        if (obj is IGameObject gameObject)
                        {
                            var animationComponent = gameObject.GoRogueComponents.GetFirstOrDefault<AnimationComponent>();

                            if (animationComponent != null && animationComponent.Update(gameTime))
                            {
                                state.RenderSystem.MarkDirtyForTile(state.Map, x, y);
                            }
                        }
                    }
                }
            }
        }
    }
}

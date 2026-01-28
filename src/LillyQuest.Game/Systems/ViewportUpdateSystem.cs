using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Game.Rendering;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.GameObjects;
using LillyQuest.RogueLike.Maps;
using SadRogue.Primitives;

namespace LillyQuest.Game.Systems;

public readonly record struct TileViewportBounds(int MinX, int MinY, int MaxX, int MaxY);

public sealed class ViewportUpdateSystem : GameEntity, IUpdateableEntity
{
    private readonly int _layerIndex;
    private readonly Dictionary<LyQuestMap, ViewportUpdateState> _states = new();

    public ViewportUpdateSystem(int layerIndex)
    {
        _layerIndex = layerIndex;
        Name = nameof(ViewportUpdateSystem);
    }

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

    public static TileViewportBounds GetViewportBounds(TilesetSurfaceScreen screen, int layerIndex)
    {
        var offset = screen.GetLayerViewTileOffset(layerIndex);
        var minX = (int)MathF.Floor(offset.X);
        var minY = (int)MathF.Floor(offset.Y);
        var maxX = minX + (int)screen.TileViewSize.X - 1;
        var maxY = minY + (int)screen.TileViewSize.Y - 1;

        return new(minX, minY, maxX, maxY);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var state in _states.Values)
        {
            var bounds = GetViewportBounds(state.Screen, _layerIndex);
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
                        if (obj is IViewportUpdateable updateable)
                        {
                            updateable.Update(gameTime);
                            state.RenderSystem.MarkDirtyForTile(state.Map, x, y);
                        }
                    }
                }
            }
        }
    }

    private sealed record ViewportUpdateState(
        LyQuestMap Map,
        TilesetSurfaceScreen Screen,
        MapRenderSystem RenderSystem
    );
}

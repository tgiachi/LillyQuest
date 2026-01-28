using System;
using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Screens.UI;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace LillyQuest.Game.Scenes;

public class RogueScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly IMapGenerator _mapGenerator;
    private readonly ITilesetManager _tilesetManager;

    private readonly IShortcutService _shortcutService;

    private readonly IActionService _actionService;
    private readonly IFOVService _fovService;

    private LyQuestMap _map = null!;
    private UIRootScreen? _uiRoot;
    private TilesetSurfaceScreen? _screen;
    private CreatureGameObject? _player;
    private MapRenderSystem? _mapRenderSystem;
    private LightOverlaySystem? _lightOverlaySystem;
    private ViewportUpdateSystem? _viewportUpdateSystem;
    private readonly List<IMapAwareSystem> _mapAwareSystems = new();

    public RogueScene(
        IScreenManager screenManager,
        IMapGenerator mapGenerator,
        ITilesetManager tilesetManager,
        IShortcutService shortcutService,
        IActionService actionService
    ) : base("RogueScene")
    {
        _screenManager = screenManager;
        _mapGenerator = mapGenerator;
        _tilesetManager = tilesetManager;
        _shortcutService = shortcutService;
        _actionService = actionService;
        _fovService = new FOVService();
    }

    private void MarkFovDirty(IReadOnlyCollection<Point> previousVisibleTiles)
    {
        if (_mapRenderSystem == null)
        {
            return;
        }

        foreach (var position in previousVisibleTiles)
        {
            _mapRenderSystem.MarkDirtyForTile(_map, position.X, position.Y);
        }

        foreach (var position in _fovService.CurrentVisibleTiles)
        {
            _mapRenderSystem.MarkDirtyForTile(_map, position.X, position.Y);
        }

        MarkLightDirty();
    }

    private void MarkLightDirty()
    {
        if (_lightOverlaySystem == null)
        {
            return;
        }

        foreach (var layer in _map.Entities.Layers)
        {
            foreach (var entity in layer.Items)
            {
                if (entity is not ItemGameObject item)
                {
                    continue;
                }

                var light = item.GoRogueComponents.GetFirstOrDefault<LightSourceComponent>();
                if (light == null)
                {
                    continue;
                }

                _lightOverlaySystem.MarkDirtyForRadius(_map, item.Position, light.Radius);
            }
        }
    }

    public override void OnLoad()
    {
        _uiRoot = new UIRootScreen()
        {
            Position = Vector2.Zero,
            Size = new Vector2(800, 600)
        };

        _screen = new TilesetSurfaceScreen(_tilesetManager)
        {
            DefaultTilesetName = "alloy",
            LayerCount = Enum.GetNames<MapLayer>().Length,
            Position = new Vector2(100, 30),
            TileRenderScale = 1f,
            TileViewSize = new Vector2(80, 30)
        };

        var windowSize = new Vector2(1280, 720);

        var availableSize = new Vector2(
            MathF.Max(0f, windowSize.X - _screen.Position.X),
            MathF.Max(0f, windowSize.Y - _screen.Position.Y)
        );

        _screen.InitializeLayers(_screen.LayerCount);

        _screen.SetLayerViewLock(0, 1);
        _screen.SetLayerViewLock(0, 2);
        _screen.SetLayerViewLock(0, 3);
        _screen.SetLayerViewLock(0, 4);

        _screen.ApplyTileViewScaleToScreen(availableSize, includeMargins: true);

        // _screen.SetLayerTileset(0, "alloy");
        // _screen.SetLayerTileset(1, "alloy");

        _screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(1, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(2, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(3, true, 0.1f);

        _shortcutService.RegisterShortcut("up", InputContextType.Global, "w", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("down", InputContextType.Global, "s", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("left", InputContextType.Global, "a", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("right", InputContextType.Global, "d", ShortcutTriggerType.Release);

        _actionService.RegisterAction(
            "up",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();
                var previousVisible = _fovService.CurrentVisibleTiles.ToArray();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Up))
                {
                    player.Item.Position += Direction.Up;
                    _fovService.UpdateFOV(player.Position);
                    MarkFovDirty(previousVisible);
                }
            }
        );
        _actionService.RegisterAction(
            "down",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();
                var previousVisible = _fovService.CurrentVisibleTiles.ToArray();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Down))
                {
                    player.Item.Position += Direction.Down;
                    _fovService.UpdateFOV(player.Position);
                    MarkFovDirty(previousVisible);
                }
            }
        );
        _actionService.RegisterAction(
            "left",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();
                var previousVisible = _fovService.CurrentVisibleTiles.ToArray();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Left))
                {
                    player.Item.Position += Direction.Left;
                    _fovService.UpdateFOV(player.Position);
                    MarkFovDirty(previousVisible);
                }
            }
        );
        _actionService.RegisterAction(
            "right",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();
                var previousVisible = _fovService.CurrentVisibleTiles.ToArray();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Right))
                {
                    player.Item.Position += Direction.Right;
                    _fovService.UpdateFOV(player.Position);
                    MarkFovDirty(previousVisible);
                }
            }
        );

        _screen.TileMouseDown += (index, x, y, buttons) =>
                                 {
                                     _screen.CenterViewOnTile(0, x, y);

                                     // _screen.CenterViewOnTile(1, x, y);
                                     //
                                     // _screen.CenterViewOnTile(2, x, y);
                                 };

        _map = _mapGenerator.GenerateMapAsync().GetAwaiter().GetResult();
        _fovService.Initialize(_map);
        _player = _map.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        if (_player != null)
        {
            _fovService.UpdateFOV(_player.Position);
        }

        _mapRenderSystem = new MapRenderSystem(chunkSize: 16);
        _mapRenderSystem.RegisterMap(_map, _screen, _fovService);
        AddEntity(_mapRenderSystem);
        _mapAwareSystems.Add(_mapRenderSystem);
        MarkFovDirty(Array.Empty<Point>());

        _lightOverlaySystem = new LightOverlaySystem(chunkSize: 16);
        _lightOverlaySystem.RegisterMap(_map, _screen, _fovService);
        AddEntity(_lightOverlaySystem);
        _mapAwareSystems.Add(_lightOverlaySystem);
        MarkLightDirty();

        _viewportUpdateSystem = new ViewportUpdateSystem(layerIndex: 0);
        _viewportUpdateSystem.RegisterMap(_map, _screen, _mapRenderSystem);
        AddEntity(_viewportUpdateSystem);
        _mapAwareSystems.Add(_viewportUpdateSystem);

        _map.ObjectMoved += (sender, args) =>
                            {
                                if (args.Item is CreatureGameObject creature)
                                {
                                    _screen.EnqueueMove(
                                        creature.Layer,
                                        new Vector2(args.OldPosition.X, args.OldPosition.Y),
                                        new Vector2(args.NewPosition.X, args.NewPosition.Y),
                                        0.1f
                                    );

                                    _screen.CenterViewOnTile(0, args.NewPosition.X, args.NewPosition.Y);
                                }
                            };

        _screen.SetLayerViewSmoothing(0, true);

        _screenManager.PushScreen(_screen);

        _screenManager.PushScreen(_uiRoot);
        base.OnLoad();
    }

    public override void OnUnload()
    {
        // Unregister map from all systems to prevent memory leaks
        if (_map != null)
        {
            foreach (var system in _mapAwareSystems)
            {
                system.UnregisterMap(_map);
            }
        }
        _mapAwareSystems.Clear();

        if (_screen != null)
        {
            _screenManager.PopScreen(_screen);
            _screen = null;
        }

        if (_uiRoot != null)
        {
            _screenManager.PopScreen(_uiRoot);
            _uiRoot = null;
        }

        base.OnUnload();
    }
}

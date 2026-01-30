using System.Numerics;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Particles;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Screens.UI;
using LillyQuest.Engine.Systems;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Game.Scenes;

public class RogueScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly ITilesetManager _tilesetManager;

    private readonly IWorldManager _worldManager;

    private readonly IShortcutService _shortcutService;
    private readonly IActionService _actionService;
    private readonly IParticleCollisionProvider _particleCollisionProvider;
    private readonly IParticleFOVProvider _particleFOVProvider;
    private readonly IParticlePixelRenderer _particlePixelRenderer;

    private FovSystem? _fovSystem;

    private UIRootScreen? _uiRoot;
    private TilesetSurfaceScreen? _screen;
    private CreatureGameObject? _player;
    private MapRenderSystem? _mapRenderSystem;
    private LightOverlaySystem? _lightOverlaySystem;
    private ViewportUpdateSystem? _viewportUpdateSystem;
    private readonly ParticleSystem _particleSystem;

    private readonly ISystemManager _systemManager;

    public RogueScene(
        IScreenManager screenManager,
        ITilesetManager tilesetManager,
        IShortcutService shortcutService,
        IActionService actionService,
        ParticleSystem particleSystem,
        IParticleCollisionProvider particleCollisionProvider,
        IParticleFOVProvider particleFOVProvider,
        IParticlePixelRenderer particlePixelRenderer,
        ISystemManager systemManager,
        IWorldManager worldManager
    ) : base("RogueScene")
    {
        _screenManager = screenManager;
        _tilesetManager = tilesetManager;
        _shortcutService = shortcutService;
        _actionService = actionService;
        _particleSystem = particleSystem;
        _particleCollisionProvider = particleCollisionProvider;
        _particleFOVProvider = particleFOVProvider;
        _particlePixelRenderer = particlePixelRenderer;
        _systemManager = systemManager;
        _worldManager = worldManager;

        if (_systemManager != null && _particleSystem != null)
        {
            _systemManager.RegisterSystem(_particleSystem);
        }
    }

    public override void OnLoad()
    {
        _uiRoot = new()
        {
            Position = Vector2.Zero,
            Size = new(800, 600)
        };

        _screen = new(_tilesetManager)
        {
            DefaultTilesetName = "alloy",
            LayerCount = Enum.GetNames<MapLayer>().Length,
            Position = new(100, 30),
            TileRenderScale = 1f,
            TileViewSize = new(80, 30)
        };

        var windowSize = new Vector2(1280, 720);

        var availableSize = new Vector2(
            MathF.Max(0f, windowSize.X - _screen.Position.X),
            MathF.Max(0f, windowSize.Y - _screen.Position.Y)
        );

        _screen.InitializeLayers(_screen.LayerCount);

        _screen.ParticlePixelRenderer = _particlePixelRenderer;
        _screen.ParticleLayerIndex = (int)MapLayer.Effects;

        _screen.SetLayerViewLock(0, 1);
        _screen.SetLayerViewLock(0, 2);
        _screen.SetLayerViewLock(0, 3);
        _screen.SetLayerViewLock(0, 4);

        _screen.ApplyTileViewScaleToScreen(availableSize);

        // _screen.SetLayerTileset(0, "alloy");
        // _screen.SetLayerTileset(1, "alloy");

        _screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(1, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(2, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(3, true, 0.1f);

        _shortcutService.RegisterShortcut(
            "up",
            InputContextType.Global,
            "w",
            ShortcutTriggerType.Press | ShortcutTriggerType.Repeat,
            500
        );
        _shortcutService.RegisterShortcut(
            "down",
            InputContextType.Global,
            "s",
            ShortcutTriggerType.Press | ShortcutTriggerType.Repeat,
            500
        );
        _shortcutService.RegisterShortcut(
            "left",
            InputContextType.Global,
            "a",
            ShortcutTriggerType.Press | ShortcutTriggerType.Repeat,
            500
        );
        _shortcutService.RegisterShortcut(
            "right",
            InputContextType.Global,
            "d",
            ShortcutTriggerType.Press | ShortcutTriggerType.Repeat,
            500
        );

        _shortcutService.RegisterShortcut("fireball", InputContextType.Global, "f", ShortcutTriggerType.Press);
        _actionService.RegisterAction(
            "fireball",
            () =>
            {
                if (_player != null)
                {
                    _particleSystem.EmitProjectile(
                        new(_player.Position.X, _player.Position.Y),
                        new(1, 0),
                        32f,
                        0,
                        1.5f,
                        LyColor.Orange,
                        LyColor.Firebrick,
                        18f
                    );
                }
            }
        );

        // Test Explosion - Premi E
        _shortcutService.RegisterShortcut("explosion", InputContextType.Global, "e", ShortcutTriggerType.Press);
        _actionService.RegisterAction(
            "explosion",
            () =>
            {
                if (_player != null)
                {
                    _particleSystem.EmitExplosion(
                        new(_player.Position.X + 2, _player.Position.Y),
                        1,
                        70,
                        26f,
                        4.2f,
                        LyColor.Yellow,
                        LyColor.OrangeRed,
                        18f
                    );
                }
            }
        );

        _actionService.RegisterAction(
            "up",
            () =>
            {
                var map = _worldManager.CurrentMap;
                var player = map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (map.GameObjectCanMove(player.Item, player.Position + Direction.Up))
                {
                    player.Item.Position += Direction.Up;
                    _fovSystem?.UpdateFov(map, player.Position);
                }
            }
        );
        _actionService.RegisterAction(
            "down",
            () =>
            {
                var map = _worldManager.CurrentMap;
                var player = map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (map.GameObjectCanMove(player.Item, player.Position + Direction.Down))
                {
                    player.Item.Position += Direction.Down;
                    _fovSystem?.UpdateFov(map, player.Position);
                }
            }
        );
        _actionService.RegisterAction(
            "left",
            () =>
            {
                var map = _worldManager.CurrentMap;
                var player = map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (map.GameObjectCanMove(player.Item, player.Position + Direction.Left))
                {
                    player.Item.Position += Direction.Left;
                    _fovSystem?.UpdateFov(map, player.Position);
                }
            }
        );
        _actionService.RegisterAction(
            "right",
            () =>
            {
                var map = _worldManager.CurrentMap;
                var player = map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (map.GameObjectCanMove(player.Item, player.Position + Direction.Right))
                {
                    player.Item.Position += Direction.Right;
                    _fovSystem?.UpdateFov(map, player.Position);
                }
            }
        );

        _screen.TileMouseDown += (index, x, y, buttons) =>
                                 {
                                     _screen.CenterViewOnTile(0, x, y);
                                     _particleSystem.EmitProjectile(
                                         new(x, y),
                                         new(1, 0),
                                         200f, // pixels/secondo
                                         0,    // ID del tile da renderizzare
                                         5f,   // secondi
                                         LyColor.Orange,
                                         LyColor.Firebrick,
                                         6f
                                     );

                                     // _screen.CenterViewOnTile(1, x, y);
                                     //
                                     // _screen.CenterViewOnTile(2, x, y);
                                 };

        _fovSystem = new();
        _worldManager.RegisterMapHandler(_fovSystem);
        AddEntity(_fovSystem);
        _mapRenderSystem = new(16);
        _mapRenderSystem.Configure(_screen, _fovSystem);
        _worldManager.RegisterMapHandler(_mapRenderSystem);
        AddEntity(_mapRenderSystem);

        _lightOverlaySystem = new(16);
        _lightOverlaySystem.Configure(_screen, _fovSystem);
        _worldManager.RegisterMapHandler(_lightOverlaySystem);
        AddEntity(_lightOverlaySystem);

        _viewportUpdateSystem = new(0);
        _viewportUpdateSystem.Configure(_screen, _mapRenderSystem);
        _worldManager.RegisterMapHandler(_viewportUpdateSystem);
        AddEntity(_viewportUpdateSystem);

        _worldManager.OnCurrentMapChanged += HandleCurrentMapChanged;
        _worldManager.GenerateMapAsync();

        _screen.SetLayerViewSmoothing(0, true);

        _screenManager.PushScreen(_screen);

        _screenManager.PushScreen(_uiRoot);
        base.OnLoad();
    }

    public override void OnUnload()
    {
        if (_mapRenderSystem != null)
        {
            _worldManager.UnregisterMapHandler(_mapRenderSystem);
        }

        if (_lightOverlaySystem != null)
        {
            _worldManager.UnregisterMapHandler(_lightOverlaySystem);
        }

        if (_viewportUpdateSystem != null)
        {
            _worldManager.UnregisterMapHandler(_viewportUpdateSystem);
        }

        _worldManager.OnCurrentMapChanged -= HandleCurrentMapChanged;

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

    private void HandleCurrentMapChanged(LyQuestMap? oldMap, LyQuestMap newMap)
    {
        if (_screen == null)
        {
            return;
        }

        // Initialize particle system providers with map and fov system
        (_particleCollisionProvider as GoRogueCollisionProvider)?.SetMap(newMap);

        var fovProvider = _particleFOVProvider as GoRogueFOVProvider;

        if (fovProvider != null && _fovSystem != null)
        {
            fovProvider.SetFovSystem(_fovSystem);
            fovProvider.SetMap(newMap);
        }

        _player = newMap.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        if (_player != null && _fovSystem != null)
        {
            _fovSystem.UpdateFov(newMap, _player.Position);
        }

        newMap.ObjectMoved += (sender, args) =>
                              {
                                  if (args.Item is CreatureGameObject creature)
                                  {
                                      _screen.EnqueueMove(
                                          creature.Layer,
                                          new(args.OldPosition.X, args.OldPosition.Y),
                                          new(args.NewPosition.X, args.NewPosition.Y),
                                          0.1f
                                      );

                                      _screen.CenterViewOnTile(0, args.NewPosition.X, args.NewPosition.Y);
                                  }
                              };
    }
}

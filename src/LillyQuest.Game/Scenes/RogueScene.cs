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
using LillyQuest.RogueLike.Interfaces.Systems;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;

namespace LillyQuest.Game.Scenes;

public class RogueScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly IMapGenerator _mapGenerator;
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
    private readonly List<IMapAwareSystem> _mapAwareSystems = new();

    private readonly ParticleSystem _particleSystem;

    private readonly ISystemManager _systemManager;

    public RogueScene(
        IScreenManager screenManager,
        IMapGenerator mapGenerator,
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
        _mapGenerator = mapGenerator;
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

        _screen.ParticlePixelRenderer = _particlePixelRenderer;
        _screen.ParticleLayerIndex = (int)MapLayer.Effects;

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
                        from: new Vector2(_player.Position.X, _player.Position.Y),
                        direction: new Vector2(1, 0),
                        speed: 32f,
                        tileId: 0,
                        lifetime: 1.5f,
                        foregroundColor: LyColor.Orange,
                        backgroundColor: LyColor.Firebrick,
                        scale: 18f
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
                        center: new Vector2(_player.Position.X + 2, _player.Position.Y),
                        tileId: 1,
                        particleCount: 70,
                        speed: 26f,
                        lifetime: 4.2f,
                        foregroundColor: LyColor.Yellow,
                        backgroundColor: LyColor.OrangeRed,
                        scale: 18f
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
                                         from: new Vector2(x,y),
                                         direction: new Vector2(1, 0),
                                         speed: 200f,            // pixels/secondo
                                         tileId: 0, // ID del tile da renderizzare
                                         lifetime: 5f,            // secondi
                                         foregroundColor: LyColor.Orange,
                                         backgroundColor: LyColor.Firebrick,
                                         scale: 6f
                                     );

                                     // _screen.CenterViewOnTile(1, x, y);
                                     //
                                     // _screen.CenterViewOnTile(2, x, y);
                                 };

        var map = _mapGenerator.GenerateMapAsync().GetAwaiter().GetResult();

        _fovSystem = new FovSystem();
        _fovSystem.RegisterMap(map);
        AddEntity(_fovSystem);
        _mapAwareSystems.Add(_fovSystem);

        // Initialize particle system providers with map and fov system
        (_particleCollisionProvider as GoRogueCollisionProvider)?.SetMap(map);

        var fovProvider = _particleFOVProvider as GoRogueFOVProvider;
        if (fovProvider != null && _fovSystem != null)
        {
            fovProvider.SetFovSystem(_fovSystem);
            fovProvider.SetMap(map);
        }

        _player = map.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        if (_player != null)
        {
            _fovSystem.UpdateFov(map, _player.Position);
        }

        _mapRenderSystem = new MapRenderSystem(chunkSize: 16);
        _mapRenderSystem.Configure(_screen, _fovSystem);
        _worldManager.RegisterMapHandler(_mapRenderSystem);
        AddEntity(_mapRenderSystem);
        _mapAwareSystems.Add(_mapRenderSystem);

        _lightOverlaySystem = new LightOverlaySystem(chunkSize: 16);
        _lightOverlaySystem.Configure(_screen, _fovSystem);
        _worldManager.RegisterMapHandler(_lightOverlaySystem);
        AddEntity(_lightOverlaySystem);
        _mapAwareSystems.Add(_lightOverlaySystem);

        _viewportUpdateSystem = new ViewportUpdateSystem(layerIndex: 0);
        _viewportUpdateSystem.Configure(_screen, _mapRenderSystem);
        _worldManager.RegisterMapHandler(_viewportUpdateSystem);
        AddEntity(_viewportUpdateSystem);
        _mapAwareSystems.Add(_viewportUpdateSystem);

        _worldManager.CurrentMap = map;

        map.ObjectMoved += (sender, args) =>
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
        var map = _worldManager.CurrentMap;
        foreach (var system in _mapAwareSystems)
        {
            system.UnregisterMap(map);
        }
        _mapAwareSystems.Clear();

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

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
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Services;
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

    private TileRenderData DarkenTile(TileRenderData tile)
    {
        return new TileRenderData(
            tile.TileIndex,
            new LyColor(
                tile.ForegroundColor.A,
                (byte)(tile.ForegroundColor.R * 0.5f),
                (byte)(tile.ForegroundColor.G * 0.5f),
                (byte)(tile.ForegroundColor.B * 0.5f)
            ),
            tile.BackgroundColor,
            tile.Flip
        );
    }

    private void FillSurface(TilesetSurfaceScreen screen)
    {
        foreach (var position in _map.Positions())
        {
            bool isVisible = _fovService.IsVisible(position);
            bool isExplored = _fovService.IsExplored(position);

            // Layer 0: Terrain
            if (_map.GetTerrainAt(position) is TerrainGameObject terrain)
            {
                var visualTile = terrain.Tile;
                var tile = new TileRenderData(
                    visualTile.Symbol[0],
                    visualTile.ForegroundColor,
                    visualTile.BackgroundColor
                );

                if (!isVisible && isExplored)
                {
                    // Explored but out of range - darken
                    tile = DarkenTile(tile);
                }
                else if (!isExplored)
                {
                    // Never explored - don't render
                    tile = new TileRenderData(-1, LyColor.White);
                }

                screen.AddTileToSurface(0, position.X, position.Y, tile);
            }

            // Layer 2: Creatures (only visible or if player)
            var objects = _map.GetObjectsAt(position);

            foreach (var obj in objects)
            {
                if (isVisible || obj == _player)
                {
                    if (obj is CreatureGameObject creature)
                    {
                        var creatureTile = new TileRenderData(
                            creature.Tile.Symbol[0],
                            creature.Tile.ForegroundColor,
                            creature.Tile.BackgroundColor
                        );
                        screen.AddTileToSurface(2, position.X, position.Y, creatureTile);
                    }
                }
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
        _screen.ApplyTileViewScaleToScreen(availableSize, includeMargins: true);

        // _screen.SetLayerTileset(0, "alloy");
        // _screen.SetLayerTileset(1, "alloy");

        _screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(1, true, 0.1f);
        _screen.SetLayerRenderScaleSmoothing(2, true, 0.1f);

        _shortcutService.RegisterShortcut("up", InputContextType.Global, "w", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("down", InputContextType.Global, "s", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("left", InputContextType.Global, "a", ShortcutTriggerType.Release);
        _shortcutService.RegisterShortcut("right", InputContextType.Global, "d", ShortcutTriggerType.Release);

        _actionService.RegisterAction(
            "up",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Up))
                {
                    player.Item.Position += Direction.Up;
                    _fovService.UpdateFOV(player.Position);
                    FillSurface(_screen!);
                }
            }
        );
        _actionService.RegisterAction(
            "down",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Down))
                {
                    player.Item.Position += Direction.Down;
                    _fovService.UpdateFOV(player.Position);
                    FillSurface(_screen!);
                }
            }
        );
        _actionService.RegisterAction(
            "left",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Left))
                {
                    player.Item.Position += Direction.Left;
                    _fovService.UpdateFOV(player.Position);
                    FillSurface(_screen!);
                }
            }
        );
        _actionService.RegisterAction(
            "right",
            () =>
            {
                var player = _map.Entities.GetLayer((int)MapLayer.Creatures).First();

                if (_map.GameObjectCanMove(player.Item, player.Position + Direction.Right))
                {
                    player.Item.Position += Direction.Right;
                    _fovService.UpdateFOV(player.Position);
                    FillSurface(_screen!);
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

        FillSurface(_screen);

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

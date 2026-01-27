using System.Numerics;
using Humanizer;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Screens.UI;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Game.Scenes;

public class RogueScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly IMapGenerator _mapGenerator;
    private readonly ITilesetManager _tilesetManager;

    private UIRootScreen? _uiRoot;
    private TilesetSurfaceScreen? _screen;

    public RogueScene(IScreenManager screenManager, IMapGenerator mapGenerator, ITilesetManager tilesetManager) : base(
        "RogueScene"
    )
    {
        _screenManager = screenManager;
        _mapGenerator = mapGenerator;
        _tilesetManager = tilesetManager;
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

        _screen.ApplyTileViewScaleToScreen(availableSize, includeMargins: true);

        _screen.InitializeLayers(_screen.LayerCount);
        // _screen.SetLayerTileset(0, "alloy");
        // _screen.SetLayerTileset(1, "alloy");

        _screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);

        _screen.TileMouseDown += (index, x, y, buttons) =>
                                 {
                                     _screen.CenterViewOnTile(0, x, y);
                                 };

        var map = _mapGenerator.GenerateMapAsync().GetAwaiter().GetResult();
        map.FillSurface(_screen);

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

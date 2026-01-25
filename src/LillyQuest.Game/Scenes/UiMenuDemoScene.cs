using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Game.Scenes;

public class UiMenuDemoScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly EngineRenderContext _renderContext;
    private UIRootScreen? _screen;
    private bool _subscribed;

    public UiMenuDemoScene(
        IScreenManager screenManager,
        LillyQuestBootstrap bootstrap,
        EngineRenderContext renderContext
    )
        : base("ui_menu_demo")
    {
        _screenManager = screenManager;
        _bootstrap = bootstrap;
        _renderContext = renderContext;
    }

    public override void OnLoad()
    {
        var initialSize = _renderContext.Window != null
            ? new Vector2(_renderContext.Window.Size.X, _renderContext.Window.Size.Y)
            : new Vector2(1280, 720);

        _screen = new UIRootScreen
        {
            Position = Vector2.Zero,
            Size = initialSize
        };

        var menu = new UIMenu
        {
            Size = new(260, 180),
            ItemHeight = 28,
            ItemSpacing = 6,
            Padding = new(12, 12, 12, 12),
            Font = new("default_font", 16, FontKind.TrueType),
            TextColor = LyColor.White,
            SelectedColor = LyColor.FromHex("#ffd88a"),
            HoveredColor = LyColor.FromHex("#ffffff"),
            PressedColor = LyColor.FromHex("#c8b27a"),
            DisabledColor = LyColor.FromHex("#808080")
        };

        menu.SetItems(new List<MenuItem>
        {
            new("Start", () => { }),
            new("Options", () => { }),
            new("Load Game", () => { }),
            new("Credits", () => { }),
            new("Quit", () => { })
        });

        menu.CenterIn(_screen.Size);
        menu.KeepCentered = true;

        _screen.Root.Add(menu);
        _screenManager.PushScreen(_screen);

        if (!_subscribed)
        {
            _bootstrap.WindowResize += OnWindowResize;
            _subscribed = true;
        }
    }

    public override void OnUnload()
    {
        if (_screen != null)
        {
            _screenManager.PopScreen(_screen);
            _screen = null;
        }

        if (_subscribed)
        {
            _bootstrap.WindowResize -= OnWindowResize;
            _subscribed = false;
        }
    }

    private void OnWindowResize(Vector2 size)
    {
        if (_screen == null)
        {
            return;
        }

        _screen.HandleResize(size);
    }
}

using System.Numerics;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Game.Scenes;

public class UiMenuDemoScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private UIRootScreen? _screen;

    public UiMenuDemoScene(IScreenManager screenManager)
        : base("ui_menu_demo")
        => _screenManager = screenManager;

    public override void OnLoad()
    {
        _screen = new UIRootScreen
        {
            Position = Vector2.Zero,
            Size = new(1280, 720)
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
    }

    public override void OnUnload()
    {
        if (_screen != null)
        {
            _screenManager.PopScreen(_screen);
            _screen = null;
        }
    }
}

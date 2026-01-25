using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Game.Scenes;

public class UiTextBoxDemoScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly EngineRenderContext _renderContext;
    private UIRootScreen? _screen;
    private bool _subscribed;

    public UiTextBoxDemoScene(
        IScreenManager screenManager,
        INineSliceAssetManager nineSliceManager,
        ITextureManager textureManager,
        LillyQuestBootstrap bootstrap,
        EngineRenderContext renderContext
    )
        : base("ui_textbox_demo")
    {
        _screenManager = screenManager;
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
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

        var textBox = new UITextBox(_nineSliceManager, _textureManager)
        {
            Size = new(360, 24),
            NineSliceKey = "simple_ui",
            Font = new("default_font", 16, FontKind.TrueType),
            TextColor = LyColor.White,
            BackgroundTint = LyColor.FromHex("#d9caa3"),
            VerticalPadding = 1f,
            Text = "Type here..."
        };

        textBox.CenterIn(_screen.Size);
        textBox.KeepCentered = true;

        _screen.Root.Add(textBox);
        _screen.Root.FocusManager.RequestFocus(textBox);
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

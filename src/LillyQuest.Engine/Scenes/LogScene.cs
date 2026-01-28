using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Logging;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.Logging;

namespace LillyQuest.Engine.Scenes;

public class LogScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly ILogEventDispatcher _logDispatcher;
    private readonly IFontManager _fontManager;
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly EngineRenderContext _engineRenderContext;
    private bool _subscribed;

    private LogScreen? _logScreen;

    public LogScene(
        IScreenManager screenManager,
        ILogEventDispatcher logDispatcher,
        IFontManager fontManager,
        LillyQuestBootstrap bootstrap,
        EngineRenderContext engineRenderContext
    )
        : base("log_scene")
    {
        _screenManager = screenManager;
        _logDispatcher = logDispatcher;
        _fontManager = fontManager;
        _bootstrap = bootstrap;
        _engineRenderContext = engineRenderContext;
        Subscribe();
    }

    public override void OnLoad()
    {
        _logScreen = new LogScreen(_logDispatcher, _fontManager)
        {
            Position = Vector2.Zero,
            Size = new Vector2(_engineRenderContext.Window.Size.X, _engineRenderContext.Window.Size.Y),
            Margin = new(10, 10, 10, 10),
            BackgroundColor = LyColor.Black,
            BackgroundAlpha = 0.9f
        };

        _screenManager.PushScreen(_logScreen);
    }

    public override void OnUnload()
    {
        if (_logScreen != null)
        {
            _screenManager.PopScreen(_logScreen);
            _logScreen = null;
        }

        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_subscribed)
        {
            return;
        }

        _bootstrap.WindowResize += OnWindowResize;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed)
        {
            return;
        }

        _bootstrap.WindowResize -= OnWindowResize;
        _subscribed = false;
    }

    private void OnWindowResize(Vector2 size)
    {
        if (_logScreen != null)
        {
            _logScreen.Size = size;
        }
    }
}

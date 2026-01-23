using System.Numerics;
using LillyQuest.Core.Data.Contexts;
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
    private readonly EngineRenderContext _renderContext;

    private LogScreen? _logScreen;

    public LogScene(IScreenManager screenManager, ILogEventDispatcher logDispatcher, IFontManager fontManager, EngineRenderContext renderContext)
        : base("log_scene")
    {
        _screenManager = screenManager;
        _logDispatcher = logDispatcher;
        _fontManager = fontManager;
        _renderContext = renderContext;
    }

    public override void OnLoad()
    {
        _logScreen = new LogScreen(_logDispatcher, _fontManager)
        {
            Position = Vector2.Zero,
            Size = new(1600, 900),
            Margin = new(10, 10, 10, 10),
            FontSize = 8,
            BackgroundColor = LyColor.Black,
            BackgroundAlpha = 0.6f
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
    }
}

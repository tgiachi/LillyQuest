using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Primitives;
using Serilog;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LillyQuest.Engine;

public class LillyQuestBootstrap
{
    private readonly ILogger _logger = Log.ForContext<LillyQuestBootstrap>();

    private readonly IContainer _container = new Container();
    private EngineRenderContext _renderContext;

    private readonly GameTime _gameTime = new();

    private readonly LillyQuestEngineConfig _engineConfig;

    private GL _gl;
    private IWindow _window;


    public LillyQuestBootstrap(LillyQuestEngineConfig engineConfig)
    {
        _engineConfig = engineConfig;
        _container.RegisterInstance(_engineConfig);
    }

    public void RegisterServices(Func<IContainer, IContainer> registerServices) { }

    public void Initialize()
    {
        _logger.Information("Initializing LillyQuest Engine...");

        var options = WindowOptions.Default;
        options.Size = new(_engineConfig.Render.Width, _engineConfig.Render.Height);
        options.Title = _engineConfig.Render.Title;
        options.VSync = _engineConfig.Render.IsVSyncEnabled;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(4, 6));

        _window = Window.Create(options);
        _window.Update += WindowOnUpdate;
        _window.Load += WindowOnLoad;
        _window.Closing += WindowOnClosing;
        _window.Render += WindowOnRender;

    }

    private void WindowOnRender(double obj)
    {

    }

    private void WindowOnClosing()
    {
    }

    private void WindowOnLoad()
    {
        _gl = GL.GetApi(_window);

    }

    private void WindowOnUpdate(double obj)
    {
        _gameTime.Update(obj);
    }

    public void Run()
    {
        _window.Run();
    }
}

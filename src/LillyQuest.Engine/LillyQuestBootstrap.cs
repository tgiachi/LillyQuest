using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Systems;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LillyQuest.Engine;

public class LillyQuestBootstrap
{
    public delegate void TickEventHandler(GameTime gameTime);

    private readonly ILogger _logger = Log.ForContext<LillyQuestBootstrap>();

    private IContainer _container = new Container();
    private readonly EngineRenderContext _renderContext = new();
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly GameTime _gameTime = new();
    private readonly GameTime _fixedGameTime = new();
    private double _fixedUpdateAccumulator;

    private readonly LillyQuestEngineConfig _engineConfig;

    private GL _gl;
    private IWindow _window;

    /// <summary>
    /// Event invoked at fixed timestep intervals during the game loop.
    /// </summary>
    public event TickEventHandler? FixedUpdate;

    /// <summary>
    ///  Event invoked every frame during the game loop for updating game logic.
    /// </summary>
    public event TickEventHandler? Update;

    /// <summary>
    ///  Event invoked every frame during the rendering phase of the game loop. fm
    /// </summary>
    public event TickEventHandler? Render;

    public LillyQuestBootstrap(LillyQuestEngineConfig engineConfig)
    {
        _engineConfig = engineConfig;
        _container.RegisterInstance(_engineConfig);
        _container.RegisterInstance(_renderContext);
        _container.RegisterInstance(this);

        if (string.IsNullOrEmpty(_engineConfig.RootDirectory))
        {
            _engineConfig.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "LillyQuest");
        }

        _directoriesConfig = new DirectoriesConfig(_engineConfig.RootDirectory, Enum.GetNames<DirectoryType>());
        _container.RegisterInstance(_directoriesConfig);
        _logger.Information("Root Directory: {RootDirectory}", _engineConfig.RootDirectory);
    }

    public void RegisterServices(Func<IContainer, IContainer> registerServices)
    {
        _container = registerServices(_container);
    }

    public void Initialize()
    {
        _renderContext.ClearColor = LyColor.CornflowerBlue;
        _logger.Information("Initializing LillyQuest Engine...");

        var options = WindowOptions.Default;
        options.Size = new(_engineConfig.Render.Width, _engineConfig.Render.Height);
        options.Title = _engineConfig.Render.Title;
        options.VSync = _engineConfig.Render.IsVSyncEnabled;
        options.Samples = _engineConfig.Render.Mssa;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(4, 1));

        _window = Window.Create(options);
        _window.Update += WindowOnUpdate;
        _window.Load += WindowOnLoad;
        _window.Closing += WindowOnClosing;
        _window.Render += WindowOnRender;
        _window.Resize += WindowOnResize;

        LoadDefaultResources();
        RegisterInternalServices();
    }

    private void LoadDefaultResources()
    {
        _logger.Information("Loading default resources...");

    }

    private void RegisterInternalServices()
    {
        _container.Register<IGameEntityManager, GameEntityManager>(Reuse.Singleton);
        _container.Register<ISystemManager, SystemManager>(Reuse.Singleton);
        _container.Register<IAssetManager, AssetManager>(Reuse.Singleton);

        _container.Register<UpdateSystem>(Reuse.Singleton);
    }

    private void StartInternalServices()
    {
        var systemManager = _container.Resolve<ISystemManager>();

        var updateSystem = _container.Resolve<UpdateSystem>();

        systemManager.AddUpdateSystem(updateSystem);


    }

    private void WindowOnResize(Vector2D<int> obj)
    {
        _logger.Information("Window Resized to {Width}x{Height}", obj.X, obj.Y);
    }

    private void WindowOnRender(double obj)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.ClearColor(_renderContext.ClearColor.ToSystemColor());

        Render?.Invoke(_gameTime);
    }

    private void WindowOnClosing()
    {
        _logger.Information("Shutting down LillyQuest Engine...");
    }

    private unsafe void WindowOnLoad()
    {
        _gl = GL.GetApi(_window);
        _renderContext.Gl = _gl;
        _renderContext.Window = _window;
        _renderContext.InputContext = _window.CreateInput();

        var vendor = SilkMarshal.PtrToString((nint)_gl.GetString(StringName.Vendor));
        var renderer = SilkMarshal.PtrToString((nint)_gl.GetString(StringName.Renderer));
        var glsl = SilkMarshal.PtrToString((nint)_gl.GetString(StringName.ShadingLanguageVersion));
        var version = SilkMarshal.PtrToString((nint)_gl.GetString(StringName.Version));

        var extensions = SilkMarshal.PtrToString((nint)_gl.GetString(StringName.Extensions));

        _logger.Information("Vendor: {Vendor}", vendor);
        _logger.Information("Renderer: {Renderer}", renderer);
        _logger.Information("OpenGL Version: {Version}", version);
        _logger.Information("GLSL Version: {GLSL}", glsl);
        _logger.Information("Extensions: {Ext}", extensions);

        StartInternalServices();
    }

    private void WindowOnUpdate(double deltaSeconds)
    {
        _gameTime.Update(deltaSeconds);
        Update?.Invoke(_gameTime);

        _fixedUpdateAccumulator += deltaSeconds;

        while (_fixedUpdateAccumulator >= _engineConfig.FixedTimestep)
        {
            _fixedUpdateAccumulator -= _engineConfig.FixedTimestep;
            _fixedGameTime.Update(_engineConfig.FixedTimestep);
            FixedUpdate?.Invoke(_fixedGameTime);
        }
    }

    public void Run()
    {
        _window.Run();
    }
}

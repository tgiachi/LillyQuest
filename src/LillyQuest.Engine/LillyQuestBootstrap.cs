using System.Diagnostics;
using System.Numerics;
using DryIoc;
using LillyQuest.Core.Data.Configs;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Data.Directories;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Internal.Data.Registrations;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Extensions;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Managers.Scenes;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Managers.Entities;
using LillyQuest.Engine.Managers.Services;
using LillyQuest.Engine.Services;
using LillyQuest.Engine.Systems;
using LillyQuest.Scripting.Lua.Data.Config;
using LillyQuest.Scripting.Lua.Extensions.Scripts;
using LillyQuest.Scripting.Lua.Interfaces;
using LillyQuest.Scripting.Lua.Services;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LillyQuest.Engine;

public class LillyQuestBootstrap
{
    private readonly Type[] _renderSystems =
    [
        typeof(UpdateSystem),
        typeof(ImGuiSystem),
        typeof(FixedUpdateSystem)
    ];

    public delegate void TickEventHandler(GameTime gameTime);

    private readonly ILogger _logger = Log.ForContext<LillyQuestBootstrap>();

    private IContainer _container = new Container();
    private readonly EngineRenderContext _renderContext = new();
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly GameTime _gameTime = new();
    private readonly GameTime _fixedGameTime = new();
    private double _fixedUpdateAccumulator;

    public TimeSpan RenderTime { get; set; }
    public TimeSpan UpdateTime { get; set; }

    private readonly LillyQuestEngineConfig _engineConfig;

    private GL _gl;
    private IWindow _window;

    /// <summary>
    /// Event invoked at fixed timestep intervals during the game loop.
    /// </summary>
    public event TickEventHandler? FixedUpdate;

    /// <summary>
    /// Event invoked every frame during the game loop for updating game logic.
    /// </summary>
    public event TickEventHandler? Update;

    /// <summary>
    /// Event invoked every frame during the rendering phase of the game loop. fm
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

        _directoriesConfig = new(_engineConfig.RootDirectory, Enum.GetNames<DirectoryType>());
        _container.RegisterInstance(_directoriesConfig);
        _logger.Information("Root Directory: {RootDirectory}", _engineConfig.RootDirectory);
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
        options.API = new(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new(4, 1));

        _window = Window.Create(options);
        _window.Update += WindowOnUpdate;
        _window.Load += WindowOnLoad;
        _window.Closing += WindowOnClosing;
        _window.Render += WindowOnRender;
        _window.Resize += WindowOnResize;

        RegisterInternalServices();


    }

    public void RegisterServices(Func<IContainer, IContainer> registerServices)
    {
        _container = registerServices(_container);
    }

    public void Run()
    {
        _window.Run();
    }

    /// <summary>
    /// Prepares the rendering context for a new frame.
    /// Clears buffers and sets up the rendering state.
    /// </summary>
    private void BeginFrame()
    {
        _gl.ClearColor(_renderContext.ClearColor.ToSystemColor());
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    /// <summary>
    /// Finalizes the frame after rendering.
    /// Can be used for post-processing, debug rendering, or cleanup.
    /// </summary>
    private void EndFrame()
    {
        var sceneManager = _container.Resolve<ISceneManager>();

        sceneManager.RenderFadeOverlay(_window.Size);
    }

    private void LoadDefaultResources()
    {
        var assetManager = _container.Resolve<IAssetManager>();

        _logger.Information("Loading default resources...");

        assetManager.ShaderManager.LoadShaderFromEmbeddedResource(
            "texture2d",
            "Assets/Shaders/spritebatch.vert",
            "Assets/Shaders/spritebatch.frag",
            typeof(SpriteBatch).Assembly
        );

        assetManager.FontManager.LoadFontFromEmbeddedResource(
            "default_font",
            "Assets/Fonts/default_font.ttf",
            typeof(SpriteBatch).Assembly
        );

        assetManager.FontManager.LoadFontFromEmbeddedResource(
            "default_font_log",
            "Assets/Fonts/default_log_font.ttf",
            typeof(SpriteBatch).Assembly
        );

        assetManager.TilesetManager.LoadTilesetFromEmbeddedResource(
            "alloy",
            "Assets/Tilesets/Alloy_curses_12x12.png",
            12,
            12,
            0,
            0,
            typeof(SpriteBatch).Assembly
        );
    }

    private void RegisterInternalServices()
    {
        _container.Register<ITextureManager, TextureManager>(Reuse.Singleton);
        _container.Register<IShaderManager, ShaderManager>(Reuse.Singleton);
        _container.Register<IFontManager, FontManager>(Reuse.Singleton);
        _container.Register<IAudioManager, AudioManager>(Reuse.Singleton);
        _container.Register<ITilesetManager, TilesetManager>(Reuse.Singleton);

        _container.Register<IGameEntityManager, GameEntityManager>(Reuse.Singleton);
        _container.Register<ISystemManager, SystemManager>(Reuse.Singleton);

        _container.Register<ISceneManager, SceneTransitionManager>(Reuse.Singleton);





        _container.Register<IAssetManager, AssetManager>(Reuse.Singleton);

        _container.Register<IActionService, ActionService>(Reuse.Singleton);
        _container.Register<IShortcutService, ShortcutService>(Reuse.Singleton);

        foreach (var systemType in _renderSystems)
        {
            _container.Register(systemType, Reuse.Singleton);
        }

        _container.RegisterInstance(
            new LuaEngineConfig(
                _directoriesConfig[DirectoryType.Scripts],
                _directoriesConfig[DirectoryType.Scripts],
                "0.5.0"
            )
        );
        _container.Register<IScriptEngineService, LuaScriptEngineService>(Reuse.Singleton);
    }

    private void StartInternalServices()
    {
        _logger.Information("Starting LillyQuest Engine...");

        var systemManager = _container.Resolve<ISystemManager>();

        foreach (var systemType in _renderSystems)
        {
            var system = (ISystem)_container.Resolve(systemType);
            systemManager.RegisterSystem(system);
        }

        systemManager.InitializeAllSystems();

        _container.RegisterLuaUserData<Vector2>();

        var scriptEngine = _container.Resolve<IScriptEngineService>();
        scriptEngine.StartAsync().GetAwaiter().GetResult();

        // Load initial scene if registered
        if (_container.IsRegistered<List<SceneRegistrationObject>>())
        {
            var sceneRegistrations = _container.Resolve<List<SceneRegistrationObject>>();
            var initialScene = sceneRegistrations.FirstOrDefault(r => r.IsInitial);

            if (initialScene != null)
            {
                var sceneManager = _container.Resolve<ISceneManager>();
                var sceneInstance = (IScene)_container.Resolve(initialScene.SceneType);
                sceneManager.SwitchScene(sceneInstance.Name, fadeDuration: 0.0f);
                _logger.Information("Loaded initial scene '{SceneName}'", sceneInstance.Name);
            }
        }

        var entityManager = _container.Resolve<IGameEntityManager>();
        entityManager.AddEntity(new TestGameEntity());
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

        LoadDefaultResources();
        StartInternalServices();
    }

    private void WindowOnRender(double obj)
    {
        var sw = Stopwatch.GetTimestamp();
        BeginFrame();
        Render?.Invoke(_gameTime);
        EndFrame();
        RenderTime = Stopwatch.GetElapsedTime(sw);
    }

    private void WindowOnResize(Vector2D<int> obj)
    {
        _logger.Information("Window Resized to {Width}x{Height}", obj.X, obj.Y);
        _renderContext.Gl.Viewport(0, 0, (uint)obj.X, (uint)obj.Y);
    }

    private void WindowOnUpdate(double deltaSeconds)
    {
        var sw = Stopwatch.GetTimestamp();
        _gameTime.Update(deltaSeconds);

        var sceneManager = _container.Resolve<ISceneManager>();
        sceneManager.Update(_gameTime);

        Update?.Invoke(_gameTime);
        UpdateTime = Stopwatch.GetElapsedTime(sw);
        _fixedUpdateAccumulator += deltaSeconds;

        while (_fixedUpdateAccumulator >= _engineConfig.FixedTimestep)
        {
            _fixedUpdateAccumulator -= _engineConfig.FixedTimestep;
            _fixedGameTime.Update(_engineConfig.FixedTimestep);
            FixedUpdate?.Invoke(_fixedGameTime);
        }
    }
}

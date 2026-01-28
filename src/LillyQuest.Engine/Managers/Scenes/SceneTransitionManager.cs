using System.Numerics;
using DryIoc;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Internal.Data.Registrations;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Data;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using Serilog;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Managers.Scenes;

/// <summary>
/// Manages scene transitions with fade animations and entity lifecycle.
/// Handles loading, unloading, and rendering of scenes with smooth fade transitions.
/// </summary>
public sealed class SceneTransitionManager : ISceneManager, IDisposable
{
    private readonly IGameEntityManager _entityManager;
    private readonly List<SceneRegistrationObject> _sceneRegistrations;
    private readonly ILogger _logger = Log.ForContext<SceneTransitionManager>();

    private readonly SpriteBatch _spriteBatch;

    // Scene management
    private readonly Stack<IScene> _sceneStack = new();
    private readonly Dictionary<string, IScene> _initializedScenes = new();
    private readonly HashSet<string> _scenesWithGlobalsRegistered = new();

    // Entity tracking (Dictionary[sceneName] = List[entityIds])
    private readonly Dictionary<string, List<uint>> _sceneEntityIds = new();
    private readonly List<uint> _globalEntityIds = new();

    private readonly IContainer _container;

    // Fade transition state
    private float _fadeAlpha; // 0.0 = transparent, 1.0 = opaque black
    private float _fadeDuration = 0.5f;
    private float _fadeProgress;
    private string? _pendingSceneName;

    public IScene? CurrentScene => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;
    public SceneTransitionState TransitionState { get; private set; } = SceneTransitionState.Idle;

    /// <summary>
    /// Creates a new SceneTransitionManager with the specified dependencies.
    /// </summary>
    public SceneTransitionManager(
        IGameEntityManager entityManager,
        EngineRenderContext renderContext,
        IShaderManager shaderManager,
        IFontManager fontManager,
        ITextureManager textureManager,
        List<SceneRegistrationObject> sceneRegistrations,
        IContainer container
    )
    {
        _entityManager = entityManager;
        _sceneRegistrations = sceneRegistrations;
        _container = container;
        _spriteBatch = new(
            renderContext,
            shaderManager,
            fontManager,
            "texture2d",
            BatchingMode.Deferred,
            textureManager
        );

        _logger.Information("SceneTransitionManager initialized");
    }

    public void Dispose()
    {
        _spriteBatch.Dispose();
    }

    /// <summary>
    /// Gets all available scenes (both registered and initialized).
    /// Returns a read-only list of scenes that have been initialized.
    /// </summary>
    public IReadOnlyList<IScene> GetAvailableScenes()
        => _initializedScenes.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets all registered scene names (including those not yet initialized).
    /// </summary>
    public IReadOnlyList<string> GetRegisteredSceneNames()
        => SceneRegistrationNameResolver.ResolveRegisteredSceneNames(_sceneRegistrations, _container);

    /// <summary>
    /// Renders the fade overlay on top of all scene rendering.
    /// </summary>
    public void RenderFadeOverlay(Vector2D<int> screenSize)
    {
        if (_fadeAlpha <= 0.0f)
        {
            return;
        }

        _spriteBatch.Begin();
        var fadeColor = LyColor.Black.WithAlpha((byte)(_fadeAlpha * 255));
        _spriteBatch.DrawRectangle(
            Vector2.Zero,
            new(screenSize.X, screenSize.Y),
            fadeColor,
            1.0f
        );
        _spriteBatch.End();
    }

    /// <summary>
    /// Initiates a scene transition with fade animation.
    /// </summary>
    /// <param name="sceneName">Name of the scene to transition to.</param>
    /// <param name="fadeDuration">Duration of fade animation in seconds.</param>
    public void SwitchScene(string sceneName, float fadeDuration = 0.5f)
    {
        if (TransitionState != SceneTransitionState.Idle)
        {
            _logger.Warning(
                "Scene transition already in progress. Ignoring SwitchScene({SceneName})",
                sceneName
            );

            return;
        }

        _logger.Information(
            "Initiating scene transition to '{SceneName}' with fade duration {Duration}s",
            sceneName,
            fadeDuration
        );

        _pendingSceneName = sceneName;
        _fadeDuration = fadeDuration;
        _fadeProgress = 0.0f;
        _fadeAlpha = 0.0f;
        TransitionState = SceneTransitionState.FadingOut;
    }

    /// <summary>
    /// Updates the scene transition state machine and fade animation.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (TransitionState == SceneTransitionState.Idle)
        {
            return;
        }

        _fadeProgress += (float)gameTime.Elapsed.TotalSeconds / _fadeDuration;

        switch (TransitionState)
        {
            case SceneTransitionState.FadingOut:
                _fadeAlpha = Lerp(0.0f, 1.0f, _fadeProgress);

                if (_fadeProgress >= 1.0f)
                {
                    _fadeAlpha = 1.0f;
                    TransitionState = SceneTransitionState.Loading;
                    _logger.Debug("Fade out complete, switching scenes");
                }

                break;

            case SceneTransitionState.Loading:
                ExecuteSceneSwitch();
                _fadeProgress = 0.0f;
                TransitionState = SceneTransitionState.FadingIn;
                _logger.Debug("Scene switch executed, beginning fade in");

                break;

            case SceneTransitionState.FadingIn:
                _fadeAlpha = Lerp(1.0f, 0.0f, _fadeProgress);

                if (_fadeProgress >= 1.0f)
                {
                    _fadeAlpha = 0.0f;
                    TransitionState = SceneTransitionState.Idle;
                    _logger.Information("Scene transition complete");
                }

                break;
        }
    }

    /// <summary>
    /// Adds all scene entities to the entity manager and tracks their IDs.
    /// </summary>
    private void AddSceneEntities(IScene scene)
    {
        var sceneEntities = scene.GetSceneGameObjects().ToList();
        var entityIds = new List<uint>();

        foreach (var entity in sceneEntities)
        {
            _entityManager.AddEntity(entity);
            entityIds.Add(entity.Id);
        }

        _sceneEntityIds[scene.Name] = entityIds;
        _logger.Debug("Added {Count} scene entities for '{SceneName}'", entityIds.Count, scene.Name);
    }

    /// <summary>
    /// Executes the actual scene switch (unload old, load new).
    /// Called during the Loading state when screen is black.
    /// </summary>
    private void ExecuteSceneSwitch()
    {
        if (string.IsNullOrEmpty(_pendingSceneName))
        {
            _logger.Error("ExecuteSceneSwitch called with null pending scene name");

            return;
        }

        // Unload current scene if one exists
        if (_sceneStack.Count > 0)
        {
            var currentScene = _sceneStack.Pop();
            _logger.Debug("Unloading scene '{SceneName}'", currentScene.Name);
            currentScene.OnUnload();
            RemoveSceneEntities(currentScene);
        }

        // Get or initialize new scene
        var newScene = GetOrInitializeScene(_pendingSceneName);
        _logger.Debug("Loaded scene '{SceneName}'", newScene.Name);

        // Register globals on first load
        RegisterSceneGlobals(newScene);

        // Call OnLoad and add entities
        newScene.OnLoad();
        AddSceneEntities(newScene);

        // Push new scene onto stack
        _sceneStack.Push(newScene);
        _pendingSceneName = null;
    }

    /// <summary>
    /// Gets or initializes a scene by name.
    /// Lazy loads and caches scenes from the DI container.
    /// </summary>
    private IScene GetOrInitializeScene(string sceneName)
    {
        if (_initializedScenes.TryGetValue(sceneName, out var cachedScene))
        {
            return cachedScene;
        }

        var registration = _sceneRegistrations.FirstOrDefault(
            r =>
            {
                // Try matching by scene instance's Name property
                var scene = (IScene)_container.Resolve(r.SceneType);

                return scene.Name == sceneName;
            }
        );

        if (registration == null)
        {
            _logger.Error("Scene '{SceneName}' not registered in container", sceneName);

            throw new InvalidOperationException($"Scene '{sceneName}' not registered.");
        }

        var newScene = (IScene)_container.Resolve(registration.SceneType);
        newScene.OnInitialize(this);

        _logger.Debug("Initialized scene '{SceneName}'", newScene.Name);
        _initializedScenes[sceneName] = newScene;

        return newScene;
    }

    /// <summary>
    /// Linear interpolation between two values.
    /// </summary>
    private static float Lerp(float from, float to, float t)
    {
        t = Math.Clamp(t, 0.0f, 1.0f);

        return from + (to - from) * t;
    }

    /// <summary>
    /// Registers global entities for a scene (called once on first load).
    /// Prevents duplicate calls via a HashSet.
    /// </summary>
    private void RegisterSceneGlobals(IScene scene)
    {
        if (_scenesWithGlobalsRegistered.Contains(scene.Name))
        {
            return;
        }

        var beforeCount = _entityManager.OrderedEntities.Count;
        scene.RegisterGlobals(_entityManager);
        var afterCount = _entityManager.OrderedEntities.Count;

        // Track newly added entities as globals
        for (var i = beforeCount; i < afterCount; i++)
        {
            var entity = _entityManager.OrderedEntities[i];
            _globalEntityIds.Add(entity.Id);
        }

        _scenesWithGlobalsRegistered.Add(scene.Name);
        _logger.Debug(
            "Registered {Count} global entities for scene '{SceneName}'",
            afterCount - beforeCount,
            scene.Name
        );
    }

    /// <summary>
    /// Removes scene entities from the entity manager (but keeps globals).
    /// </summary>
    private void RemoveSceneEntities(IScene scene)
    {
        if (!_sceneEntityIds.TryGetValue(scene.Name, out var entityIds))
        {
            return;
        }

        var removedCount = 0;

        foreach (var entityId in entityIds)
        {
            var entity = _entityManager.GetEntityById(entityId);

            if (entity != null && !_globalEntityIds.Contains(entityId))
            {
                _entityManager.RemoveEntity(entity);
                removedCount++;
            }
        }

        _sceneEntityIds.Remove(scene.Name);
        _logger.Debug(
            "Removed {Count} scene entities from '{SceneName}' (globals preserved)",
            removedCount,
            scene.Name
        );
    }
}

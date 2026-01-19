using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Managers;

/// <summary>
/// Core scene management with state machine and entity lifecycle.
/// Handles scene registration, transitions with fade effects, and entity lifecycle.
/// </summary>
public class SceneManager : BaseSystem, ISceneManager, IUpdateSystem
{
    private readonly Dictionary<string, IScene> _scenes = new();
    private readonly HashSet<string> _initializedScenes = new();
    private readonly HashSet<string> _globalsRegisteredScenes = new();
    private readonly Dictionary<string, List<IGameEntity>> _sceneGameEntities = new();

    private IScene? _currentScene;
    private IScene? _nextScene;
    private SceneTransitionState _transitionState = SceneTransitionState.Idle;
    private float _fadeTimer;
    private float _fadeDuration;

    /// <summary>
    /// Event fired when scene has been fully changed.
    /// </summary>
    public event ISceneManager.SceneChangedHandler? OnSceneChanged;

    /// <summary>
    /// Event fired when scene is loading (during Loading state).
    /// </summary>
    public event ISceneManager.SceneLoadingHandler? OnSceneLoading;

    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    public IScene CurrentScene
    {
        get => _currentScene ?? throw new InvalidOperationException("No scene is currently active.");
        private set => _currentScene = value;
    }

    /// <summary>
    /// Creates a new SceneManager with default priority 10 (early execution).
    /// </summary>
    public SceneManager(IGameEntityManager entityManager)
        : base("Scene Manager", 10, entityManager) { }

    /// <summary>
    /// Not used by SceneManager, but required by IUpdateSystem interface.
    /// </summary>
    public void FixedUpdate(GameTime gameTime) { }

    /// <summary>
    /// Gets the current fade alpha value for transition overlay.
    /// Returns 0 when not transitioning, 1 when fully faded.
    /// </summary>
    public float GetFadeAlpha()
    {
        if (_fadeDuration <= 0f)
        {
            return 0f;
        }

        return _transitionState switch
        {
            SceneTransitionState.FadingOut => Math.Clamp(_fadeTimer / _fadeDuration, 0f, 1f),
            SceneTransitionState.FadingIn  => Math.Clamp(1f - _fadeTimer / _fadeDuration, 0f, 1f),
            _                              => 0f
        };
    }

    /// <summary>
    /// Loads and returns a scene by name.
    /// </summary>
    public IScene LoadScene(string sceneName)
    {
        if (!_scenes.TryGetValue(sceneName, out var scene))
        {
            throw new KeyNotFoundException($"Scene '{sceneName}' is not registered.");
        }

        return scene;
    }

    /// <summary>
    /// Registers a scene in the manager and initializes it immediately.
    /// </summary>
    public void RegisterScene(IScene scene)
    {
        if (_scenes.ContainsKey(scene.Name))
        {
            throw new InvalidOperationException($"A scene with name '{scene.Name}' is already registered.");
        }

        _scenes.Add(scene.Name, scene);

        // Initialize the scene immediately
        if (!_initializedScenes.Contains(scene.Name))
        {
            scene.OnInitialize(this);
            _initializedScenes.Add(scene.Name);
        }
    }

    /// <summary>
    /// Immediately switches to a scene without transition.
    /// </summary>
    public void SetCurrentScene(IScene scene)
    {
        if (_currentScene == scene)
        {
            return;
        }

        _currentScene = scene;
        _transitionState = SceneTransitionState.Idle;
        _fadeTimer = 0f;
        _fadeDuration = 0f;
    }

    /// <summary>
    /// Switches to a scene with fade transition effect.
    /// </summary>
    public void SwitchScene(string sceneName, float fadeDuration = 1.0f)
    {
        var scene = LoadScene(sceneName);
        SwitchScene(scene, fadeDuration);
    }

    /// <summary>
    /// Switches to a scene with fade transition effect.
    /// </summary>
    public void SwitchScene(IScene scene, float fadeDuration = 1.0f)
    {
        if (_currentScene == scene && _transitionState == SceneTransitionState.Idle)
        {
            return;
        }

        _nextScene = scene;
        _fadeDuration = fadeDuration;
        _fadeTimer = 0f;

        if (fadeDuration <= 0f)
        {
            // No fade, switch immediately
            _transitionState = SceneTransitionState.Loading;
        }
        else
        {
            _transitionState = SceneTransitionState.FadingOut;
        }
    }

    /// <summary>
    /// Updates the scene transition state machine.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_transitionState == SceneTransitionState.Idle)
        {
            return;
        }

        _fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_transitionState)
        {
            case SceneTransitionState.FadingOut:
                if (_fadeTimer >= _fadeDuration)
                {
                    _fadeTimer = 0f;
                    _transitionState = SceneTransitionState.Loading;
                }

                break;

            case SceneTransitionState.Loading:
                PerformSceneTransition();
                _fadeTimer = 0f;

                if (_fadeDuration > 0f)
                {
                    _transitionState = SceneTransitionState.FadingIn;
                }
                else
                {
                    _transitionState = SceneTransitionState.Idle;
                }

                break;

            case SceneTransitionState.FadingIn:
                if (_fadeTimer >= _fadeDuration)
                {
                    _fadeTimer = 0f;
                    _transitionState = SceneTransitionState.Idle;
                }

                break;
        }
    }

    /// <summary>
    /// Performs the actual scene transition logic.
    /// Unloads old scene, loads new scene, and fires events.
    /// </summary>
    private void PerformSceneTransition()
    {
        if (_nextScene == null)
        {
            return;
        }

        var oldScene = _currentScene;

        // Unload old scene if exists
        if (_currentScene != null)
        {
            _currentScene.OnUnload();

            // Remove scene entities from entity manager
            if (_sceneGameEntities.TryGetValue(_currentScene.Name, out var entities))
            {
                foreach (var entity in entities)
                {
                    EntityManager.RemoveEntity(entity);
                }
                _sceneGameEntities.Remove(_currentScene.Name);
            }
        }

        // Set new scene
        _currentScene = _nextScene;
        _nextScene = null;

        // Register globals only once
        if (!_globalsRegisteredScenes.Contains(_currentScene.Name))
        {
            _currentScene.RegisterGlobals(EntityManager);
            _globalsRegisteredScenes.Add(_currentScene.Name);
        }

        // Load new scene
        _currentScene.OnLoad();

        // Add scene entities to entity manager
        var sceneEntities = new List<IGameEntity>();

        foreach (var entity in _currentScene.GetSceneGameEntities())
        {
            EntityManager.AddEntity(entity);
            sceneEntities.Add(entity);
        }

        if (sceneEntities.Count > 0)
        {
            _sceneGameEntities[_currentScene.Name] = sceneEntities;
        }

        // Fire events
        OnSceneLoading?.Invoke(_currentScene);
        OnSceneChanged?.Invoke(_currentScene);
    }
}

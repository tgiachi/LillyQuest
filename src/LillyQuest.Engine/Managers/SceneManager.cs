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
/// Supports both single-scene switching (SwitchScene) and stack-based layering (PushScene/PopScene).
/// </summary>
public class SceneManager : BaseSystem, ISceneManager, IUpdateSystem
{
    private readonly Dictionary<string, IScene> _scenes = new();
    private readonly HashSet<string> _initializedScenes = new();
    private readonly HashSet<string> _globalsRegisteredScenes = new();
    private readonly Dictionary<string, List<IGameEntity>> _sceneGameEntities = new();

    private Stack<IScene> _sceneStack = new();
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
    /// Gets the currently active scene (top of stack), or null if empty.
    /// </summary>
    public IScene? CurrentScene => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    /// <summary>
    /// Gets all scenes in the stack (immutable view).
    /// </summary>
    /// <remarks>
    /// Note: This property creates a new list on each access (O(n) allocation).
    /// <c>Stack&lt;T&gt;</c> does not have an <c>AsReadOnly()</c> method, so
    /// <c>ToList().AsReadOnly()</c> is required to satisfy the <c>IReadOnlyList&lt;IScene&gt;</c>
    /// interface contract. If the scene stack is accessed frequently, consider caching the result
    /// or redesigning the interface to accept <c>IReadOnlyCollection&lt;IScene&gt;</c>.
    /// </remarks>
    public IReadOnlyList<IScene> SceneStack => _sceneStack.ToList().AsReadOnly();

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
    /// Sets the current active scene (clears stack and pushes new scene).
    /// Used internally during initialization - does not call lifecycle methods.
    /// </summary>
    public void SetCurrentScene(IScene scene)
    {
        // Clear stack
        while (_sceneStack.Count > 0)
        {
            _sceneStack.Pop();
        }

        // Push new scene without calling lifecycle
        _sceneStack.Push(scene);
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
        if (_sceneStack.Count > 0 && _sceneStack.Peek() == scene && _transitionState == SceneTransitionState.Idle)
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
    /// Pushes a scene onto the stack without fade transition.
    /// </summary>
    public void PushScene(string sceneName)
    {
        var scene = LoadScene(sceneName);

        // Register globals only once (they persist across entire session)
        if (!_globalsRegisteredScenes.Contains(scene.Name))
        {
            scene.RegisterGlobals(EntityManager);
            _globalsRegisteredScenes.Add(scene.Name);
        }

        // Call OnLoad lifecycle method
        scene.OnLoad();

        // Add scene entities to entity manager
        var sceneEntities = new List<IGameEntity>();
        foreach (var entity in scene.GetSceneGameEntities())
        {
            EntityManager.AddEntity(entity);
            sceneEntities.Add(entity);
        }

        if (sceneEntities.Count > 0)
        {
            _sceneGameEntities[scene.Name] = sceneEntities;
        }

        // Push onto stack
        _sceneStack.Push(scene);

        // Fire events
        OnSceneLoading?.Invoke(scene);
        OnSceneChanged?.Invoke(scene);
    }

    /// <summary>
    /// Pops the top scene from the stack.
    /// </summary>
    public void PopScene()
    {
        if (_sceneStack.Count == 0)
        {
            throw new InvalidOperationException("Cannot pop from empty scene stack.");
        }

        var scene = _sceneStack.Pop();

        // Call OnUnload lifecycle method
        scene.OnUnload();

        // Remove scene entities from entity manager
        if (_sceneGameEntities.TryGetValue(scene.Name, out var entities))
        {
            foreach (var entity in entities)
            {
                EntityManager.RemoveEntity(entity);
            }
            _sceneGameEntities.Remove(scene.Name);
        }

        // Fire event with new top scene (or null if stack empty)
        var newTopScene = CurrentScene;
        if (newTopScene != null)
        {
            OnSceneChanged?.Invoke(newTopScene);
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
    /// Clears the stack, unloads old scene, loads new scene.
    /// </summary>
    private void PerformSceneTransition()
    {
        if (_nextScene == null)
        {
            return;
        }

        // Unload and remove all scenes currently in stack
        while (_sceneStack.Count > 0)
        {
            var oldScene = _sceneStack.Pop();
            oldScene.OnUnload();

            if (_sceneGameEntities.TryGetValue(oldScene.Name, out var entities))
            {
                foreach (var entity in entities)
                {
                    EntityManager.RemoveEntity(entity);
                }
                _sceneGameEntities.Remove(oldScene.Name);
            }
        }

        // Register globals only once
        if (!_globalsRegisteredScenes.Contains(_nextScene.Name))
        {
            _nextScene.RegisterGlobals(EntityManager);
            _globalsRegisteredScenes.Add(_nextScene.Name);
        }

        // Load new scene
        _nextScene.OnLoad();

        // Add scene entities to entity manager
        var sceneEntities = new List<IGameEntity>();
        foreach (var entity in _nextScene.GetSceneGameEntities())
        {
            EntityManager.AddEntity(entity);
            sceneEntities.Add(entity);
        }

        if (sceneEntities.Count > 0)
        {
            _sceneGameEntities[_nextScene.Name] = sceneEntities;
        }

        // Push new scene to now-empty stack
        _sceneStack.Push(_nextScene);
        _nextScene = null;

        // Fire events
        OnSceneLoading?.Invoke(CurrentScene!);
        OnSceneChanged?.Invoke(CurrentScene!);
    }
}

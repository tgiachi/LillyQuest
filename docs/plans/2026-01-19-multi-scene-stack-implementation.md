# Multi-Scene Stack Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** Implement a stack-based scene management system enabling overlaid scenes (game + menu) with background scene rendering and selective updating.

**Architecture:** SceneManager maintains a `Stack<IScene>` where only the top scene updates. All scenes render from bottom to top. SwitchScene clears the stack with fade, PushScene adds without transition, PopScene removes with state preservation.

**Tech Stack:** C# (.NET), xUnit tests, DryIoc DI container, Silk.NET rendering

---

## Task 1: Update ISceneManager Interface

**Files:**
- Modify: `src/LillyQuest.Engine/Interfaces/Managers/ISceneManager.cs`

**Step 1: Read interface and understand current API**

Read: `src/LillyQuest.Engine/Interfaces/Managers/ISceneManager.cs`

**Step 2: Add new interface members**

Update the interface to add three new methods and a property for stack-based operations:

```csharp
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Scenes;

namespace LillyQuest.Engine.Interfaces.Managers;

/// <summary>
/// Manages scene loading, switching, and change notifications.
/// Supports both single-scene switching with transitions and stack-based scene layering.
/// </summary>
public interface ISceneManager
{
    /// <summary>
    /// Handler invoked after a scene switch completes.
    /// </summary>
    delegate void SceneChangedHandler(IScene newScene);

    /// <summary>
    /// Handler invoked when a scene starts loading.
    /// </summary>
    delegate void SceneLoadingHandler(IScene scene);

    /// <summary>
    /// Raised after a scene switch completes.
    /// </summary>
    event SceneChangedHandler? OnSceneChanged;

    /// <summary>
    /// Raised when a scene starts loading.
    /// </summary>
    event SceneLoadingHandler? OnSceneLoading;

    /// <summary>
    /// Gets the current active scene (top of stack), or null if stack is empty.
    /// </summary>
    IScene? CurrentScene { get; }

    /// <summary>
    /// Gets all scenes currently in the stack (bottom to top).
    /// </summary>
    IReadOnlyList<IScene> SceneStack { get; }

    /// <summary>
    /// Returns the current fade alpha (0-1) for transitions.
    /// </summary>
    float GetFadeAlpha();

    /// <summary>
    /// Loads a scene by name without activating it.
    /// </summary>
    IScene LoadScene(string sceneName);

    /// <summary>
    /// Registers a scene with the scene manager.
    /// </summary>
    void RegisterScene(IScene scene);

    /// <summary>
    /// Sets the current active scene without any transition or stack manipulation.
    /// Used internally during scene initialization.
    /// </summary>
    void SetCurrentScene(IScene scene);

    /// <summary>
    /// Switches to another scene by name with fade transition.
    /// Clears the entire stack and pushes the new scene.
    /// </summary>
    void SwitchScene(string sceneName, float fadeDuration = 1.0f);

    /// <summary>
    /// Pushes a scene onto the stack without fade transition.
    /// The previous top scene becomes paused in background.
    /// </summary>
    void PushScene(string sceneName);

    /// <summary>
    /// Pops the top scene from the stack.
    /// The scene below becomes active and resumes from its frozen state.
    /// Throws InvalidOperationException if stack is empty.
    /// </summary>
    void PopScene();

    /// <summary>
    /// Updates the scene transition state machine.
    /// </summary>
    void Update(GameTime gameTime);
}
```

**Step 3: Commit**

```bash
git add src/LillyQuest.Engine/Interfaces/Managers/ISceneManager.cs
git commit -m "refactor(ISceneManager): add stack-based operations (PushScene, PopScene, SceneStack)"
```

---

## Task 2: Refactor SceneManager to Use Stack

**Files:**
- Modify: `src/LillyQuest.Engine/Managers/SceneManager.cs`

**Step 1: Update SceneManager class declaration and fields**

Replace the single-scene fields with stack-based fields. Read the current file first, then replace the class body up to the first method:

Current state (lines 1-46):
```csharp
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
    public IReadOnlyList<IScene> SceneStack => _sceneStack.AsReadOnly();
```

**Step 2: Commit**

```bash
git add src/LillyQuest.Engine/Managers/SceneManager.cs
git commit -m "refactor(SceneManager): replace single-scene with Stack<IScene> for layered scenes"
```

---

## Task 3: Implement PushScene Method

**Files:**
- Modify: `src/LillyQuest.Engine/Managers/SceneManager.cs`

**Step 1: Add PushScene method after existing methods**

Add this method before the `SwitchScene` implementation:

```csharp
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
```

**Step 2: Commit**

```bash
git add src/LillyQuest.Engine/Managers/SceneManager.cs
git commit -m "feat(SceneManager): implement PushScene for stack-based scene overlaying"
```

---

## Task 4: Implement PopScene Method

**Files:**
- Modify: `src/LillyQuest.Engine/Managers/SceneManager.cs`

**Step 1: Add PopScene method**

Add this method after PushScene:

```csharp
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
```

**Step 2: Commit**

```bash
git add src/LillyQuest.Engine/Managers/SceneManager.cs
git commit -m "feat(SceneManager): implement PopScene for scene removal with state preservation"
```

---

## Task 5: Refactor SwitchScene to Use Stack

**Files:**
- Modify: `src/LillyQuest.Engine/Managers/SceneManager.cs`

**Step 1: Update SwitchScene(string, float) overload**

Replace the SwitchScene method that takes a string sceneName:

```csharp
/// <summary>
/// Switches to a scene with fade transition effect.
/// Clears the entire stack, applies fade, then pushes the new scene.
/// </summary>
public void SwitchScene(string sceneName, float fadeDuration = 1.0f)
{
    var scene = LoadScene(sceneName);
    SwitchScene(scene, fadeDuration);
}
```

**Step 2: Update SwitchScene(IScene, float) overload**

Replace the SwitchScene method that takes an IScene:

```csharp
/// <summary>
/// Switches to a scene with fade transition effect.
/// Clears the entire stack, applies fade, then pushes the new scene.
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
        _transitionState = SceneTransitionState.Loading;
    }
    else
    {
        _transitionState = SceneTransitionState.FadingOut;
    }
}
```

**Step 3: Update PerformSceneTransition to clear stack**

Replace the PerformSceneTransition method:

```csharp
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
```

**Step 4: Commit**

```bash
git add src/LillyQuest.Engine/Managers/SceneManager.cs
git commit -m "refactor(SceneManager): adapt SwitchScene to clear stack before transition"
```

---

## Task 6: Update SetCurrentScene for Stack

**Files:**
- Modify: `src/LillyQuest.Engine/Managers/SceneManager.cs`

**Step 1: Update SetCurrentScene method**

This method is used internally during initialization. Update it to work with stack:

```csharp
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
```

**Step 2: Commit**

```bash
git add src/LillyQuest.Engine/Managers/SceneManager.cs
git commit -m "refactor(SceneManager): update SetCurrentScene to clear stack safely"
```

---

## Task 7: Create SceneManager Unit Tests

**Files:**
- Create: `tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs`

**Step 1: Create test file with basic structure**

```csharp
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Managers;
using Moq;
using Xunit;

namespace LillyQuest.Tests.Engine.Managers;

public class SceneManagerStackTests
{
    private readonly Mock<IGameEntityManager> _entityManagerMock;
    private readonly Mock<IScene> _gameSceneMock;
    private readonly Mock<IScene> _menuSceneMock;
    private readonly SceneManager _sceneManager;

    public SceneManagerStackTests()
    {
        _entityManagerMock = new();
        _gameSceneMock = new();
        _menuSceneMock = new();

        _gameSceneMock.SetupGet(s => s.Name).Returns("GameScene");
        _menuSceneMock.SetupGet(s => s.Name).Returns("MenuScene");

        _gameSceneMock.Setup(s => s.GetSceneGameEntities()).Returns(new List<IGameEntity>());
        _menuSceneMock.Setup(s => s.GetSceneGameEntities()).Returns(new List<IGameEntity>());

        _sceneManager = new(_entityManagerMock.Object);

        // Register scenes
        _sceneManager.RegisterScene(_gameSceneMock.Object);
        _sceneManager.RegisterScene(_menuSceneMock.Object);
    }

    [Fact]
    public void PushScene_AddsSceneToStack()
    {
        // Act
        _sceneManager.PushScene("GameScene");

        // Assert
        Assert.Single(_sceneManager.SceneStack);
        Assert.Equal(_gameSceneMock.Object, _sceneManager.CurrentScene);
    }

    [Fact]
    public void PushScene_CallsOnLoadAndRegisterGlobals()
    {
        // Act
        _sceneManager.PushScene("GameScene");

        // Assert
        _gameSceneMock.Verify(s => s.RegisterGlobals(_entityManagerMock.Object), Times.Once);
        _gameSceneMock.Verify(s => s.OnLoad(), Times.Once);
    }

    [Fact]
    public void PushScene_Multiple_CreatesStack()
    {
        // Act
        _sceneManager.PushScene("GameScene");
        _sceneManager.PushScene("MenuScene");

        // Assert
        Assert.Equal(2, _sceneManager.SceneStack.Count);
        Assert.Equal(_menuSceneMock.Object, _sceneManager.CurrentScene); // Top of stack
    }

    [Fact]
    public void PopScene_RemovesTopScene()
    {
        // Arrange
        _sceneManager.PushScene("GameScene");
        _sceneManager.PushScene("MenuScene");

        // Act
        _sceneManager.PopScene();

        // Assert
        Assert.Single(_sceneManager.SceneStack);
        Assert.Equal(_gameSceneMock.Object, _sceneManager.CurrentScene);
    }

    [Fact]
    public void PopScene_CallsOnUnload()
    {
        // Arrange
        _sceneManager.PushScene("GameScene");
        _sceneManager.PushScene("MenuScene");

        // Act
        _sceneManager.PopScene();

        // Assert
        _menuSceneMock.Verify(s => s.OnUnload(), Times.Once);
    }

    [Fact]
    public void PopScene_EmptyStack_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sceneManager.PopScene());
    }

    [Fact]
    public void CurrentScene_EmptyStack_ReturnsNull()
    {
        // Assert
        Assert.Null(_sceneManager.CurrentScene);
    }

    [Fact]
    public void SceneStack_ReturnsImmutableList()
    {
        // Arrange
        _sceneManager.PushScene("GameScene");

        // Act
        var stack = _sceneManager.SceneStack;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<IScene>>(stack);
        Assert.Single(stack);
    }
}
```

**Step 2: Run tests to verify they pass**

```bash
cd .worktrees/feature/multi-scene-stack
dotnet test tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs -v
```

Expected: All tests pass (6 passing)

**Step 3: Commit**

```bash
git add tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs
git commit -m "test(SceneManager): add unit tests for PushScene, PopScene, and stack operations"
```

---

## Task 8: Update RenderSystem for Multi-Scene Rendering

**Files:**
- Modify: `src/LillyQuest.Engine/Systems/RenderSystem.cs`
- Modify: `src/LillyQuest.Engine/Interfaces/Managers/ISceneManager.cs` (already done, just using it now)

**Step 1: Inject ISceneManager into RenderSystem**

Add ISceneManager as a dependency. Current constructor (lines 35-46):

```csharp
private readonly IShaderManager _shaderManager;
private readonly IFontManager _fontManager;
private readonly ITextureManager _textureManager;
private readonly ISceneManager _sceneManager;  // ADD THIS

private SpriteBatch? _spriteBatch;

public SpriteBatch SpriteBatch
{
    get => _spriteBatch ?? throw new InvalidOperationException("SpriteBatch not initialized. Call Initialize() first.");
    private set => _spriteBatch = value;
}

public RenderSystem(
    IGameEntityManager entityManager,
    IShaderManager shaderManager,
    IFontManager fontManager,
    ITextureManager textureManager,
    ISceneManager sceneManager  // ADD THIS PARAMETER
)
    : base("Render System", 100, entityManager)
{
    _shaderManager = shaderManager;
    _fontManager = fontManager;
    _textureManager = textureManager;
    _sceneManager = sceneManager;  // ADD THIS ASSIGNMENT
}
```

**Step 2: Update Render method to render all scenes in stack**

Replace the Render method (lines 51-75):

```csharp
/// <summary>
/// Renders all scenes in stack from bottom to top, then all global features.
/// </summary>
public void Render(GameTime gameTime)
{
    SpriteBatch.Begin();

    // Render all scenes in stack from bottom to top
    if (_sceneManager.SceneStack.Count > 0)
    {
        var scenesInOrder = _sceneManager.SceneStack.Reverse().ToList();

        foreach (var scene in scenesInOrder)
        {
            var sceneEntities = scene.GetSceneGameEntities();
            var sceneFeatures = sceneEntities
                .SelectMany(e => e.GetAllFeatures().OfType<IRenderFeature>())
                .Where(f => f.IsEnabled)
                .OrderBy(f => f.RenderOrder)
                .ToList();

            foreach (var feature in sceneFeatures)
            {
                feature.Render(SpriteBatch, gameTime);
            }
        }
    }

    // Render all global render features
    var globalFeatures = EntityManager.QueryOfType<IRenderFeature>()
                                      .Where(f => f.IsEnabled)
                                      .OrderBy(f => f.RenderOrder)
                                      .ToList();

    foreach (var feature in globalFeatures)
    {
        feature.Render(SpriteBatch, gameTime);
    }

    SpriteBatch.End();
}
```

**Step 3: Update DI registration**

Find where RenderSystem is registered in the DI container and ensure ISceneManager is injected. Usually in bootstrap or DI setup file. Update the registration to include _sceneManager parameter.

**Step 4: Commit**

```bash
git add src/LillyQuest.Engine/Systems/RenderSystem.cs
git commit -m "refactor(RenderSystem): render all scenes in stack from bottom to top"
```

---

## Task 9: Update UpdateSystem for Single-Scene Updates

**Files:**
- Modify: `src/LillyQuest.Engine/Systems/UpdateSystem.cs`

**Step 1: Inject ISceneManager**

Add ISceneManager as a dependency:

```csharp
public class UpdateSystem : BaseSystem, IUpdateSystem
{
    private readonly ISceneManager _sceneManager;

    public UpdateSystem(IGameEntityManager entityManager, ISceneManager sceneManager)
        : base("Update System", 0, entityManager)
    {
        _sceneManager = sceneManager;
    }
```

**Step 2: Update Update method to only update top scene**

Replace the Update method:

```csharp
public void Update(GameTime gameTime)
{
    var currentScene = _sceneManager.CurrentScene;
    if (currentScene == null)
    {
        return;
    }

    // Only update entities from the current (top) scene
    var sceneEntities = currentScene.GetSceneGameEntities();
    foreach (var entity in sceneEntities)
    {
        if (entity.TryGetFeature<IUpdateFeature>(out var updateFeature))
        {
            updateFeature.Update(gameTime);
        }
    }
}
```

**Step 3: Update FixedUpdate method to only update top scene**

Replace the FixedUpdate method:

```csharp
public void FixedUpdate(GameTime gameTime)
{
    var currentScene = _sceneManager.CurrentScene;
    if (currentScene == null)
    {
        return;
    }

    // Only update entities from the current (top) scene
    var sceneEntities = currentScene.GetSceneGameEntities();
    foreach (var entity in sceneEntities)
    {
        if (entity.TryGetFeature<IFixedUpdateFeature>(out var fixedUpdateFeature))
        {
            fixedUpdateFeature.FixedUpdate(gameTime);
        }
    }
}
```

**Step 4: Update DI registration**

Ensure UpdateSystem gets ISceneManager injected in the DI container setup.

**Step 5: Commit**

```bash
git add src/LillyQuest.Engine/Systems/UpdateSystem.cs
git commit -m "refactor(UpdateSystem): only update current (top) scene in stack"
```

---

## Task 10: Verify Build and Run Integration Tests

**Files:**
- Test: `tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs`

**Step 1: Build the project**

```bash
cd .worktrees/feature/multi-scene-stack
dotnet build -c Debug
```

Expected: Build successful with 0 errors

**Step 2: Run all tests**

```bash
dotnet test tests/LillyQuest.Tests/ -v
```

Expected: All tests pass

**Step 3: Verify no compile warnings related to unused methods**

Check that the old CurrentScene property (which threw when null) is not causing issues. If it's still in use elsewhere, we may need to create an adapter or carefully transition.

**Step 4: Commit**

```bash
git add .
git commit -m "verify: all tests passing, build successful"
```

---

## Task 11: Integration Test - PushScene and PopScene Flow

**Files:**
- Modify: `tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs`

**Step 1: Add integration test for ESC menu flow**

Add to SceneManagerStackTests class:

```csharp
[Fact]
public void IntegrationTest_GameMenuFlow()
{
    // Initial state: push game scene
    _sceneManager.PushScene("GameScene");
    Assert.Single(_sceneManager.SceneStack);

    // Player presses ESC: push menu
    _sceneManager.PushScene("MenuScene");
    Assert.Equal(2, _sceneManager.SceneStack.Count);
    Assert.Equal(_menuSceneMock.Object, _sceneManager.CurrentScene);

    // Verify game scene OnLoad called but not during menu push
    _gameSceneMock.Verify(s => s.OnLoad(), Times.Once);
    _menuSceneMock.Verify(s => s.OnLoad(), Times.Once);

    // Player closes menu: pop
    _sceneManager.PopScene();
    Assert.Single(_sceneManager.SceneStack);
    Assert.Equal(_gameSceneMock.Object, _sceneManager.CurrentScene);

    // Verify menu unload called
    _menuSceneMock.Verify(s => s.OnUnload(), Times.Once);
}

[Fact]
public void IntegrationTest_RegisterGlobalsOnlyOnce()
{
    // Push same scene twice (nested)
    _sceneManager.PushScene("GameScene");
    _sceneManager.PushScene("GameScene");

    // RegisterGlobals should only be called once for the scene type
    _gameSceneMock.Verify(s => s.RegisterGlobals(It.IsAny<IGameEntityManager>()), Times.Once);
}
```

**Step 2: Run tests**

```bash
dotnet test tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs::SceneManagerStackTests::IntegrationTest_GameMenuFlow -v
dotnet test tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs::SceneManagerStackTests::IntegrationTest_RegisterGlobalsOnlyOnce -v
```

Expected: Both tests pass

**Step 3: Commit**

```bash
git add tests/LillyQuest.Tests/Engine/Managers/SceneManagerStackTests.cs
git commit -m "test(SceneManager): add integration tests for game menu flow and global registration"
```

---

## Task 12: Test RenderSystem Multi-Scene Rendering

**Files:**
- Create: `tests/LillyQuest.Tests/Engine/Systems/RenderSystemMultiSceneTests.cs`

**Step 1: Create test file**

```csharp
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Systems;
using LillyQuest.Core.Primitives;
using Moq;
using Xunit;

namespace LillyQuest.Tests.Engine.Systems;

public class RenderSystemMultiSceneTests
{
    [Fact]
    public void Render_MultipleScenes_RendersInOrder()
    {
        // Arrange
        var entityManagerMock = new Mock<IGameEntityManager>();
        var sceneManagerMock = new Mock<ISceneManager>();
        var shaderManagerMock = new Mock<IShaderManager>();
        var fontManagerMock = new Mock<IFontManager>();
        var textureManagerMock = new Mock<ITextureManager>();

        var gameScene = new Mock<IScene>();
        var menuScene = new Mock<IScene>();

        var renderOrder = new List<int>();

        var gameRenderFeature = new Mock<IRenderFeature>();
        gameRenderFeature.SetupGet(f => f.IsEnabled).Returns(true);
        gameRenderFeature.SetupGet(f => f.RenderOrder).Returns(0);
        gameRenderFeature.Setup(f => f.Render(It.IsAny<object>(), It.IsAny<GameTime>()))
            .Callback(() => renderOrder.Add(0));

        var menuRenderFeature = new Mock<IRenderFeature>();
        menuRenderFeature.SetupGet(f => f.IsEnabled).Returns(true);
        menuRenderFeature.SetupGet(f => f.RenderOrder).Returns(0);
        menuRenderFeature.Setup(f => f.Render(It.IsAny<object>(), It.IsAny<GameTime>()))
            .Callback(() => renderOrder.Add(1));

        var gameEntity = new Mock<IGameEntity>();
        gameEntity.Setup(e => e.GetAllFeatures()).Returns(new[] { gameRenderFeature.Object });

        var menuEntity = new Mock<IGameEntity>();
        menuEntity.Setup(e => e.GetAllFeatures()).Returns(new[] { menuRenderFeature.Object });

        gameScene.Setup(s => s.GetSceneGameEntities()).Returns(new[] { gameEntity.Object });
        menuScene.Setup(s => s.GetSceneGameEntities()).Returns(new[] { menuEntity.Object });

        sceneManagerMock.SetupGet(sm => sm.SceneStack).Returns(new[] { gameScene.Object, menuScene.Object });
        entityManagerMock.Setup(em => em.QueryOfType<IRenderFeature>()).Returns(new List<IRenderFeature>());

        var renderSystem = new RenderSystem(entityManagerMock.Object, shaderManagerMock.Object,
                                          fontManagerMock.Object, textureManagerMock.Object, sceneManagerMock.Object);

        // Act
        renderSystem.Render(new(new TimeSpan(0), new TimeSpan(0)));

        // Assert - game scene (0) should render before menu scene (1)
        Assert.Equal(new[] { 0, 1 }, renderOrder);
    }

    [Fact]
    public void Render_EmptyStack_NoErrors()
    {
        // Arrange
        var entityManagerMock = new Mock<IGameEntityManager>();
        var sceneManagerMock = new Mock<ISceneManager>();
        var shaderManagerMock = new Mock<IShaderManager>();
        var fontManagerMock = new Mock<IFontManager>();
        var textureManagerMock = new Mock<ITextureManager>();

        sceneManagerMock.SetupGet(sm => sm.SceneStack).Returns(new List<IScene>());
        entityManagerMock.Setup(em => em.QueryOfType<IRenderFeature>()).Returns(new List<IRenderFeature>());

        var renderSystem = new RenderSystem(entityManagerMock.Object, shaderManagerMock.Object,
                                          fontManagerMock.Object, textureManagerMock.Object, sceneManagerMock.Object);

        // Act & Assert - should not throw
        renderSystem.Render(new(new TimeSpan(0), new TimeSpan(0)));
    }
}
```

**Step 2: Run tests**

```bash
dotnet test tests/LillyQuest.Tests/Engine/Systems/RenderSystemMultiSceneTests.cs -v
```

Expected: Both tests pass

**Step 3: Commit**

```bash
git add tests/LillyQuest.Tests/Engine/Systems/RenderSystemMultiSceneTests.cs
git commit -m "test(RenderSystem): add tests for multi-scene rendering order"
```

---

## Task 13: Test UpdateSystem Single-Scene Updates

**Files:**
- Create: `tests/LillyQuest.Tests/Engine/Systems/UpdateSystemStackTests.cs`

**Step 1: Create test file**

```csharp
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Systems;
using LillyQuest.Core.Primitives;
using Moq;
using Xunit;

namespace LillyQuest.Tests.Engine.Systems;

public class UpdateSystemStackTests
{
    [Fact]
    public void Update_TopSceneOnly_UpdatesOnlyTopScene()
    {
        // Arrange
        var entityManagerMock = new Mock<IGameEntityManager>();
        var sceneManagerMock = new Mock<ISceneManager>();

        var gameScene = new Mock<IScene>();
        var menuScene = new Mock<IScene>();

        var gameUpdateFeature = new Mock<IUpdateFeature>();
        var menuUpdateFeature = new Mock<IUpdateFeature>();

        var gameEntity = new Mock<IGameEntity>();
        gameEntity.Setup(e => e.TryGetFeature<IUpdateFeature>(out gameUpdateFeature.Object)).Returns(true);

        var menuEntity = new Mock<IGameEntity>();
        menuEntity.Setup(e => e.TryGetFeature<IUpdateFeature>(out menuUpdateFeature.Object)).Returns(true);

        gameScene.Setup(s => s.GetSceneGameEntities()).Returns(new[] { gameEntity.Object });
        menuScene.Setup(s => s.GetSceneGameEntities()).Returns(new[] { menuEntity.Object });

        // Menu scene is on top
        sceneManagerMock.SetupGet(sm => sm.CurrentScene).Returns(menuScene.Object);

        var updateSystem = new UpdateSystem(entityManagerMock.Object, sceneManagerMock.Object);

        // Act
        updateSystem.Update(new(new TimeSpan(0), new TimeSpan(0, 0, 0, 0, 16))); // 16ms frame

        // Assert - only menu scene entities updated
        menuUpdateFeature.Verify(f => f.Update(It.IsAny<GameTime>()), Times.Once);
        gameUpdateFeature.Verify(f => f.Update(It.IsAny<GameTime>()), Times.Never);
    }

    [Fact]
    public void Update_NullCurrentScene_NoErrors()
    {
        // Arrange
        var entityManagerMock = new Mock<IGameEntityManager>();
        var sceneManagerMock = new Mock<ISceneManager>();

        sceneManagerMock.SetupGet(sm => sm.CurrentScene).Returns((IScene?)null);

        var updateSystem = new UpdateSystem(entityManagerMock.Object, sceneManagerMock.Object);

        // Act & Assert - should not throw
        updateSystem.Update(new(new TimeSpan(0), new TimeSpan(0)));
    }
}
```

**Step 2: Run tests**

```bash
dotnet test tests/LillyQuest.Tests/Engine/Systems/UpdateSystemStackTests.cs -v
```

Expected: Both tests pass

**Step 3: Commit**

```bash
git add tests/LillyQuest.Tests/Engine/Systems/UpdateSystemStackTests.cs
git commit -m "test(UpdateSystem): add tests to verify only top scene updates"
```

---

## Task 14: Final Build and All Tests

**Files:**
- Build & test: entire project

**Step 1: Clean build**

```bash
cd .worktrees/feature/multi-scene-stack
dotnet clean
dotnet build -c Debug
```

Expected: Build successful, 0 errors, ~3 warnings max

**Step 2: Run all tests**

```bash
dotnet test tests/LillyQuest.Tests/ -v --logger:"console;verbosity=minimal"
```

Expected: All tests pass (at least 10+)

**Step 3: Verify no regressions**

```bash
dotnet test tests/LillyQuest.Tests/ --filter "Category!=Integration" -v
```

Expected: All unit tests pass

**Step 4: Final commit**

```bash
git add .
git commit -m "refactor: multi-scene stack implementation complete and tested"
```

---

## Validation Checklist

Before marking implementation complete:

- [ ] SceneManager uses `Stack<IScene>` internally
- [ ] PushScene adds scene to stack without transition
- [ ] PopScene removes scene with OnUnload called
- [ ] SwitchScene clears stack and applies fade transition
- [ ] RenderSystem renders all scenes bottom-to-top
- [ ] UpdateSystem only updates top scene
- [ ] All unit tests pass (SceneManager, RenderSystem, UpdateSystem)
- [ ] No compilation errors or warnings
- [ ] DI container properly injects all dependencies
- [ ] State preservation: popped scene resumes exactly when pushed back
- [ ] GlobalEntity registration only happens once per scene


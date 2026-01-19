using System.Numerics;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Features;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.GameObjects.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Systems;
using NUnit.Framework;

namespace LillyQuest.Tests.Engine.Systems;

public class InputFocusSystemTests
{
    /// <summary>
    /// Test implementation of IScene for testing.
    /// </summary>
    private sealed class TestScene : IScene
    {
        private readonly List<IGameEntity> _entities = new();

        public string Name { get; set; } = "TestScene";

        public void AddEntity(IGameEntity entity)
        {
            _entities.Add(entity);
        }

        public void OnInitialize(ISceneManager sceneManager) { }
        public void OnLoad() { }
        public void OnUnload() { }
        public void RegisterGlobals(IGameEntityManager entityManager) { }
        public IEnumerable<IGameEntity> GetSceneGameEntities() => _entities;
    }

    /// <summary>
    /// Test implementation of ISceneManager for testing.
    /// </summary>
    private sealed class TestSceneManager : ISceneManager
    {
        public IScene? CurrentScene { get; set; }
        public IReadOnlyList<IScene> SceneStack => Array.Empty<IScene>();

        public event ISceneManager.SceneChangedHandler? OnSceneChanged;
        public event ISceneManager.SceneLoadingHandler? OnSceneLoading;

        public float GetFadeAlpha() => 0f;
        public IScene LoadScene(string sceneName) => null!;
        public void RegisterScene(IScene scene) { }
        public void SetCurrentScene(IScene scene) { }
        public void SwitchScene(string sceneName, float fadeDuration = 1.0f) { }
        public void PushScene(string sceneName) { }
        public void PopScene() { }
        public void Update(Core.Primitives.GameTime gameTime) { }
    }

    [Test]
    public void HandleMouseClick_OnScreen_SetsFocus()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();
        var scene = new TestScene();

        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200),
            IsVisible = true,
            Order = 10
        };

        scene.AddEntity(screen);
        sceneManager.CurrentScene = scene;

        var system = new InputFocusSystem(entityManager, sceneManager);

        // Act
        system.HandleMouseClick(150, 150); // Inside screen

        // Assert
        Assert.That(system.FocusedScreen, Is.EqualTo(screen));
    }

    [Test]
    public void HandleMouseClick_OutsideScreen_ClearsFocus()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();
        var scene = new TestScene();

        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200),
            IsVisible = true,
            Order = 10
        };

        scene.AddEntity(screen);
        sceneManager.CurrentScene = scene;

        var system = new InputFocusSystem(entityManager, sceneManager);

        // Set initial focus
        system.HandleMouseClick(150, 150);
        Assert.That(system.FocusedScreen, Is.Not.Null);

        // Act - click outside
        system.HandleMouseClick(50, 50);

        // Assert
        Assert.That(system.FocusedScreen, Is.Null);
    }

    [Test]
    public void HandleMouseClick_OverlappingScreens_FocusesTopMost()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();
        var scene = new TestScene();

        var screen1 = new Screen(1, "Screen1")
        {
            Position = new Vector2(50, 50),
            Size = new Vector2(300, 300),
            IsVisible = true,
            Order = 5  // Lower order - behind
        };

        var screen2 = new Screen(2, "Screen2")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200),
            IsVisible = true,
            Order = 10  // Higher order - on top
        };

        scene.AddEntity(screen1);
        scene.AddEntity(screen2);
        sceneManager.CurrentScene = scene;

        var system = new InputFocusSystem(entityManager, sceneManager);

        // Act - click in overlapping area
        system.HandleMouseClick(150, 150);

        // Assert - should focus top-most (higher Order)
        Assert.That(system.FocusedScreen, Is.EqualTo(screen2));
    }

    [Test]
    public void HandleMouseClick_InvisibleScreen_Ignored()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();
        var scene = new TestScene();

        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200),
            IsVisible = false,  // Invisible
            Order = 10
        };

        scene.AddEntity(screen);
        sceneManager.CurrentScene = scene;

        var system = new InputFocusSystem(entityManager, sceneManager);

        // Act - click on invisible screen
        system.HandleMouseClick(150, 150);

        // Assert - should not focus invisible screen
        Assert.That(system.FocusedScreen, Is.Null);
    }

    [Test]
    public void DispatchMouseInput_WithFocus_InvokesAction()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();

        var screen = new Screen(1, "TestScreen")
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(200, 200),
            IsVisible = true
        };

        var inputFeature = new ScreenInputFeature(screen);
        screen.AddFeature(inputFeature);

        var system = new InputFocusSystem(entityManager, sceneManager);
        system.SetFocus(screen);

        bool actionInvoked = false;

        // Act
        system.DispatchMouseInput(150, 150, feature => actionInvoked = true);

        // Assert
        Assert.That(actionInvoked, Is.True);
    }

    [Test]
    public void DispatchMouseInput_WithoutFocus_DoesNotInvokeAction()
    {
        // Arrange
        var entityManager = new GameEntityManager();
        var sceneManager = new TestSceneManager();

        var system = new InputFocusSystem(entityManager, sceneManager);

        bool actionInvoked = false;

        // Act
        system.DispatchMouseInput(150, 150, feature => actionInvoked = true);

        // Assert
        Assert.That(actionInvoked, Is.False);
    }
}

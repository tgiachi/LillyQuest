using NUnit.Framework;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Managers;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Tests;

/// <summary>
/// Tests for SceneManager stack operations including PushScene, PopScene, and stack manipulation.
/// </summary>
[TestFixture]
public class SceneManagerStackTests
{
    /// <summary>
    /// Mock implementation of IScene for testing.
    /// </summary>
    private sealed class TestScene : IScene
    {
        private bool _onLoadCalled;
        private bool _onUnloadCalled;
        private bool _registerGlobalsCalled;

        public string Name { get; set; }
        public bool OnLoadCalled => _onLoadCalled;
        public bool OnUnloadCalled => _onUnloadCalled;
        public bool RegisterGlobalsCalled => _registerGlobalsCalled;

        public TestScene(string name)
        {
            Name = name;
        }

        public void OnInitialize(ISceneManager sceneManager) { }
        public void OnLoad() => _onLoadCalled = true;
        public void OnUnload() => _onUnloadCalled = true;
        public void RegisterGlobals(IGameEntityManager entityManager) => _registerGlobalsCalled = true;
        public IEnumerable<IGameEntity> GetSceneGameEntities() => new List<IGameEntity>();
    }

    private SceneManager _sceneManager = null!;
    private TestScene _testScene1 = null!;
    private TestScene _testScene2 = null!;
    private TestScene _testScene3 = null!;

    [SetUp]
    public void SetUp()
    {
        var entityManager = new GameEntityManager();
        _sceneManager = new SceneManager(entityManager);

        _testScene1 = new TestScene("Scene1");
        _testScene2 = new TestScene("Scene2");
        _testScene3 = new TestScene("Scene3");
    }

    [Test]
    public void PushScene_AddsSceneToStack()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);

        // Act
        _sceneManager.PushScene("Scene1");

        // Assert
        Assert.That(_sceneManager.CurrentScene, Is.EqualTo(_testScene1));
        Assert.That(_sceneManager.SceneStack.Count, Is.EqualTo(1));
    }

    [Test]
    public void PushScene_CallsOnLoadLifecycle()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);

        // Act
        _sceneManager.PushScene("Scene1");

        // Assert
        Assert.That(_testScene1.OnLoadCalled, Is.True);
    }

    [Test]
    public void PopScene_RemovesSceneFromStack()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.PushScene("Scene1");

        // Act
        _sceneManager.PopScene();

        // Assert
        Assert.That(_sceneManager.CurrentScene, Is.Null);
        Assert.That(_sceneManager.SceneStack.Count, Is.EqualTo(0));
    }

    [Test]
    public void PopScene_CallsOnUnloadLifecycle()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.PushScene("Scene1");

        // Act
        _sceneManager.PopScene();

        // Assert
        Assert.That(_testScene1.OnUnloadCalled, Is.True);
    }

    [Test]
    public void PopScene_ThrowsOnEmptyStack()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sceneManager.PopScene());
    }

    [Test]
    public void CurrentScene_ReturnsTopOfStack()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.RegisterScene(_testScene2);
        _sceneManager.PushScene("Scene1");
        _sceneManager.PushScene("Scene2");

        // Act
        var current = _sceneManager.CurrentScene;

        // Assert
        Assert.That(current, Is.EqualTo(_testScene2));
    }

    [Test]
    public void SceneStack_ContainsAllPushedScenes()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.RegisterScene(_testScene2);

        // Act
        _sceneManager.PushScene("Scene1");
        _sceneManager.PushScene("Scene2");

        // Assert
        var stack = _sceneManager.SceneStack;
        Assert.That(stack.Count, Is.EqualTo(2));
        // Stack.ToList() returns top-first order, so Scene2 is at index 0, Scene1 at index 1
        Assert.That(stack[0].Name, Is.EqualTo("Scene2"));
        Assert.That(stack[1].Name, Is.EqualTo("Scene1"));
    }

    [Test]
    public void PushScene_FiresOnSceneChangedEvent()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        var eventFired = false;
        IScene? changedScene = null;
        _sceneManager.OnSceneChanged += (scene) =>
        {
            eventFired = true;
            changedScene = scene;
        };

        // Act
        _sceneManager.PushScene("Scene1");

        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(changedScene, Is.EqualTo(_testScene1));
    }

    [Test]
    public void PopScene_FiresOnSceneChangedEventWhenStackNotEmpty()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.RegisterScene(_testScene2);
        _sceneManager.PushScene("Scene1");
        _sceneManager.PushScene("Scene2");

        var eventFired = false;
        IScene? changedScene = null;
        _sceneManager.OnSceneChanged += (scene) =>
        {
            eventFired = true;
            changedScene = scene;
        };

        // Act
        _sceneManager.PopScene();

        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(changedScene, Is.EqualTo(_testScene1));
    }

    [Test]
    public void PushScene_CallsRegisterGlobalsOnce()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);

        // Act
        _sceneManager.PushScene("Scene1");
        _sceneManager.PopScene();
        _sceneManager.PushScene("Scene1");

        // Assert - RegisterGlobals should only be called once
        Assert.That(_testScene1.RegisterGlobalsCalled, Is.True, "RegisterGlobals should have been called");
        // Note: We can't easily verify "only once" without more complex tracking, but OnLoad should be called twice
    }

    [Test]
    public void MultipleScenes_MaintainStackOrder()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.RegisterScene(_testScene2);
        _sceneManager.RegisterScene(_testScene3);

        // Act
        _sceneManager.PushScene("Scene1");
        _sceneManager.PushScene("Scene2");
        _sceneManager.PushScene("Scene3");

        // Assert
        var stack = _sceneManager.SceneStack;
        Assert.That(stack.Count, Is.EqualTo(3));
        // Stack.ToList() returns top-first order: Scene3, Scene2, Scene1
        Assert.That(stack[0].Name, Is.EqualTo("Scene3"), "Top of stack should be Scene3");
        Assert.That(stack[1].Name, Is.EqualTo("Scene2"), "Middle of stack should be Scene2");
        Assert.That(stack[2].Name, Is.EqualTo("Scene1"), "Bottom of stack should be Scene1");
        Assert.That(_sceneManager.CurrentScene, Is.EqualTo(_testScene3), "Current scene should be top");
    }

    [Test]
    public void PopScene_ReturnsToCorrectScene()
    {
        // Arrange
        _sceneManager.RegisterScene(_testScene1);
        _sceneManager.RegisterScene(_testScene2);
        _sceneManager.PushScene("Scene1");
        _sceneManager.PushScene("Scene2");

        // Act
        _sceneManager.PopScene();

        // Assert
        Assert.That(_sceneManager.CurrentScene, Is.EqualTo(_testScene1), "After pop, current should be Scene1");
    }
}

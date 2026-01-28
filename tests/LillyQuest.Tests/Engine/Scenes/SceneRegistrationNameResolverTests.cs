using DryIoc;
using LillyQuest.Core.Internal.Data.Registrations;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Scenes;
using LillyQuest.Engine.Managers.Scenes;

namespace LillyQuest.Tests.Engine.Scenes;

public class SceneRegistrationNameResolverTests
{
    private static readonly string[] ExpectedNames = ["scene_a", "scene_b"];
    private sealed class SceneA : IScene
    {
        public string Name => "scene_a";

        public IEnumerable<IGameEntity> GetSceneGameObjects()
            => [];

        public void OnInitialize(ISceneManager sceneManager) { }

        public void OnLoad() { }

        public void OnUnload() { }

        public void RegisterGlobals(IGameEntityManager gameObjectManager) { }
    }

    private sealed class SceneB : IScene
    {
        public string Name => "scene_b";

        public IEnumerable<IGameEntity> GetSceneGameObjects()
            => [];

        public void OnInitialize(ISceneManager sceneManager) { }

        public void OnLoad() { }

        public void OnUnload() { }

        public void RegisterGlobals(IGameEntityManager gameObjectManager) { }
    }

    [Test]
    public void ResolveRegisteredSceneNames_Returns_All_Registered_Scene_Names()
    {
        var container = new Container();
        container.Register<SceneA>(Reuse.Singleton);
        container.Register<SceneB>(Reuse.Singleton);

        var registrations = new List<SceneRegistrationObject>
        {
            new(typeof(SceneA), false),
            new(typeof(SceneB), false)
        };

        var names = SceneRegistrationNameResolver.ResolveRegisteredSceneNames(registrations, container);

        Assert.That(names, Is.EquivalentTo(ExpectedNames));
    }
}

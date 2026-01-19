using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Systems;

public class UpdateSystem : BaseSystem, IUpdateSystem
{
    private readonly ISceneManager _sceneManager;

    public UpdateSystem(IGameEntityManager entityManager, ISceneManager sceneManager)
        : base("Update System", 0, entityManager)
    {
        _sceneManager = sceneManager;
    }

    public void FixedUpdate(GameTime gameTime)
    {
        // Only update entities from the current (top) scene
        var currentScene = _sceneManager.CurrentScene;
        if (currentScene == null)
        {
            return;
        }

        var updateableFeatures = EntityManager.QueryOfType<IFixedUpdateFeature>();
        foreach (var updateableFeature in updateableFeatures)
        {
            updateableFeature.FixedUpdate(gameTime);
        }
    }

    public void Update(GameTime gameTime)
    {
        // Only update entities from the current (top) scene
        var currentScene = _sceneManager.CurrentScene;
        if (currentScene == null)
        {
            return;
        }

        var updateableFeatures = EntityManager.QueryOfType<IUpdateFeature>();
        foreach (var updateableFeature in updateableFeatures)
        {
            updateableFeature.Update(gameTime);
        }
    }
}

using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Systems;

public class UpdateSystem : BaseSystem, IUpdateSystem
{
    public UpdateSystem(IGameEntityManager entityManager) : base("Update System", 0, entityManager) { }

    public void FixedUpdate(GameTime gameTime)
    {
        foreach (var updateableFeature in EntityManager.QueryOfType<IFixedUpdateFeature>())
        {
            updateableFeature.FixedUpdate(gameTime);
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var updateableFeature in EntityManager.QueryOfType<IUpdateFeature>())
        {
            updateableFeature.Update(gameTime);
        }
    }
}

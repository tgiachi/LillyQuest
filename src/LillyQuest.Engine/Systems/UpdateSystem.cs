using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Systems;

public class UpdateSystem : BaseSystem, IUpdateSystem
{
    public UpdateSystem( IGameEntityManager entityManager) : base("Update system", 0, entityManager) { }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement update logic using EntityManager
    }

    public void FixedUpdate(GameTime gameTime)
    {
        // TODO: Implement fixed update logic using EntityManager
    }
}

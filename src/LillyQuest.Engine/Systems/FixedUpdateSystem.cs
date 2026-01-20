using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Systems.Base;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems;

public class FixedUpdateSystem : BaseSystem<IFixedUpdateableEntity>
{
    public FixedUpdateSystem() : base(
        140,
        "Fixed update system",
        SystemQueryType.FixedUpdateable
    ) { }

    protected override void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<IFixedUpdateableEntity> typedEntities
    )
    {
        foreach (var entity in typedEntities)
        {
            entity.FixedUpdate(gameTime);
        }
    }
}

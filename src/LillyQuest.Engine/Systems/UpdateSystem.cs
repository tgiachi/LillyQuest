using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Systems.Base;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems;

public class UpdateSystem : BaseSystem<IUpdateableEntity>
{
    public UpdateSystem(uint order, string name, SystemQueryType queryType) : base(
        0,
        "Update system",
        SystemQueryType.Updateable
    ) { }

    protected override void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<IUpdateableEntity> typedEntities
    )
    {
        foreach (var entity in typedEntities)
        {
            entity.Update(gameTime);
        }
    }
}

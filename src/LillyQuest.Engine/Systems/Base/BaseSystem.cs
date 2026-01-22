using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems.Base;

public abstract class BaseSystem<TEntityFeature> : ISystem where TEntityFeature : class, IEntityFeature
{
    public uint Order { get; }
    public string Name { get; }
    public SystemQueryType QueryType { get; }

    protected BaseSystem(uint order, string name, SystemQueryType queryType)
    {
        Order = order;
        Name = name;
        QueryType = queryType;
    }

    public virtual void Initialize() { }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        var typedEntities = entityManager.GetQueryOf<TEntityFeature>();

        ProcessTypedEntities(gameTime, entityManager, typedEntities);
    }

    protected virtual void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<TEntityFeature> typedEntities
    ) { }
}

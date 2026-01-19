using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;

namespace LillyQuest.Engine.Systems;

public abstract class BaseSystem : ISystem
{
    public string Name { get; }
    public uint Priority { get; }

    protected IGameEntityManager EntityManager { get; }

    protected BaseSystem(string name, uint priority, IGameEntityManager entityManager)
    {
        Name = name;
        Priority = priority;
        EntityManager = entityManager;
    }

    public virtual void Initialize()
    {
    }
    public virtual void Shutdown()
    {
    }
}

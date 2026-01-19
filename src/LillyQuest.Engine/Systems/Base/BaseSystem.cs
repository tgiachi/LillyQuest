using LillyQuest.Core.Data.Contexts;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;

namespace LillyQuest.Engine.Systems.Base;

public abstract class BaseSystem : ISystem
{
    public string Name { get; }
    public uint Priority { get; }

    private bool _initialized;

    protected IGameEntityManager EntityManager { get; }

    protected EngineRenderContext RenderContext { get; private set; }

    protected BaseSystem(string name, uint priority, IGameEntityManager entityManager)
    {
        Name = name;
        Priority = priority;
        EntityManager = entityManager;
    }

    public void Initialize(EngineRenderContext renderContext)
    {
        if (_initialized)
        {
            return;
        }
        RenderContext = renderContext;

        _initialized = true;

        OnInitialize();
    }

    public virtual void Shutdown() { }

    protected virtual void OnInitialize() { }
}

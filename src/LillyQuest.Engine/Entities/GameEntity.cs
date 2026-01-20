using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Entities;

public class GameEntity : IGameEntity
{
    public uint Id { get; set; }
    public uint Order { get; set; }
    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public IList<IGameEntity> Children { get; set; } = new List<IGameEntity>();

    public IGameEntity? Parent { get; set; }

    public virtual void Initialize() { }

    protected void AddChild(IGameEntity child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    protected void RemoveChild(IGameEntity child)
    {
        if (Children.Remove(child))
        {
            child.Parent = null;
        }
    }
}

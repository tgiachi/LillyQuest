using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Entities;

public class GameEntity : IGameEntity
{
    public uint Id { get; set; }

    public uint Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public IList<IGameEntity> Children { get; set; } = new List<IGameEntity>();

    public IGameEntity? Parent { get; set; }

    public void Initialize()
    {
    }
}

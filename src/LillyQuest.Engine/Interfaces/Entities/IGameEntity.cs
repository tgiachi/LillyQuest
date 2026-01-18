using LillyQuest.Engine.Interfaces.Components;

namespace LillyQuest.Engine.Interfaces.Entities;

public interface IGameEntity
{
    uint Id { get; }
    string Name { get; }
    bool IsActive { get; set; }

    IEnumerable<IGameComponent> Components { get; }
}

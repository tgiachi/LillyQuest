namespace LillyQuest.Engine.Interfaces.Entities;

public interface IGameEntity
{

    uint Id { get; set; }

    uint Order { get; set; }

    string Name { get; set; }

    bool IsActive { get; set; }

    IList<IGameEntity> Children { get; set; }

    IGameEntity? Parent { get; set; }

    void Initialize();
}

using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Interfaces.Components;

public interface IGameComponent
{
    IGameEntity Owner { get; set; }

    void Initialize();


}

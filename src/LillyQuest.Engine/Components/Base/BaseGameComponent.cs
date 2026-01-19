using LillyQuest.Engine.Interfaces.Components;
using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Components.Base;

public abstract class BaseGameComponent : IGameComponent
{
    public IGameEntity Owner { get; set; }
    public virtual void Initialize()
    {

    }
}

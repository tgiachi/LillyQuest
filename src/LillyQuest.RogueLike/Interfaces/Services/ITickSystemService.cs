using Runeforge.Engine.Interfaces.Ticks;

namespace LillyQuest.RogueLike.Interfaces.Services;

public interface ITickSystemService
{
    delegate void TickDelegate(int tickCount);

    int TickCount { get; }

    event TickDelegate Tick;
    event TickDelegate TickStarted;
    event TickDelegate TickEnded;

    void ExecuteTick();

    void EnqueueActions(IEnumerable<ITickAction> actions);

    void EnqueueAction(ITickAction action);

    void ClearContinuingActions();
}

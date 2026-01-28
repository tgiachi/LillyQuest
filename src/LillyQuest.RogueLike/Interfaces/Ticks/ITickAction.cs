using LillyQuest.RogueLike.Types.Tick;

namespace Runeforge.Engine.Interfaces.Ticks;

public interface ITickAction
{
    Guid Id { get; }

    ActionPriority Priority { get; }

    int Speed { get; set; }

    bool CanBeExecuted();

    ActionResult Execute();
}

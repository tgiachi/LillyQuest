using LillyQuest.RogueLike.Interfaces.Scheduler;
using LillyQuest.RogueLike.Types.Scheduler;

namespace LillyQuest.RogueLike.Data.Scheduler;

/// <summary>
/// Record of a single action execution.
/// </summary>
public sealed class ActionExecutionRecord
{
    public required ISchedulerAction Action { get; init; }
    public required ISchedulerEntity Actor { get; init; }
    public required ActionResult Result { get; init; }
    public required int EnergyCostPaid { get; init; }
    public required int ActorEnergyAfter { get; init; }
}

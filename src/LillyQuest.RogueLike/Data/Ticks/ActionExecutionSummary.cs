using System.Diagnostics;
using LillyQuest.RogueLike.Interfaces.Ticks;
using LillyQuest.RogueLike.Types.Tick;

namespace LillyQuest.RogueLike.Data.Ticks;

/// <summary>
/// Summary of action execution with timing and result information
/// </summary>
public record ActionExecutionSummary(
    ITickAction Action,
    ActionResult Result,
    long StartTimestamp,
    long EndTimestamp,
    string? Message = null
)
{
    /// <summary>
    /// Duration of action execution
    /// </summary>
    public TimeSpan Duration => Stopwatch.GetElapsedTime(StartTimestamp, EndTimestamp);

    /// <summary>
    /// Whether the action executed successfully
    /// </summary>
    public bool WasSuccessful => Result == ActionResult.Success;

    /// <summary>
    /// Action type name for logging
    /// </summary>
    public string ActionTypeName => Action.GetType().Name;

    /// <summary>
    /// Action ID for tracking
    /// </summary>
    public Guid ActionId => Action.Id;

    public override string ToString()
        => $"{ActionTypeName}({ActionId}): {Result} in {Duration.TotalMilliseconds:F3}ms" +
           (Message != null ? $" - {Message}" : "");
}

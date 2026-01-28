namespace LillyQuest.RogueLike.Data.Ticks;

/// <summary>
/// Statistics about action queue execution
/// </summary>
public record ActionQueueStats(
    int TotalActions,
    int SuccessfulActions,
    int FailedActions,
    double TotalDuration,
    double AverageDuration
)
{
    public double SuccessRate => TotalActions > 0 ? (double)SuccessfulActions / TotalActions * 100 : 0;

    public override string ToString()
        => $"Actions: {SuccessfulActions}/{TotalActions} ({SuccessRate:F1}% success), " +
           $"Duration: {TotalDuration:F1}ms total, {AverageDuration:F1}ms avg";
}

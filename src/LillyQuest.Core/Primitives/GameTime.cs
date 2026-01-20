namespace LillyQuest.Core.Primitives;

public sealed class GameTime
{
    public TimeSpan TotalGameTime { get; set; }
    public TimeSpan Elapsed { get; set; }

    public GameTime()
        : this(TimeSpan.Zero, TimeSpan.Zero) { }

    public GameTime(TimeSpan totalGameTime, TimeSpan elapsed)
    {
        TotalGameTime = totalGameTime;
        Elapsed = elapsed;
    }

    public override string ToString()
        => $"TotalGameTime: {TotalGameTime}, Elapsed: {Elapsed}";

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta must be non-negative.");
        }

        Elapsed = TimeSpan.FromSeconds(deltaSeconds);
        TotalGameTime += Elapsed;
    }
}

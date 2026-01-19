namespace LillyQuest.Core.Primitives;

public sealed class GameTime
{
    public TimeSpan TotalGameTime { get; set; }
    public TimeSpan ElapsedGameTime { get; set; }

    public GameTime()
        : this(TimeSpan.Zero, TimeSpan.Zero) { }

    public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
    {
        TotalGameTime = totalGameTime;
        ElapsedGameTime = elapsedGameTime;
    }

    public override string ToString()
        => $"TotalGameTime: {TotalGameTime}, ElapsedGameTime: {ElapsedGameTime}";

    public void Update(double deltaSeconds)
    {
        if (deltaSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta must be non-negative.");
        }

        ElapsedGameTime = TimeSpan.FromSeconds(deltaSeconds);
        TotalGameTime += ElapsedGameTime;
    }
}

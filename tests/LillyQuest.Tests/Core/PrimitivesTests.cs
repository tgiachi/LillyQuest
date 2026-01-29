using LillyQuest.Core.Primitives;

namespace LillyQuest.Tests.Core;

/// <summary>
/// Tests for the GameTime primitive class
/// </summary>
public class GameTimeTests
{
    [Test]
    public void Constructor_Default_InitializeWithZeroTimeSpans()
    {
        var gameTime = new GameTime();

        Assert.That(gameTime.TotalGameTime, Is.EqualTo(TimeSpan.Zero));
        Assert.That(gameTime.Elapsed, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void Constructor_WithTimeSpans_InitializeCorrectly()
    {
        var totalTime = TimeSpan.FromSeconds(100);
        var elapsedTime = TimeSpan.FromSeconds(16.666);

        var gameTime = new GameTime(totalTime, elapsedTime);

        Assert.That(gameTime.TotalGameTime, Is.EqualTo(totalTime));
        Assert.That(gameTime.Elapsed, Is.EqualTo(elapsedTime));
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var gameTime = new GameTime(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.016));

        var result = gameTime.ToString();

        Assert.That(result, Contains.Substring("TotalGameTime"));
        Assert.That(result, Contains.Substring("Elapsed"));
    }

    [Test]
    public void Update_MultipleTimes_CumulatesCorrectly()
    {
        var gameTime = new GameTime();
        const double deltaPerFrame = 0.016666;

        for (var i = 0; i < 60; i++)
        {
            gameTime.Update(deltaPerFrame);
        }

        Assert.That(gameTime.TotalGameTime.TotalSeconds, Is.GreaterThan(0.99).And.LessThan(1.01));
        Assert.That(gameTime.Elapsed.TotalSeconds, Is.EqualTo(deltaPerFrame).Within(0.0001));
    }

    [Test]
    public void Update_WithLargeDeltas_HandlesCorrectly()
    {
        var gameTime = new GameTime();
        const double largeFrameTime = 0.5; // 500ms frame

        gameTime.Update(largeFrameTime);

        Assert.That(gameTime.Elapsed.TotalSeconds, Is.EqualTo(largeFrameTime).Within(0.0001));
        Assert.That(gameTime.TotalGameTime.TotalSeconds, Is.EqualTo(largeFrameTime).Within(0.0001));
    }

    [Test]
    public void Update_WithNegativeDelta_ThrowsArgumentOutOfRangeException()
    {
        var gameTime = new GameTime();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => gameTime.Update(-0.016));
        Assert.That(ex.ParamName, Is.EqualTo("deltaSeconds"));
    }

    [Test]
    public void Update_WithPositiveDelta_IncreasesTotalAndSetElapsedTime()
    {
        var gameTime = new GameTime();
        const double deltaSeconds = 0.016666; // ~60 FPS

        gameTime.Update(deltaSeconds);

        Assert.That(gameTime.Elapsed.TotalSeconds, Is.EqualTo(deltaSeconds).Within(0.0001));
        Assert.That(gameTime.TotalGameTime.TotalSeconds, Is.EqualTo(deltaSeconds).Within(0.0001));
    }

    [Test]
    public void Update_WithZeroDelta_SetsElapsedTimeToZero()
    {
        var gameTime = new GameTime(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.016));

        gameTime.Update(0);

        Assert.That(gameTime.Elapsed, Is.EqualTo(TimeSpan.Zero));
        Assert.That(gameTime.TotalGameTime.TotalSeconds, Is.EqualTo(10));
    }
}

using LillyQuest.RogueLike.Components;

namespace LillyQuest.Tests.RogueLike.Components;

public class AnimationComponentTests
{
    [Test]
    public void Constructor_DefaultInterval_IsOneSecond()
    {
        var component = new AnimationComponent();

        Assert.That(component.IntervalSeconds, Is.EqualTo(1.0));
    }

    [Test]
    public void Constructor_SetsIntervalSeconds()
    {
        var component = new AnimationComponent(2.5);

        Assert.That(component.IntervalSeconds, Is.EqualTo(2.5));
    }

    [Test]
    public void Reset_ClearsAccumulatedTime()
    {
        var component = new AnimationComponent(1.0);

        component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.9)));
        component.Reset();

        // After reset, should need full interval again
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))), Is.False);
    }

    [Test]
    public void Update_AccumulatesTime_AcrossMultipleCalls()
    {
        var component = new AnimationComponent();

        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.3))), Is.False);
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.3))), Is.False);
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.3))), Is.False);
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.3))), Is.True);
    }

    [Test]
    public void Update_DoesNotInvokeCallback_WhenIntervalNotReached()
    {
        var callCount = 0;
        var component = new AnimationComponent(1.0, () => callCount++);

        component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

        Assert.That(callCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_InvokesCallback_WhenIntervalReached()
    {
        var callCount = 0;
        var component = new AnimationComponent(0.5, () => callCount++);

        component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.6)));

        Assert.That(callCount, Is.EqualTo(1));
    }

    [Test]
    public void Update_ResetsAccumulatedTime_AfterTrigger()
    {
        var component = new AnimationComponent(0.5);

        // First trigger
        component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.6)));

        // Should need another 0.5s to trigger again (0.1 carried over)
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.3))), Is.False);
        Assert.That(component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.2))), Is.True);
    }

    [Test]
    public void Update_ReturnsFalse_WhenIntervalNotReached()
    {
        var component = new AnimationComponent();

        var result = component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

        Assert.That(result, Is.False);
    }

    [Test]
    public void Update_ReturnsTrue_WhenIntervalReached()
    {
        var component = new AnimationComponent();

        component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));
        var result = component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.6)));

        Assert.That(result, Is.True);
    }

    [Test]
    public void Update_WorksWithoutCallback()
    {
        var component = new AnimationComponent(0.5);

        // Should not throw when no callback is set
        var result = component.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.6)));

        Assert.That(result, Is.True);
    }
}

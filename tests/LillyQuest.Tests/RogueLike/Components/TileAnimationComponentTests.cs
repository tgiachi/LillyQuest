using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.RogueLike.Components;

public class TileAnimationComponentTests
{
    [Test]
    public void Animation_Property_ReturnsOriginalAnimation()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop);
        var component = new TileAnimationComponent(animation);

        Assert.That(component.Animation, Is.SameAs(animation));
    }

    [Test]
    public void Constructor_InitializesWithFirstFrame()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop);
        var component = new TileAnimationComponent(animation);

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(0));
        Assert.That(component.CurrentFrame.Symbol, Is.EqualTo("A"));
    }

    [Test]
    public void Loop_WrapsAroundToFirstFrame()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, 3, 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(100)); // Frame 1
        component.Update(CreateGameTime(100)); // Frame 2
        component.Update(CreateGameTime(100)); // Back to Frame 0

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(0));
    }

    [Test]
    public void Once_IsFinishedIsFalseUntilComplete()
    {
        var animation = CreateTestAnimation(TileAnimationType.Once, 3, 100);
        var component = new TileAnimationComponent(animation);

        Assert.That(component.IsFinished, Is.False);

        component.Update(CreateGameTime(100));
        Assert.That(component.IsFinished, Is.False);

        component.Update(CreateGameTime(100));
        Assert.That(component.IsFinished, Is.True);
    }

    [Test]
    public void Once_StopsAtLastFrame()
    {
        var animation = CreateTestAnimation(TileAnimationType.Once, 3, 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(100)); // 1
        component.Update(CreateGameTime(100)); // 2
        component.Update(CreateGameTime(100)); // Still 2
        component.Update(CreateGameTime(100)); // Still 2

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(2));
        Assert.That(component.IsFinished, Is.True);
    }

    [Test]
    public void PingPong_ReversesAtEnd()
    {
        var animation = CreateTestAnimation(TileAnimationType.PingPong, 3, 100);
        var component = new TileAnimationComponent(animation);

        // 0 -> 1 -> 2 -> 1 -> 0 -> 1
        component.Update(CreateGameTime(100)); // 1
        component.Update(CreateGameTime(100)); // 2
        component.Update(CreateGameTime(100)); // 1 (reverse)
        component.Update(CreateGameTime(100)); // 0
        component.Update(CreateGameTime(100)); // 1 (forward again)

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(1));
    }

    [Test]
    public void Random_SelectsRandomFrame()
    {
        var animation = CreateTestAnimation(TileAnimationType.Random, 5, 100);
        var rng = new Random(42);
        var component = new TileAnimationComponent(animation, rng);

        var seenFrames = new HashSet<int>();

        // Update many times and collect different frames
        for (var i = 0; i < 20; i++)
        {
            component.Update(CreateGameTime(100));
            seenFrames.Add(component.CurrentFrameIndex);
        }

        // Should have seen multiple different frames
        Assert.That(seenFrames.Count, Is.GreaterThan(1));
    }

    [Test]
    public void Reset_ClearsIsFinished()
    {
        var animation = CreateTestAnimation(TileAnimationType.Once, 2);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(100)); // Finish animation

        Assert.That(component.IsFinished, Is.True);

        component.Reset();

        Assert.That(component.IsFinished, Is.False);
    }

    [Test]
    public void Reset_ReturnsToFirstFrame()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, durationMs: 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(100));
        component.Update(CreateGameTime(100));

        component.Reset();

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(0));
    }

    [Test]
    public void Update_AccumulatesTime()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, durationMs: 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(30));
        component.Update(CreateGameTime(30));
        component.Update(CreateGameTime(30));

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(0));

        component.Update(CreateGameTime(20)); // Total 110ms, should advance

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(1));
    }

    [Test]
    public void Update_AdvancesFrameAfterDuration()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, durationMs: 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(100));

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(1));
        Assert.That(component.CurrentFrame.Symbol, Is.EqualTo("B"));
    }

    [Test]
    public void Update_DoesNotAdvanceBeforeDuration()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, durationMs: 100);
        var component = new TileAnimationComponent(animation);

        component.Update(CreateGameTime(50));

        Assert.That(component.CurrentFrameIndex, Is.EqualTo(0));
    }

    [Test]
    public void Update_ReturnsTrue_WhenFrameChanges()
    {
        var animation = CreateTestAnimation(TileAnimationType.Loop, durationMs: 100);
        var component = new TileAnimationComponent(animation);

        var result1 = component.Update(CreateGameTime(50));
        var result2 = component.Update(CreateGameTime(60)); // Total 110ms

        Assert.That(result1, Is.False);
        Assert.That(result2, Is.True);
    }

    private static GameTime CreateGameTime(double elapsedMs)
        => new(TimeSpan.Zero, TimeSpan.FromMilliseconds(elapsedMs));

    private static TileAnimation CreateTestAnimation(TileAnimationType type, int frameCount = 3, int durationMs = 100)
    {
        var frames = new List<TileAnimationFrame>();

        for (var i = 0; i < frameCount; i++)
        {
            frames.Add(
                new()
                {
                    Symbol = ((char)('A' + i)).ToString(),
                    FgColor = $"color{i}"
                }
            );
        }

        return new()
        {
            Type = type,
            Frames = frames,
            FrameDurationMs = durationMs
        };
    }
}

using System.Reflection;
using LillyQuest.Engine.Systems;

namespace LillyQuest.Tests;

public class InputSystemScrollWheelTests
{
    [Test]
    public void ComputeWheelDelta_IgnoresResetToZero()
    {
        var delta = InvokeComputeWheelDelta(1.5f, 0f);

        Assert.That(delta, Is.EqualTo(0f));
    }

    [Test]
    public void ComputeWheelDelta_ReturnsDeltaWhenWheelContinues()
    {
        var delta = InvokeComputeWheelDelta(1.5f, 2.0f);

        Assert.That(delta, Is.EqualTo(0.5f));
    }

    [Test]
    public void ComputeWheelDelta_ReturnsDeltaWhenWheelMoves()
    {
        var delta = InvokeComputeWheelDelta(0f, 1.5f);

        Assert.That(delta, Is.EqualTo(1.5f));
    }

    private static float InvokeComputeWheelDelta(float previousY, float currentY)
    {
        var method = typeof(InputSystem).GetMethod(
            "ComputeWheelDelta",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        Assert.That(method, Is.Not.Null);

        var result = method!.Invoke(null, new object[] { previousY, currentY });

        return result is float delta ? delta : 0f;
    }
}

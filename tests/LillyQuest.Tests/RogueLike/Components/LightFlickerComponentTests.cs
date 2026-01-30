using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.Tests.RogueLike.Components;

public class LightFlickerComponentTests
{
    [Test]
    public void Constructor_SetsProperties()
    {
        var component = new LightFlickerComponent(
            LightFlickerMode.Deterministic,
            0.4f,
            2f,
            6f,
            123
        );

        Assert.That(component.Mode, Is.EqualTo(LightFlickerMode.Deterministic));
        Assert.That(component.Intensity, Is.EqualTo(0.4f));
        Assert.That(component.RadiusJitter, Is.EqualTo(2f));
        Assert.That(component.FrequencyHz, Is.EqualTo(6f));
        Assert.That(component.Seed, Is.EqualTo(123));
    }
}

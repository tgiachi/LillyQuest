using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Components;

public sealed class LightFlickerComponent
{
    public LightFlickerMode Mode { get; }
    public float Intensity { get; }
    public float RadiusJitter { get; }
    public float FrequencyHz { get; }
    public int? Seed { get; }

    public LightFlickerComponent(
        LightFlickerMode mode,
        float intensity,
        float radiusJitter,
        float frequencyHz,
        int? seed = null)
    {
        Mode = mode;
        Intensity = intensity;
        RadiusJitter = radiusJitter;
        FrequencyHz = frequencyHz;
        Seed = seed;
    }
}

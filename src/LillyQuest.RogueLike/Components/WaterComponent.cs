using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Types;

namespace LillyQuest.RogueLike.Components;

/// <summary>
/// Component that animates water with a wave-like PingPong animation.
/// Water waves move back and forth, creating a natural ripple effect with blue color variations.
/// </summary>
public sealed class WaterComponent
{
    public TileAnimationComponent Animation { get; }

    public WaterComponent(int frameDurationMs = 150)
    {
        var animation = new TileAnimation
        {
            Type = TileAnimationType.PingPong,
            FrameDurationMs = frameDurationMs,
            Frames = new()
            {
                new() { Symbol = "~", FgColor = "#2E5984" }, // Dark blue
                new() { Symbol = "~", FgColor = "#3A7BC8" }, // Medium blue
                new() { Symbol = "~", FgColor = "#4A9FDF" }, // Bright blue
                new() { Symbol = "≈", FgColor = "#5BB3E8" }, // Light blue wave
            }
        };

        Animation = new(animation);
    }
}

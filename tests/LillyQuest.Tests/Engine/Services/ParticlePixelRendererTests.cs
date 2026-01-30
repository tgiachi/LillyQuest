using System.Numerics;
using LillyQuest.Engine.Services.Rendering;

namespace LillyQuest.Tests.Engine.Services;

public sealed class ParticlePixelRendererTests
{
    [Test]
    public void ComputeParticleScreenPosition_WithOffsetsAndScale_ReturnsExpected()
    {
        // Arrange
        var particlePosition = new Vector2(1.5f, 2.25f);
        var tileSize = new Vector2(16f, 16f);
        var tileRenderScale = 2f;
        var layerRenderScale = 0.5f;
        var viewTileOffset = new Vector2(1f, 1f);
        var viewPixelOffset = new Vector2(4f, 6f);
        var layerPixelOffset = new Vector2(8f, 10f);

        // Act
        var result = ParticlePixelRenderer.ComputeParticleScreenPosition(
            particlePosition,
            tileSize,
            tileRenderScale,
            layerRenderScale,
            viewTileOffset,
            viewPixelOffset,
            layerPixelOffset
        );

        // Assert
        Assert.That(result.X, Is.EqualTo(12f).Within(0.001f));
        Assert.That(result.Y, Is.EqualTo(24f).Within(0.001f));
    }
}

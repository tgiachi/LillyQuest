using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.TilesetSurface;
using NUnit.Framework;
using System.Numerics;

namespace LillyQuest.Tests.Engine;

public class TilesetSurfaceAnimatorTests
{
    [Test]
    public void ProcessMovements_AdvancesElapsedTime()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        surface.SetTile(0, 0, 0, new TileRenderData(1, LyColor.White));

        var animator = new TilesetSurfaceAnimator(surface);
        animator.EnqueueMove(0, new Vector2(0, 0), new Vector2(1, 0), 1.0f);

        animator.ProcessMovements(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

        var activeMovements = animator.GetActiveMovements(0);
        Assert.That(activeMovements.Count, Is.EqualTo(1));
        Assert.That(activeMovements[0].ElapsedSeconds, Is.EqualTo(0.5f).Within(0.01f));
    }

    [Test]
    public void EnqueueMove_ReturnsFalse_WhenLayerIndexInvalid()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.EnqueueMove(99, Vector2.Zero, Vector2.One, 1.0f);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ProcessMovements_CompletesMovement_WhenDurationExceeded()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        surface.SetTile(0, 0, 0, new TileRenderData(1, LyColor.White));

        var animator = new TilesetSurfaceAnimator(surface);
        animator.EnqueueMove(0, new Vector2(0, 0), new Vector2(1, 0), 0.5f);

        animator.ProcessMovements(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

        var activeMovements = animator.GetActiveMovements(0);
        Assert.That(activeMovements.Count, Is.EqualTo(0));

        var destinationTile = surface.GetTile(0, 1, 0);
        Assert.That(destinationTile.TileIndex, Is.EqualTo(1));
    }

    [Test]
    public void EnqueueMove_ReturnsFalse_WhenSourceOutOfBounds()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.EnqueueMove(0, new Vector2(-1, 0), new Vector2(1, 0), 1.0f);

        Assert.That(result, Is.False);
    }

    [Test]
    public void EnqueueMove_ReturnsFalse_WhenDestinationOutOfBounds()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        surface.SetTile(0, 0, 0, new TileRenderData(1, LyColor.White));
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.EnqueueMove(0, new Vector2(0, 0), new Vector2(100, 0), 1.0f);

        Assert.That(result, Is.False);
    }

    [Test]
    public void EnqueueMove_ReturnsFalse_WhenSourceTileEmpty()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        // No tile set at (0,0), so TileIndex will be -1
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.EnqueueMove(0, new Vector2(0, 0), new Vector2(1, 0), 1.0f);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GetActiveMovements_ReturnsEmpty_WhenLayerIndexInvalid()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.GetActiveMovements(99);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetLayerMovementQueue_ReturnsNull_WhenLayerIndexInvalid()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.GetLayerMovementQueue(99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLayerMovementQueue_ReturnsQueue_WhenLayerValid()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        var animator = new TilesetSurfaceAnimator(surface);

        var result = animator.GetLayerMovementQueue(0);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void GetMovementTilePosition_ReturnsInterpolatedPosition()
    {
        var movement = new TileMovement(
            0,
            new Vector2(0, 0),
            new Vector2(4, 0),
            false,
            1.0f,
            new TileRenderData(1, LyColor.White)
        );
        movement.ElapsedSeconds = 0.5f;

        var position = TilesetSurfaceAnimator.GetMovementTilePosition(movement);

        Assert.That(position.X, Is.EqualTo(2.0f).Within(0.01f));
        Assert.That(position.Y, Is.EqualTo(0.0f).Within(0.01f));
    }

    [Test]
    public void ProcessMovements_DoesNothing_WhenElapsedTimeIsZero()
    {
        var surface = new TilesetSurface(10, 10);
        surface.Initialize(1);
        surface.SetTile(0, 0, 0, new TileRenderData(1, LyColor.White));

        var animator = new TilesetSurfaceAnimator(surface);
        animator.EnqueueMove(0, new Vector2(0, 0), new Vector2(1, 0), 1.0f);

        animator.ProcessMovements(new GameTime(TimeSpan.Zero, TimeSpan.Zero));

        // Movement should still be pending since no time passed
        var pendingQueue = animator.GetLayerMovementQueue(0);
        Assert.That(pendingQueue!.Pending.Count, Is.EqualTo(1));
        Assert.That(pendingQueue.Active.Count, Is.EqualTo(0));
    }
}

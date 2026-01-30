using LillyQuest.Engine.Services;

namespace LillyQuest.Tests.Engine.Services;

public class JobSchedulerTests
{
    [Test]
    public async Task Dispose_WhenCalled_PreventsFurtherEnqueue()
    {
        // Arrange
        var sut = new JobScheduler();
        sut.Start(1);

        // Act
        sut.Dispose();

        // Assert
        Assert.Throws<InvalidOperationException>(() => sut.Enqueue(() => { }));
    }

    [Test]
    public async Task Enqueue_BeforeStart_ExecutesAfterStart()
    {
        // Arrange
        var sut = new JobScheduler();
        var tcs = new TaskCompletionSource<bool>();

        // Act
        sut.Enqueue(() => tcs.SetResult(true));
        sut.Start(1);

        // Assert
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(500));
        Assert.That(completed, Is.EqualTo(tcs.Task));
    }

    [Test]
    public async Task StopAsync_WhenCalled_PreventsFurtherEnqueue()
    {
        // Arrange
        var sut = new JobScheduler();
        sut.Start(1);
        await sut.StopAsync();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Enqueue(() => { }));
    }
}

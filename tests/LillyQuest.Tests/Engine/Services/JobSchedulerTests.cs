using LillyQuest.Engine.Services;

namespace LillyQuest.Tests.Engine.Services;

public class JobSchedulerTests
{
    [Test]
    public async Task Enqueue_BeforeStart_ExecutesAfterStart()
    {
        // Arrange
        var sut = new JobScheduler();
        var tcs = new TaskCompletionSource<bool>();

        // Act
        sut.Enqueue(() => tcs.SetResult(true));
        sut.Start(workerCount: 1);

        // Assert
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(500));
        Assert.That(completed, Is.EqualTo(tcs.Task));
    }
}

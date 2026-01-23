using LillyQuest.Engine.Bootstrap;

namespace LillyQuest.Tests.Engine.Bootstrap;

public class ResourceLoadingFlowTests
{
    private sealed class FakeLoader : IAsyncResourceLoader
    {
        public bool IsLoading { get; private set; }
        public bool IsLoadingComplete { get; private set; } = true;
        public int StartCalls { get; private set; }
        public Func<Task>? PendingOperation { get; private set; }

        public void StartAsyncLoading(Func<Task> loadingOperation)
        {
            StartCalls++;
            IsLoading = true;
            IsLoadingComplete = false;
            PendingOperation = loadingOperation;
        }

        public Task WaitForLoadingComplete()
            => Task.CompletedTask;

        public void Complete()
        {
            IsLoading = false;
            IsLoadingComplete = true;
        }
    }

    [Test]
    public void StartLoading_ShowsLogScreen_And_Starts_Async_Load()
    {
        var loader = new FakeLoader();
        var flow = new ResourceLoadingFlow(loader);
        var logShown = 0;
        var started = 0;

        flow.StartLoading(
            () => Task.CompletedTask,
            () => Task.CompletedTask,
            () => logShown++,
            () => started++
        );

        Assert.That(logShown, Is.EqualTo(1));
        Assert.That(started, Is.EqualTo(0));
    }

    [Test]
    public void Update_Starts_InitialScene_When_Loading_Completes()
    {
        var loader = new FakeLoader();
        var flow = new ResourceLoadingFlow(loader);
        var completed = 0;

        flow.StartLoading(
            () => Task.CompletedTask,
            () => Task.CompletedTask,
            () => { },
            () => completed++
        );

        loader.Complete();
        flow.Update();

        Assert.That(completed, Is.EqualTo(1));

        flow.Update();
        Assert.That(completed, Is.EqualTo(1));
    }
}

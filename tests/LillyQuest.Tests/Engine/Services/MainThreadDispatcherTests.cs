using System.Diagnostics;
using System.Threading;
using LillyQuest.Engine.Services;

namespace LillyQuest.Tests.Engine.Services;

public class MainThreadDispatcherTests
{
    [Test]
    public void Post_Queues_Until_Processed()
    {
        var dispatcher = new MainThreadDispatcher();
        var ran = false;

        dispatcher.Post(() => ran = true);

        Assert.That(ran, Is.False);

        dispatcher.ExecutePending(1);

        Assert.That(ran, Is.True);
    }

    [Test]
    public void ExecutePending_Respects_MaxPerFrame()
    {
        var dispatcher = new MainThreadDispatcher();
        var count = 0;

        dispatcher.Post(() => count++);
        dispatcher.Post(() => count++);

        dispatcher.ExecutePending(1);

        Assert.That(count, Is.EqualTo(1));

        dispatcher.ExecutePending(1);

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Invoke_FromBackgroundThread_RunsOnMainThread()
    {
        var dispatcher = new MainThreadDispatcher();
        var mainThreadId = dispatcher.MainThreadId;
        var invokedThreadId = -1;

        var task = Task.Run(() =>
        {
            return dispatcher.Invoke(() =>
            {
                invokedThreadId = Environment.CurrentManagedThreadId;
                return 42;
            });
        });

        var sw = Stopwatch.StartNew();
        while (!task.IsCompleted && sw.Elapsed < TimeSpan.FromSeconds(1))
        {
            dispatcher.ExecutePending(1);
            Thread.Sleep(1);
        }

        Assert.That(task.IsCompleted, Is.True);
        Assert.That(task.Result, Is.EqualTo(42));
        Assert.That(invokedThreadId, Is.EqualTo(mainThreadId));
    }

    [Test]
    public void Invoke_OnMainThread_ExecutesImmediately()
    {
        var dispatcher = new MainThreadDispatcher();
        var ran = false;

        var result = dispatcher.Invoke(() =>
        {
            ran = true;
            return 7;
        });

        Assert.That(ran, Is.True);
        Assert.That(result, Is.EqualTo(7));
    }
}

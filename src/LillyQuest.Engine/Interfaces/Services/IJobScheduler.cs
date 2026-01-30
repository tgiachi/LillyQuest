namespace LillyQuest.Engine.Interfaces.Services;

public interface IJobScheduler
{
    void Start(int workerCount);

    Task StopAsync();

    void Enqueue(Action job);

    void Enqueue(Func<Task> job);
}

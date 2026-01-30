namespace LillyQuest.Engine.Interfaces.Services;

public interface IJobScheduler
{
    void Enqueue(Action job);

    void Enqueue(Func<Task> job);
    void Start(int workerCount);

    Task StopAsync();
}

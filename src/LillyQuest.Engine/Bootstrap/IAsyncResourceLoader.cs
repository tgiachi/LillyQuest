namespace LillyQuest.Engine.Bootstrap;

public interface IAsyncResourceLoader
{
    bool IsLoading { get; }
    bool IsLoadingComplete { get; }
    void StartAsyncLoading(Func<Task> loadingOperation);
    Task WaitForLoadingComplete();
}

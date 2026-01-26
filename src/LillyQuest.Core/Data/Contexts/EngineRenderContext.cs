using LillyQuest.Core.Primitives;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LillyQuest.Core.Data.Contexts;

public class EngineRenderContext
{
    public LyColor ClearColor { get; set; } = LyColor.CornflowerBlue;
    public IWindow Window { get; set; }
    public IInputContext InputContext { get; set; }
    public GL Gl { get; set; }
    /// <summary>
    /// Handler used to dispatch fire-and-forget work on the main thread.
    /// </summary>
    public Action<Action>? PostOnMainThreadHandler { get; set; }

    /// <summary>
    /// Handler used to invoke work on the main thread and block until completion.
    /// </summary>
    public Action<Action>? InvokeOnMainThreadHandler { get; set; }

    /// <summary>
    /// Handler used to invoke a function on the main thread and return its result.
    /// </summary>
    public Func<Delegate, object?>? InvokeOnMainThreadFuncHandler { get; set; }

    /// <summary>
    /// Dispatches fire-and-forget work on the main thread.
    /// </summary>
    public void PostOnMainThread(Action action)
    {
        if (PostOnMainThreadHandler != null)
        {
            PostOnMainThreadHandler(action);
            return;
        }

        action();
    }

    /// <summary>
    /// Invokes work on the main thread and blocks until completion.
    /// </summary>
    public void InvokeOnMainThread(Action action)
    {
        if (InvokeOnMainThreadHandler != null)
        {
            InvokeOnMainThreadHandler(action);
            return;
        }

        action();
    }

    /// <summary>
    /// Invokes a function on the main thread and returns its result.
    /// </summary>
    public T InvokeOnMainThread<T>(Func<T> func)
    {
        if (InvokeOnMainThreadFuncHandler != null)
        {
            var result = InvokeOnMainThreadFuncHandler(func);
            return result is T typed ? typed : default!;
        }

        return func();
    }
}

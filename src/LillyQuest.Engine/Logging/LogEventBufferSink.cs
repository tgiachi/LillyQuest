using Serilog.Core;
using Serilog.Events;

namespace LillyQuest.Engine.Logging;

/// <summary>
/// Serilog sink that buffers log events for later dispatch.
/// </summary>
public sealed class LogEventBufferSink : ILogEventSink
{
    private readonly ILogEventDispatcher _dispatcher;

    public LogEventBufferSink(ILogEventDispatcher dispatcher)
        => _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
        {
            return;
        }

        var entry = new LogEntry(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.RenderMessage(),
            logEvent.Exception?.ToString()
        );

        _dispatcher.Enqueue(entry);
    }
}

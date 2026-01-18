using Serilog.Events;

namespace LillyQuest.Engine.Logging;

/// <summary>
/// Represents a log entry captured from Serilog.
/// </summary>
public sealed record LogEntry(DateTimeOffset Timestamp, LogEventLevel Level, string Message, string? Exception);

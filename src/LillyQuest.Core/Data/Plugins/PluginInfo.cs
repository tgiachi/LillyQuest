namespace LillyQuest.Core.Data.Plugins;

/// <summary>
/// Defines metadata for an engine plugin.
/// </summary>
public record PluginInfo(
    string Id,
    string Name,
    string Version,
    string Author,
    string Description,
    params string[]? Dependencies
);

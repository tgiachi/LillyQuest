using System.Reflection;

namespace LillyQuest.Core.Data.Plugins;

/// <summary>
/// Represents a registered engine plugin type.
/// </summary>
public record EnginePluginRegistration(Assembly Assembly, Type PluginType);

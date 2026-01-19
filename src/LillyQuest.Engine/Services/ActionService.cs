using DarkLilly.Core.Utils;
using DarkLilly.Engine.Interfaces.Services;
using Serilog;

namespace DarkLilly.Engine.Services;

/// <summary>
/// Provides action registration and execution by name.
/// </summary>
public sealed class ActionService : IActionService
{
    private readonly Dictionary<string, Action> _actions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _actionUseCounts = new(StringComparer.OrdinalIgnoreCase);

    private readonly ILogger _logger = Log.ForContext<ActionService>();

    /// <summary>
    /// Executes a registered action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when executed.</returns>
    public bool Execute(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        var normalizedName = NormalizeActionName(actionName);

        if (!_actions.TryGetValue(normalizedName, out var action))
        {
            return false;
        }

        _logger.Debug("Executing action '{ActionName}'.", actionName);
        action();

        return true;
    }

    /// <summary>
    /// Checks whether an action exists.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when registered.</returns>
    public bool HasAction(string actionName)
        => !string.IsNullOrWhiteSpace(actionName) && _actions.ContainsKey(NormalizeActionName(actionName));

    /// <summary>
    /// Checks whether an action is currently in use (pressed/held).
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when active.</returns>
    public bool IsActionInUse(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return false;
        }

        var normalizedName = NormalizeActionName(actionName);

        return _actionUseCounts.TryGetValue(normalizedName, out var count) && count > 0;
    }

    /// <summary>
    /// Marks an action as in use (pressed/held).
    /// </summary>
    /// <param name="actionName">Action name.</param>
    public void MarkActionInUse(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return;
        }

        var normalizedName = NormalizeActionName(actionName);
        _actionUseCounts.TryGetValue(normalizedName, out var count);
        _actionUseCounts[normalizedName] = count + 1;
    }

    /// <summary>
    /// Marks an action as released.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    public void MarkActionReleased(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return;
        }

        var normalizedName = NormalizeActionName(actionName);

        if (!_actionUseCounts.TryGetValue(normalizedName, out var count))
        {
            return;
        }

        count = Math.Max(0, count - 1);

        if (count == 0)
        {
            _actionUseCounts.Remove(normalizedName);

            return;
        }

        _actionUseCounts[normalizedName] = count;
    }

    /// <summary>
    /// Registers or replaces an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="action">Action callback.</param>
    public void RegisterAction(string actionName, Action action)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException("Action name cannot be null or empty.", nameof(actionName));
        }

        _logger.Information("Registering action '{ActionName}'.", actionName);
        var normalizedName = NormalizeActionName(actionName);
        _actions[normalizedName] = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Removes a registered action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when removed.</returns>
    public bool UnregisterAction(string actionName)
        => !string.IsNullOrWhiteSpace(actionName) && _actions.Remove(NormalizeActionName(actionName));

    private static string NormalizeActionName(string actionName)
        => StringUtils.ToUpperSnakeCase(actionName);
}

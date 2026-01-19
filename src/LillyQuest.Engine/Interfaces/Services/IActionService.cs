namespace LillyQuest.Engine.Interfaces.Services;

/// <summary>
/// Defines action registration and execution by name.
/// </summary>
public interface IActionService
{
    /// <summary>
    /// Executes a registered action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when executed.</returns>
    bool Execute(string actionName);

    /// <summary>
    /// Checks whether an action exists.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when registered.</returns>
    bool HasAction(string actionName);

    /// <summary>
    /// Checks whether an action is currently in use (pressed/held).
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when active.</returns>
    bool IsActionInUse(string actionName);

    /// <summary>
    /// Marks an action as in use (pressed/held).
    /// </summary>
    /// <param name="actionName">Action name.</param>
    void MarkActionInUse(string actionName);

    /// <summary>
    /// Marks an action as released.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    void MarkActionReleased(string actionName);

    /// <summary>
    /// Registers or replaces an action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <param name="action">Action callback.</param>
    void RegisterAction(string actionName, Action action);

    /// <summary>
    /// Removes a registered action.
    /// </summary>
    /// <param name="actionName">Action name.</param>
    /// <returns>True when removed.</returns>
    bool UnregisterAction(string actionName);
}

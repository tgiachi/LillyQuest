namespace LillyQuest.Core.Types;

/// <summary>
/// Specifies how sprites are batched and rendered.
/// </summary>
public enum BatchingMode
{
    /// <summary>
    /// Accumulates all draw calls and renders them on End().
    /// </summary>
    Deferred,

    /// <summary>
    /// Renders immediately on each draw call, skipping batching.
    /// </summary>
    Immediate,

    /// <summary>
    /// Draws on the fly without accumulating, but still uses batching.
    /// </summary>
    OnTheFly,

    /// <summary>
    /// Sorts sprites by texture to minimize texture changes.
    /// </summary>
    SortByTexture,

    /// <summary>
    /// Sorts sprites by depth (ascending) and then by texture.
    /// </summary>
    SortByDepthThenTexture
}

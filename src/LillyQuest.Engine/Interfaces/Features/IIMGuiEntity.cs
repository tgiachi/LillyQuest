namespace LillyQuest.Engine.Interfaces.Features;

/// <summary>
/// Marks an entity that can draw an ImGui panel.
/// </summary>
public interface IIMGuiEntity : IEntityFeature
{
    /// <summary>
    ///  The name of the ImGui panel.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Draws the ImGui UI for this entity.
    /// </summary>
    void DrawIMGui();
}

using ImGuiNET;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug entity that displays all entities in IGameEntityManager via ImGui.
/// Shows entities in a hierarchical tree view with their children and properties.
/// </summary>
public class DebugEntityGameObject : GameEntity, IIMGuiEntity
{
    private readonly IGameEntityManager _entityManager;
    private bool _showInactive = true;

    public string Name => "Entity Hierarchy";

    public DebugEntityGameObject(IGameEntityManager entityManager)
    {
        _entityManager = entityManager;
        IsActive = true;
    }

    /// <summary>
    /// Draws the ImGui panel showing entity hierarchy.
    /// </summary>
    public void DrawIMGui()
    {
        // Show/hide inactive entities toggle
        ImGui.Checkbox("Show Inactive Entities", ref _showInactive);

        var totalEntities = _entityManager.OrderedEntities.Count;
        var activeEntities = _entityManager.OrderedEntities.Count(e => e.IsActive);
        ImGui.Text($"Total Entities: {totalEntities} (Active: {activeEntities})");

        ImGui.Separator();

        // Show only root entities (those without a parent)
        var rootEntities = _entityManager.OrderedEntities
            .Where(e => e.Parent == null && (_showInactive || e.IsActive))
            .OrderBy(e => e.Order)
            .ThenBy(e => e.Id);

        foreach (var entity in rootEntities)
        {
            DrawEntityTree(entity);
        }
    }

    /// <summary>
    /// Recursively draws an entity and its children as a tree node.
    /// </summary>
    private void DrawEntityTree(IGameEntity entity)
    {
        // Show child count in node label
        var childCount = entity.Children.Count;
        var hasVisibleChildren = childCount > 0 && (
            _showInactive ||
            entity.Children.Any(c => c.IsActive)
        );

        var nodeLabel = $"{entity.Name} (ID: {entity.Id}, Order: {entity.Order})";
        var nodeFlags = ImGuiTreeNodeFlags.DefaultOpen;

        // Add a bullet for leaf nodes
        if (childCount == 0)
        {
            nodeFlags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        // Color code based on active state
        if (!entity.IsActive)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1.0f));
        }

        var isOpen = ImGui.TreeNodeEx(nodeLabel, nodeFlags);

        if (!entity.IsActive)
        {
            ImGui.PopStyleColor();
        }

        // Show entity details in a tooltip
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text($"Name: {entity.Name}");
            ImGui.Text($"ID: {entity.Id}");
            ImGui.Text($"Order: {entity.Order}");
            ImGui.Text($"Active: {entity.IsActive}");
            ImGui.Text($"Children: {childCount}");
            ImGui.EndTooltip();
        }

        // Draw children if this node is open
        if (isOpen && hasVisibleChildren)
        {
            var children = entity.Children
                .Where(c => _showInactive || c.IsActive)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.Id);

            foreach (var child in children)
            {
                DrawEntityTree(child);
            }

            if (childCount > 0)
            {
                ImGui.TreePop();
            }
        }
    }
}

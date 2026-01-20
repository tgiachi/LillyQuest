using ImGuiNET;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug entity that displays system and bootstrap timing information via ImGui.
/// Shows rendering times, update times, and per-system processing times.
/// </summary>
public class DebugSystemGameObject : GameEntity, IIMGuiEntity
{
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly ISystemManager _systemManager;

    public string Name => "Debug System Monitor";

    public DebugSystemGameObject(LillyQuestBootstrap bootstrap, ISystemManager systemManager)
    {
        _bootstrap = bootstrap;
        _systemManager = systemManager;
        IsActive = true;
    }

    /// <summary>
    /// Draws the ImGui debug panel with timing information.
    /// </summary>
    public void DrawIMGui()
    {
        // Bootstrap timing section
        if (ImGui.CollapsingHeader("Bootstrap Timings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text($"Update Time: {_bootstrap.UpdateTime.TotalMilliseconds:F2}ms");
            ImGui.Text($"Render Time: {_bootstrap.RenderTime.TotalMilliseconds:F2}ms");

            var totalTime = _bootstrap.UpdateTime.TotalMilliseconds + _bootstrap.RenderTime.TotalMilliseconds;
            ImGui.Text($"Total Time: {totalTime:F2}ms");
        }

        // System timings section
        if (ImGui.CollapsingHeader("System Timings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var totalSystemTime = 0.0;

            foreach (SystemQueryType queryType in Enum.GetValues<SystemQueryType>())
            {
                var processingTime = _systemManager.GetSystemProcessingTime(queryType);
                ImGui.Text($"{queryType}: {processingTime.TotalMilliseconds:F2}ms");
                totalSystemTime += processingTime.TotalMilliseconds;
            }

            ImGui.Separator();
            ImGui.Text($"Total System Time: {totalSystemTime:F2}ms");
        }

        ImGui.End();
    }
}

using ImGuiNET;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug entity that displays system and bootstrap timing information via ImGui.
/// Shows rendering times, update times, per-system processing times, and FPS.
/// </summary>
public class DebugSystemGameObject : GameEntity, IIMGuiEntity, IUpdateableEntity
{
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly ISystemManager _systemManager;

    private readonly Queue<double> _frameTimeSamples = new(120);
    private const int MaxFrameSamples = 120;
    private double _averageFrameTime;
    private double _currentFps;

    public string Name => "Debug System Monitor";

    public DebugSystemGameObject(LillyQuestBootstrap bootstrap, ISystemManager systemManager)
    {
        _bootstrap = bootstrap;
        _systemManager = systemManager;
        IsActive = true;
    }

    /// <summary>
    /// Updates FPS calculation based on frame time.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        var frameTime = gameTime.ElapsedGameTime.TotalMilliseconds;

        // Add new frame time sample
        _frameTimeSamples.Enqueue(frameTime);

        // Keep only recent samples
        if (_frameTimeSamples.Count > MaxFrameSamples)
        {
            _frameTimeSamples.Dequeue();
        }

        // Calculate average frame time
        _averageFrameTime = _frameTimeSamples.Count > 0 ? _frameTimeSamples.Average() : frameTime;

        // Calculate FPS from average frame time
        _currentFps = _averageFrameTime > 0 ? 1000.0 / _averageFrameTime : 0;
    }

    /// <summary>
    /// Draws the ImGui debug panel with timing information and FPS.
    /// </summary>
    public void DrawIMGui()
    {
        // FPS section at the top
        if (ImGui.CollapsingHeader("Performance", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text($"FPS: {_currentFps:F1}");
            ImGui.Text($"Frame Time: {_averageFrameTime:F2}ms (avg)");
            ImGui.ProgressBar((float)(_averageFrameTime / 16.67f), new System.Numerics.Vector2(-1, 0), "");
        }

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

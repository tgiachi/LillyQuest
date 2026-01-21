using ImGuiNET;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Systems;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug entity that displays current input state via ImGui.
/// Shows pressed keyboard keys, mouse position, and mouse button states in real-time.
/// </summary>
public class DebugInputGameObject : GameEntity, IIMGuiEntity
{
    private readonly InputSystem _inputSystem;

    public DebugInputGameObject(InputSystem inputSystem)
    {
        _inputSystem = inputSystem;
        IsActive = true;
        Name = "Debug Input State";
    }

    /// <summary>
    /// Draws the ImGui panel showing current input state.
    /// </summary>
    public void DrawIMGui()
    {
        // Mouse section
        ImGui.SeparatorText("Mouse");
        ImGui.Text($"Position: X={_inputSystem.MousePosition.X:F0}, Y={_inputSystem.MousePosition.Y:F0}");
        ImGui.Text($"Captured: {(_inputSystem.IsMouseCaptured ? "Yes" : "No")}");

        // Mouse buttons section
        var mouseButtons = _inputSystem.CurrentMouseButtons;
        ImGui.Text($"Buttons Pressed: {mouseButtons.Count}");

        if (mouseButtons.Count > 0)
        {
            ImGui.Indent();

            foreach (var button in mouseButtons)
            {
                ImGui.BulletText(button.ToString());
            }
            ImGui.Unindent();
        }

        // Mouse wheel section
        var scrollWheels = _inputSystem.ScrollWheels;

        if (scrollWheels.Count > 0)
        {
            ImGui.Text($"Scroll Wheels: {scrollWheels.Count}");
            ImGui.Indent();

            for (var i = 0; i < scrollWheels.Count; i++)
            {
                var wheel = scrollWheels[i];
                ImGui.BulletText($"Wheel {i}: X={wheel.X:F2}, Y={wheel.Y:F2}");
            }
            ImGui.Unindent();
        }

        ImGui.Spacing();

        // Keyboard section
        ImGui.SeparatorText("Keyboard");
        var pressedKeys = _inputSystem.CurrentKeys;
        ImGui.Text($"Keys Pressed: {pressedKeys.Count}");

        if (pressedKeys.Count > 0)
        {
            ImGui.Indent();

            foreach (var key in pressedKeys.OrderBy(k => k.ToString()))
            {
                ImGui.BulletText(key.ToString());
            }
            ImGui.Unindent();
        }
    }
}

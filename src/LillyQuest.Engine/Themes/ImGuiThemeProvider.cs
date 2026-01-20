using ImGuiNET;
using static ImGuiNET.ImGui;

namespace LillyQuest.Engine.Themes;

/// <summary>
/// Provides predefined ImGui themes and utilities for styling.
/// </summary>
public static class ImGuiThemeProvider
{
    /// <summary>
    /// Applies a classic theme (original ImGui colors).
    /// </summary>
    public static void ApplyClassicTheme()
    {
        StyleColorsClassic();
    }

    /// <summary>
    /// Applies the Cyan/Teal theme (dark mode with cyan accents).
    /// Based on the popular ImGui Cyan theme.
    /// </summary>
    public static void ApplyCyanTheme()
    {
        var style = GetStyle();

        // Style settings
        style.Alpha = 1.0f;
        style.ChildRounding = 3;
        style.WindowRounding = 3;
        style.GrabRounding = 1;
        style.GrabMinSize = 20;
        style.FrameRounding = 3;

        // Colors
        style.Colors[(int)ImGuiCol.Text] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new(0.00f, 0.40f, 0.41f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new(0.00f, 0.00f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.Border] = new(0.00f, 1.00f, 1.00f, 0.65f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new(0.44f, 0.80f, 0.80f, 0.18f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.44f, 0.80f, 0.80f, 0.27f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new(0.44f, 0.81f, 0.86f, 0.66f);
        style.Colors[(int)ImGuiCol.TitleBg] = new(0.14f, 0.18f, 0.21f, 0.73f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.00f, 0.00f, 0.00f, 0.54f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new(0.00f, 1.00f, 1.00f, 0.27f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new(0.00f, 0.00f, 0.00f, 0.20f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new(0.22f, 0.29f, 0.30f, 0.71f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new(0.00f, 1.00f, 1.00f, 0.44f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(0.00f, 1.00f, 1.00f, 0.74f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new(0.00f, 1.00f, 1.00f, 0.68f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new(0.00f, 1.00f, 1.00f, 0.36f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new(0.00f, 1.00f, 1.00f, 0.76f);
        style.Colors[(int)ImGuiCol.Button] = new(0.00f, 0.65f, 0.65f, 0.46f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new(0.01f, 1.00f, 1.00f, 0.43f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new(0.00f, 1.00f, 1.00f, 0.62f);
        style.Colors[(int)ImGuiCol.Header] = new(0.00f, 1.00f, 1.00f, 0.33f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new(0.00f, 1.00f, 1.00f, 0.42f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new(0.00f, 1.00f, 1.00f, 0.54f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new(0.00f, 1.00f, 1.00f, 0.54f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new(0.00f, 1.00f, 1.00f, 0.74f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new(0.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new(0.00f, 1.00f, 1.00f, 0.22f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.04f, 0.10f, 0.09f, 0.51f);
        style.Colors[(int)ImGuiCol.Tab] = new(0.00f, 0.50f, 0.50f, 0.33f);
        style.Colors[(int)ImGuiCol.TabHovered] = new(0.00f, 1.00f, 1.00f, 0.42f);
        style.Colors[(int)ImGuiCol.TabActive] = new(0.00f, 1.00f, 1.00f, 0.54f);
    }

    /// <summary>
    /// Applies a Dark Fantasy theme with gold, purple and mystical blue accents.
    /// Evokes a medieval fantasy atmosphere with rich jewel tones.
    /// </summary>
    public static void ApplyDarkFantasyTheme()
    {
        var style = GetStyle();

        // Style settings - rounded for fantasy feel
        style.Alpha = 1.0f;
        style.ChildRounding = 4;
        style.WindowRounding = 4;
        style.GrabRounding = 2;
        style.GrabMinSize = 20;
        style.FrameRounding = 4;
        style.PopupRounding = 4;
        style.TabRounding = 4;

        // Colors - Dark Fantasy Palette
        // Gold: #D4AF37 (212, 175, 55)
        // Purple: #6B2D5C (107, 45, 92)
        // Dark Blue: #0F1419 (15, 20, 25)
        // Mystical Blue: #4A7C9E (74, 124, 158)

        style.Colors[(int)ImGuiCol.Text] = new(0.85f, 0.85f, 0.85f, 1.00f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new(0.50f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new(0.10f, 0.10f, 0.15f, 1.00f);
        style.Colors[(int)ImGuiCol.ChildBg] = new(0.12f, 0.12f, 0.18f, 0.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new(0.08f, 0.08f, 0.12f, 0.95f);
        style.Colors[(int)ImGuiCol.Border] = new(0.42f, 0.26f, 0.36f, 0.65f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new(0.20f, 0.15f, 0.25f, 0.54f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.30f, 0.20f, 0.35f, 0.66f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new(0.42f, 0.26f, 0.36f, 0.80f);
        style.Colors[(int)ImGuiCol.TitleBg] = new(0.08f, 0.06f, 0.10f, 1.00f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new(0.42f, 0.26f, 0.36f, 0.87f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.08f, 0.08f, 0.12f, 0.75f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new(0.10f, 0.10f, 0.15f, 0.40f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new(0.12f, 0.12f, 0.18f, 0.50f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new(0.83f, 0.69f, 0.22f, 0.44f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(0.83f, 0.69f, 0.22f, 0.74f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.CheckMark] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new(0.83f, 0.69f, 0.22f, 0.50f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.Button] = new(0.42f, 0.26f, 0.36f, 0.60f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new(0.42f, 0.26f, 0.36f, 0.80f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new(0.83f, 0.69f, 0.22f, 0.60f);
        style.Colors[(int)ImGuiCol.Header] = new(0.42f, 0.26f, 0.36f, 0.45f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new(0.42f, 0.26f, 0.36f, 0.80f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new(0.83f, 0.69f, 0.22f, 0.50f);
        style.Colors[(int)ImGuiCol.Separator] = new(0.42f, 0.26f, 0.36f, 0.50f);
        style.Colors[(int)ImGuiCol.SeparatorHovered] = new(0.42f, 0.26f, 0.36f, 0.78f);
        style.Colors[(int)ImGuiCol.SeparatorActive] = new(0.83f, 0.69f, 0.22f, 0.75f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new(0.83f, 0.69f, 0.22f, 0.45f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new(0.83f, 0.69f, 0.22f, 0.78f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.Tab] = new(0.42f, 0.26f, 0.36f, 0.86f);
        style.Colors[(int)ImGuiCol.TabHovered] = new(0.42f, 0.26f, 0.36f, 0.80f);
        style.Colors[(int)ImGuiCol.TabActive] = new(0.83f, 0.69f, 0.22f, 0.50f);
        style.Colors[(int)ImGuiCol.TabUnfocused] = new(0.20f, 0.20f, 0.20f, 0.97f);
        style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new(0.40f, 0.40f, 0.40f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLines] = new(0.29f, 0.49f, 0.62f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new(0.75f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new(1.00f, 0.88f, 0.40f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new(0.42f, 0.26f, 0.36f, 0.35f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new(0.83f, 0.69f, 0.22f, 0.90f);
        style.Colors[(int)ImGuiCol.NavHighlight] = new(0.83f, 0.69f, 0.22f, 1.00f);
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new(0.83f, 0.69f, 0.22f, 0.70f);
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.20f, 0.20f, 0.20f, 0.20f);
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.20f, 0.20f, 0.20f, 0.35f);
    }

    /// <summary>
    /// Applies a dark theme with blue accents (default ImGui style).
    /// </summary>
    public static void ApplyDarkTheme()
    {
        StyleColorsDark();
    }

    /// <summary>
    /// Applies a classic light theme.
    /// </summary>
    public static void ApplyLightTheme()
    {
        StyleColorsLight();
    }
}

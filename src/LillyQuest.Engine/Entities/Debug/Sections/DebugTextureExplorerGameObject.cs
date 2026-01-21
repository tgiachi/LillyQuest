using ImGuiNET;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug object that displays all loaded textures in memory with their properties.
/// </summary>
public class DebugTextureExplorerGameObject : GameEntity, IIMGuiEntity
{
    private readonly ITextureManager _textureManager;

    public DebugTextureExplorerGameObject(ITextureManager textureManager)
    {
        _textureManager = textureManager;
        IsActive = true;
        Name = "Texture Explorer";
    }

    public void DrawIMGui()
    {
        ImGui.Text("Loaded Textures:");
        ImGui.Separator();

        var textures = _textureManager.GetAllTextures();
        ImGui.Text($"Total Textures: {textures.Count}");
        ImGui.Spacing();

        if (textures.Count == 0)
        {
            ImGui.TextDisabled("No textures loaded");

            return;
        }

        // Calculate total memory usage
        long totalMemoryBytes = 0;

        foreach (var texture in textures.Values)
        {
            // RGBA = 4 bytes per pixel
            totalMemoryBytes += (long)texture.Width * texture.Height * 4;
        }

        ImGui.Text($"Total Memory: {FormatBytes(totalMemoryBytes)}");
        ImGui.Spacing();

        // Display all textures with preview
        if (ImGui.BeginChild("TexturesChild", new(0, 400)))
        {
            foreach (var (name, texture) in textures)
            {
                var textureMemory = (long)texture.Width * texture.Height * 4;

                // Texture item
                ImGui.Text($"{name}");
                ImGui.Text($"Size: {texture.Width}x{texture.Height}px | Memory: {FormatBytes(textureMemory)}");

                // Preview thumbnail
                var thumbSize = 64.0f;
                var aspectRatio = (float)texture.Width / texture.Height;
                var thumbWidth = thumbSize * aspectRatio;
                var thumbHeight = thumbSize;

                // Clamp to max size
                if (thumbWidth > 200)
                {
                    thumbWidth = 200;
                    thumbHeight = 200 / aspectRatio;
                }

                // Convert OpenGL texture handle to IntPtr for ImGui
                IntPtr texturePtr = new((int)texture.Handle);

                // ImGui expects texture coordinates with Y flipped for OpenGL
                ImGui.Image(
                    texturePtr,
                    new(thumbWidth, thumbHeight),
                    new(0, 1),                  // UV0 - bottom left (OpenGL coords)
                    new(1, 0),                  // UV1 - top right (OpenGL coords)
                    new(1, 1, 1, 1),            // Tint white
                    new(0.7f, 0.7f, 0.7f, 1.0f) // Border gray
                );

                ImGui.Separator();
            }

            ImGui.EndChild();
        }
    }

    /// <summary>
    /// Formats bytes to human-readable format (B, KB, MB, GB).
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}

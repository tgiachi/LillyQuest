using ImGuiNET;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities.Debug;

/// <summary>
/// Debug object that displays all loaded tilesets in memory with their properties and preview.
/// </summary>
public class DebugTileExplorerGameObject : GameEntity, IIMGuiEntity
{
    private readonly ITilesetManager _tilesetManager;

    public DebugTileExplorerGameObject(ITilesetManager tilesetManager)
    {
        _tilesetManager = tilesetManager;
        IsActive = true;
        Name = "Tile Explorer";
    }

    public void DrawIMGui()
    {
        ImGui.Text("Loaded Tilesets:");
        ImGui.Separator();

        var tilesets = _tilesetManager.GetAllTilesets();
        ImGui.Text($"Total Tilesets: {tilesets.Count}");
        ImGui.Spacing();

        if (tilesets.Count == 0)
        {
            ImGui.TextDisabled("No tilesets loaded");

            return;
        }

        // Display all tilesets with details
        if (ImGui.BeginChild("TilesetsChild", new(0, 400)))
        {
            foreach (var (name, tileset) in tilesets)
            {
                DrawTilesetItem(name, tileset);
            }

            ImGui.EndChild();
        }
    }

    private void DrawFullPreview(Tileset tileset)
    {
        var thumbSize = 128.0f;
        var aspectRatio = (float)tileset.Texture.Width / tileset.Texture.Height;
        var thumbWidth = thumbSize * aspectRatio;
        var thumbHeight = thumbSize;

        // Clamp to max size
        if (thumbWidth > 250)
        {
            thumbWidth = 250;
            thumbHeight = 250 / aspectRatio;
        }

        // Convert OpenGL texture handle to IntPtr for ImGui
        IntPtr texturePtr = new((int)tileset.Texture.Handle);

        // ImGui expects texture coordinates with Y flipped for OpenGL
        ImGui.Image(
            texturePtr,
            new(thumbWidth, thumbHeight),
            new(0, 1),                  // UV0 - bottom left (OpenGL coords)
            new(1, 0),                  // UV1 - top right (OpenGL coords)
            new(1, 1, 1, 1),            // Tint white
            new(0.7f, 0.7f, 0.7f, 1.0f) // Border gray
        );
    }

    private void DrawTilesetItem(string name, Tileset tileset)
    {
        if (ImGui.TreeNode($"{name}##tileset_{name}"))
        {
            ImGui.Text($"Image Path: {tileset.ImagePath}");

            // Grid info
            ImGui.Text($"Tile Size: {tileset.TileWidth}x{tileset.TileHeight}px");
            ImGui.Text(
                $"Grid: {tileset.TilesPerRow} cols x {tileset.TilesPerColumn} rows = {tileset.TileCount} total tiles"
            );
            ImGui.Text($"Spacing: {tileset.Spacing}px | Margin: {tileset.Margin}px");
            ImGui.Text($"Texture: {tileset.Texture.Width}x{tileset.Texture.Height}px");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Full preview
            if (ImGui.TreeNode("Full Preview##preview_" + name))
            {
                DrawFullPreview(tileset);
                ImGui.TreePop();
            }

            ImGui.Spacing();

            // Individual tiles grid
            if (ImGui.TreeNode("Individual Tiles##tiles_" + name))
            {
                DrawTilesGrid(tileset);
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        ImGui.Spacing();
    }

    private void DrawTilesGrid(Tileset tileset)
    {
        var tileDisplaySize = 64.0f;
        var tableColumns = tileset.TilesPerRow;

        ImGui.Text($"Showing {tileset.TileCount} tiles ({tileset.TilesPerColumn} rows x {tileset.TilesPerRow} cols):");
        ImGui.Spacing();

        IntPtr texturePtr = new((int)tileset.Texture.Handle);
        var textureWidth = tileset.Texture.Width;
        var textureHeight = tileset.Texture.Height;

        // Use ImGui table for proper grid layout
        if (ImGui.BeginTable("TilesTable", tableColumns, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, tileDisplaySize + 20);

            for (var col = 1; col < tableColumns; col++)
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, tileDisplaySize + 20);
            }

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i % tableColumns == 0)
                {
                    ImGui.TableNextRow();
                }
                ImGui.TableNextColumn();

                var tileData = tileset.GetTile(i);

                // Calculate UV coordinates from source rect
                var uvX0 = (float)tileData.SourceRect.Origin.X / textureWidth;
                var uvY0 = (float)tileData.SourceRect.Origin.Y / textureHeight;
                var uvX1 = (float)(tileData.SourceRect.Origin.X + tileData.SourceRect.Size.X) / textureWidth;
                var uvY1 = (float)(tileData.SourceRect.Origin.Y + tileData.SourceRect.Size.Y) / textureHeight;

                // Draw tile (don't flip UV - use normal coordinates)
                ImGui.Image(
                    texturePtr,
                    new(tileDisplaySize, tileDisplaySize),
                    new(uvX0, uvY0), // UV0 - top left
                    new(uvX1, uvY1), // UV1 - bottom right
                    new(1, 1, 1, 1),
                    new(0.5f, 0.5f, 0.5f, 1.0f)
                );

                // Show tooltip with tile info
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        $"Index: {i}\nGrid: ({tileData.TileX}, {tileData.TileY})\nSourceRect: ({tileData.SourceRect.Origin.X}, {tileData.SourceRect.Origin.Y})"
                    );
                }

                // Add index label centered below tile
                var textWidth = ImGui.CalcTextSize(i.ToString()).X;
                var availWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (availWidth - textWidth) * 0.5f);
                ImGui.Text(i.ToString());
            }

            ImGui.EndTable();
        }
    }
}

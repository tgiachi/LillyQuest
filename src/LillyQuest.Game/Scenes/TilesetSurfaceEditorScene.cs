using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Screens.Base;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Game.Screens.TilesetSurface;

namespace LillyQuest.Game.Scenes;

/// <summary>
/// Scene for the tileset surface editor.
/// Demonstrates TilesetSurfaceScreen with random tiles and colors.
/// </summary>
public class TilesetSurfaceEditorScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly ITilesetManager _tilesetManager;

    public TilesetSurfaceEditorScene(IScreenManager screenManager, ITilesetManager tilesetManager)
        : base("tileset_surface_editor")
    {
        _screenManager = screenManager;
        _tilesetManager = tilesetManager;
    }

    public override void OnInitialize(ISceneManager sceneManager)
    {
        // Create the tileset surface screen
        var screen = new TilesetSurfaceScreen(_tilesetManager)
        {
            DefaultTilesetName = "alloy",
            LayerCount = 3
        };

        // Initialize the surface layers before population
        screen.InitializeLayers(screen.LayerCount);

        // Populate with random tiles and colors
        PopulateWithRandomTiles(screen);

        // Add the screen to the screen manager
        _screenManager.PushScreen(screen);

        base.OnInitialize(sceneManager);
    }

    /// <summary>
    /// Populates the tileset surface with random tiles and colors.
    /// </summary>
    private void PopulateWithRandomTiles(TilesetSurfaceScreen screen)
    {
        var tileset = _tilesetManager.GetTileset(screen.DefaultTilesetName);
        if (tileset == null)
        {
            return;
        }

        var random = Random.Shared;
        const int tilesToPlace = 150; // Number of random tiles to place

        for (var i = 0; i < tilesToPlace; i++)
        {
            // Random position
            var x = random.Next(0, 50);
            var y = random.Next(0, 50);

            // Random layer
            var layerIndex = random.Next(0, screen.LayerCount);

            // Random tile index
            var tileIndex = random.Next(0, tileset.TileCount);

            // Random foreground color
            var foregroundColor = new LyColor(
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256),
                255
            );

            // Random background color (30% chance)
            LyColor? backgroundColor = null;
            if (random.Next(100) < 30)
            {
                backgroundColor = new LyColor(
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    200
                );
            }

            // Create and place the tile
            var tileData = new TileRenderData(tileIndex, foregroundColor,null);

            // Set the current layer and add the tile
            screen.SelectedLayerIndex = layerIndex;
            screen.AddTileToSurface(x, y, tileData);
        }

        // Set opacity variations for layers
        screen.SetLayerOpacity(0, 1.0f);
        screen.SetLayerOpacity(1, 0.8f);
        screen.SetLayerOpacity(2, 0.6f);
    }
}

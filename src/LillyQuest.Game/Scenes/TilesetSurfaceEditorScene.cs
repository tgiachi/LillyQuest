using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Extensions.TilesetSurface;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.TilesetSurface;
using Silk.NET.Input;

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
            DefaultTilesetName = "roguelike",
            LayerCount = 2,
            Position = new Vector2(100, 30),
            TileRenderScale = 1f,
            TileViewSize = new Vector2(20, 20),
        };

        screen.TileMouseMoveAllLayers += (index, x, y, mouseX, mouseY) =>
                                         {
                                             if (index != 0)
                                             {
                                                 return;
                                             }
                                             screen.ClearLayer(1);
                                             screen.DrawTextPixel(
                                                 1,
                                                 $"Tile X: {x}, Tile Y: {y}       ",
                                                 mouseX,
                                                 mouseY,
                                                 LyColor.White,
                                                 LyColor.Black
                                             );
                                         };
        screen.TileMouseDown += (layerIndex, tileX, tileY, buttons) =>
                                {
                                    if (buttons.Contains(MouseButton.Right))
                                    {
                                        screen.CenterViewOnTile(0, tileX, tileY);
                                    }
                                };

        // Initialize the surface layers before population
        screen.InitializeLayers(screen.LayerCount);
        screen.SetLayerTileset(0, "roguelike");
        screen.SetLayerTileset(1, "alloy");

        screen.SetLayerViewSmoothing(0, true);

        // Populate with random tiles and colors
        PopulateWithRandomTiles(screen);

        screen.SelectedLayerIndex = 0;
        screen.DrawText(1, "hello from LillyQuest *", 2, 2, LyColor.Yellow, LyColor.Black);

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
        screen.SelectedLayerIndex = 0;

        for (var x = 0; x < 100; x++)
        {
            for (var y = 0; y < 100; y++)
            {
                // Random tile index
                var tileIndex = random.Next(0, tileset.TileCount);

                // Random foreground color
                var foregroundColor = new LyColor(
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256)
                );

                // // Random background color (30% chance)
                LyColor? backgroundColor = null;

                if (random.Next(100) < 30)
                {
                    backgroundColor = new LyColor(
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256)
                    );
                }

                // backgroundColor = LyColor.Blue;

                var rotation = Enum.GetValues<TileFlipType>();
                var flip = rotation[random.Next(0, rotation.Length)];

                // Create and place the tile
                var tileData = new TileRenderData(tileIndex, foregroundColor, backgroundColor, flip);
                screen.AddTileToSurface(x, y, tileData);
            }
        }

        // Set opacity variations for layers
        screen.SetLayerOpacity(0, 1.0f);
        screen.SetLayerOpacity(1, 1.0f);
    }
}

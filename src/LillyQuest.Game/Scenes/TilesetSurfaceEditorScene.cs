using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Extensions.TilesetSurface;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.TilesetSurface;
using LillyQuest.Engine.Screens.UI;
using Serilog;
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
                                                 $"Tile X: {x}, Tile Y: {y}",
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

                                    if (buttons.Contains(MouseButton.Middle))
                                    {
                                        foreach (var i in Enumerable.Range(0, 1000))
                                        {
                                            var randX = Random.Shared.Next(0, 100);
                                            var randY = Random.Shared.Next(0, 100);

                                            var randomDenstinationVector = new Vector2(randX + Random.Shared.Next(-1, 2), randY + Random.Shared.Next(-1, 2));

                                            screen.EnqueueMove(0, new Vector2((float)randX,(float)randY), randomDenstinationVector, 0.2f, true);

                                        }

                                    }
                                };

        screen.TileMouseWheel += (layerIndex, x, y, delta) =>
                                 {
                                     var point = 0.1f;

                                     if (delta < 0)
                                     {
                                         delta = (point * -1);
                                     }
                                     else
                                     {
                                         delta = point;
                                     }

                                     var scale = screen.GetLayerRenderScale(0);

                                     if (scale + delta > 0)
                                     {
                                         screen.SetLayerRenderScaleTarget(0, scale + delta, 1f);

                                         // screen.CenterViewOnTile(0, x, y);
                                         Log.Logger.Information("Scale adjusted to {Scale}", scale + delta);
                                     }
                                 };

        // Initialize the surface layers before population
        screen.InitializeLayers(screen.LayerCount);
        screen.SetLayerTileset(0, "roguelike");
        screen.SetLayerTileset(1, "alloy");

        screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);

        screen.SetLayerViewSmoothing(0, true);

        // Populate with random tiles and colors
        PopulateWithRandomTiles(screen);

        screen.SelectedLayerIndex = 0;
        screen.DrawText(1, "hello from LillyQuest *", 2, 2, LyColor.Yellow, LyColor.Black);


        // Add the screen to the screen manager
        _screenManager.PushScreen(screen);

        var uiOverlay = new UIScreenOverlay
        {
            Position = Vector2.Zero,
            Size = new Vector2(1600, 900)
        };

        var label = new UILabel
        {
            Text = "UI Label",
            Position = new Vector2(20, 20),
            Color = LyColor.Yellow,
            ZIndex = 0
        };
        uiOverlay.Root.Add(label);

        var window = new UIWindow
        {
            Position = new Vector2(20, 50),
            Size = new Vector2(240, 120),
            Title = "UI Window",
            IsTitleBarEnabled = true,
            IsWindowMovable = true,
            BackgroundColor = LyColor.Black,
            BackgroundAlpha = 0.6f,
            BorderColor = LyColor.White,
            ZIndex = 1
        };

        var windowLabel = new UILabel
        {
            Text = "Hello from UIWindow",
            Position = new Vector2(8, 26),
            Color = LyColor.White
        };
        window.Add(windowLabel);
        uiOverlay.Root.Add(window);

        // var uiTile = new UITileSurfaceControl(_tilesetManager, 20, 5)
        // {
        //     Position = new Vector2(20, 50),
        //     Size = new Vector2(10, 3),
        //     ZIndex = 1
        // };
        // uiTile.Surface.DefaultTilesetName = "alloy";
        // uiTile.Surface.SetLayerTileset(0, "alloy");
        // uiTile.Surface.DrawText(0, "UI Tiles", 1, 1, LyColor.White, LyColor.Black);
        // uiOverlay.Root.Add(uiTile);

        _screenManager.PushScreen(uiOverlay);

        base.OnInitialize(sceneManager);
    }

    /// <summary>
    /// Populates the tileset surface with random tiles and colors.
    /// </summary>
    private void PopulateWithRandomTiles(TilesetSurfaceScreen screen)
    {
        var tileset = _tilesetManager.GetTileset(screen.DefaultTilesetName);

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

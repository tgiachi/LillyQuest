using System.IO;
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
using Silk.NET.Maths;

namespace LillyQuest.Game.Scenes;

/// <summary>
/// Scene for the tileset surface editor.
/// Demonstrates TilesetSurfaceScreen with random tiles and colors.
/// </summary>
public class TilesetSurfaceEditorScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly ITilesetManager _tilesetManager;
    private readonly ITextureManager _textureManager;
    private readonly INineSliceAssetManager _nineSliceManager;

    public TilesetSurfaceEditorScene(
        IScreenManager screenManager,
        ITilesetManager tilesetManager,
        ITextureManager textureManager,
        INineSliceAssetManager nineSliceManager
    )
        : base("tileset_surface_editor")
    {
        _screenManager = screenManager;
        _tilesetManager = tilesetManager;
        _textureManager = textureManager;
        _nineSliceManager = nineSliceManager;
    }

    public override void OnInitialize(ISceneManager sceneManager)
    {
        // Create the tileset surface screen
        var screen = new TilesetSurfaceScreen(_tilesetManager)
        {
            DefaultTilesetName = "roguelike",
            LayerCount = 2,
            Position = new(100, 30),
            TileRenderScale = 1f,
            TileViewSize = new(20, 20)
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

                                            var randomDenstinationVector = new Vector2(
                                                randX + Random.Shared.Next(-1, 2),
                                                randY + Random.Shared.Next(-1, 2)
                                            );

                                            screen.EnqueueMove(0, new(randX, randY), randomDenstinationVector, 0.2f, true);
                                        }
                                    }
                                };

        screen.TileMouseWheel += (layerIndex, x, y, delta) =>
                                 {
                                     var point = 0.1f;

                                     if (delta < 0)
                                     {
                                         delta = point * -1;
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

        var uiRoot = new UIRootScreen
        {
            Position = Vector2.Zero,
            Size = new(1600, 900)
        };

        var label = new UILabel
        {
            Text = "UI Label",
            Position = new(20, 200),
            Color = LyColor.Yellow,
            ZIndex = 0
        };
        uiRoot.Root.Add(label);

        var window = new UIWindow
        {
            Position = new(100, 90),
            Size = new(240, 120),
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
            Position = new(8, 26),
            Color = LyColor.White
        };
        window.Add(windowLabel);
        uiRoot.Root.Add(window);

        var staticWindow = new UIWindow
        {
            Position = new(280, 50),
            Size = new(220, 90),
            Title = "Static Window",
            IsTitleBarEnabled = false,
            IsWindowMovable = false,
            BackgroundColor = LyColor.Black,
            BackgroundAlpha = 0.4f,
            BorderColor = LyColor.White,
            ZIndex = 2
        };

        var staticLabel = new UILabel
        {
            Text = "Non-movable",
            Position = new(8, 10),
            Color = LyColor.White
        };
        staticWindow.Add(staticLabel);
        uiRoot.Root.Add(staticWindow);

        var nineSliceTextureName = "lillyquest_cover";
        var nineSliceKey = "lillyquest_cover_slice";
        if (!_textureManager.HasTexture(nineSliceTextureName))
        {
            var texturePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "lillyquest_cover.jpg");
            if (File.Exists(texturePath))
            {
                _textureManager.LoadTexture(nineSliceTextureName, texturePath);
            }
        }

        if (_textureManager.TryGetTexture(nineSliceTextureName, out var nineSliceTexture))
        {
            if (!_nineSliceManager.TryGetNineSlice(nineSliceKey, out _))
            {
                _nineSliceManager.RegisterNineSlice(
                    nineSliceKey,
                    nineSliceTextureName,
                    new Rectangle<int>(0, 0, (int)nineSliceTexture.Width, (int)nineSliceTexture.Height),
                    new Vector4D<float>(64f, 64f, 64f, 64f)
                );
            }

            var nineSliceWindow = new UINinePatchWindow(_nineSliceManager, _textureManager)
            {
                Position = new(520, 240),
                Size = new(420, 240),
                Title = "Nine-Slice Window",
                TitleFontName = "default_font",
                TitleFontSize = 16,
                TitleMargin = new Vector4D<float>(20f, 12f, 0f, 0f),
                ContentMargin = new Vector4D<float>(20f, 40f, 0f, 0f),
                NineSliceScale = 1f,
                NineSliceKey = nineSliceKey,
                ZIndex = 3
            };

            var nineSliceLabel = new UILabel
            {
                Text = "Nine-slice content",
                Position = new(0, 0),
                Color = LyColor.White
            };
            nineSliceWindow.Add(nineSliceLabel);
            uiRoot.Root.Add(nineSliceWindow);
        }

        // var uiTile = new UITileSurfaceControl(_tilesetManager, 20, 5)
        // {
        //     Position = new Vector2(20, 50),
        //     Size = new Vector2(10, 3),
        //     ZIndex = 1
        // };
        // uiTile.Surface.DefaultTilesetName = "alloy";
        // uiTile.Surface.SetLayerTileset(0, "alloy");
        // uiTile.Surface.DrawText(0, "UI Tiles", 1, 1, LyColor.White, LyColor.Black);
        // uiRoot.Root.Add(uiTile);

        _screenManager.PushScreen(uiRoot);

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

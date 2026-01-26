using System.Numerics;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Engine;
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
    private readonly ITextureManager _textureManager;
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly LillyQuestBootstrap _bootstrap;
    private readonly EngineRenderContext _renderContext;

    private UIRootScreen? _uiRoot;
    private TilesetSurfaceScreen? _screen;
    private bool _subscribed;

    public TilesetSurfaceEditorScene(
        IScreenManager screenManager,
        ITilesetManager tilesetManager,
        ITextureManager textureManager,
        INineSliceAssetManager nineSliceManager,
        LillyQuestBootstrap bootstrap,
        EngineRenderContext renderContext
    )
        : base("tileset_surface_editor")
    {
        _screenManager = screenManager;
        _tilesetManager = tilesetManager;
        _textureManager = textureManager;
        _nineSliceManager = nineSliceManager;
        _bootstrap = bootstrap;
        _renderContext = renderContext;
    }

    public override void OnLoad()
    {
        _uiRoot = new UIRootScreen
        {
            Position = Vector2.Zero,
            Size = new(1600, 900)
        };

        // Create the tileset surface screen
        _screen = new TilesetSurfaceScreen(_tilesetManager)
        {
            DefaultTilesetName = "roguelike",
            LayerCount = 2,
            Position = new(100, 30),
            TileRenderScale = 1f,
            TileViewSize = new(80, 30)
        };

        var windowSize = _renderContext.Window != null
                             ? new Vector2(_renderContext.Window.Size.X, _renderContext.Window.Size.Y)
                             : new Vector2(1280, 720);

        var availableSize = new Vector2(
            MathF.Max(0f, windowSize.X - _screen.Position.X),
            MathF.Max(0f, windowSize.Y - _screen.Position.Y)
        );

        _screen.ApplyTileViewScaleToScreen(availableSize, includeMargins: true);


        Log.Logger.Information(
            "TilesetSurfaceScreen state: Size={Size}, TileViewSize={TileViewSize}, Layer0Scale={Scale}",
            _screen.Size,
            _screen.TileViewSize,
            _screen.GetLayerRenderScale(0)
        );

        _screen.TileMouseMoveAllLayers += (index, x, y, mouseX, mouseY) =>
                                          {
                                              if (index != 0)
                                              {
                                                  return;
                                              }
                                              _screen.ClearLayer(1);
                                              _screen.DrawTextPixel(
                                                  1,
                                                  $"Tile X: {x}, Tile Y: {y}",
                                                  mouseX,
                                                  mouseY,
                                                  LyColor.White,
                                                  LyColor.Black
                                              );
                                          };
        _screen.TileMouseDown += (layerIndex, tileX, tileY, buttons) =>
                                 {
                                     if (buttons.Contains(MouseButton.Right))
                                     {
                                         _screen.CenterViewOnTile(0, tileX, tileY);
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

                                             _screen.EnqueueMove(0, new(randX, randY), randomDenstinationVector, 0.2f, true);
                                         }
                                     }
                                 };

        _screen.TileMouseWheel += (layerIndex, x, y, delta) =>
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

                                      var scale = _screen.GetLayerRenderScale(0);

                                      if (scale + delta > 0)
                                      {
                                          _screen.SetLayerRenderScaleTarget(0, scale + delta, 1f);

                                          // screen.CenterViewOnTile(0, x, y);
                                          Log.Logger.Information("Scale adjusted to {Scale}", scale + delta);
                                      }
                                  };

        // Initialize the surface layers before population
        _screen.InitializeLayers(_screen.LayerCount);
        _screen.SetLayerTileset(0, "alloy");
        _screen.SetLayerTileset(1, "alloy");

        _screen.SetLayerRenderScaleSmoothing(0, true, 0.1f);

        _screen.SetLayerViewSmoothing(0, true);

        // Populate with random tiles and colors
        PopulateWithRandomTiles(_screen);

        _screen.SelectedLayerIndex = 0;
        _screen.DrawText(1, "hello from LillyQuest *", 2, 2, LyColor.Yellow, LyColor.Black);

        // Add the screen to the screen manager
        _screenManager.PushScreen(_screen);

        if (!_subscribed)
        {
            _bootstrap.WindowResize += OnWindowResize;
            _subscribed = true;
        }

        var label = new UILabel
        {
            Text = "UI Label",
            Position = new(20, 200),
            Color = LyColor.Yellow,
            ZIndex = 0
        };
        _uiRoot.Root.Add(label);

        var window = new UIWindow
        {
            Position = new(100, 90),
            Size = new(240, 120),
            Title = "UI Window",
            IsTitleBarEnabled = true,
            IsWindowMovable = true,
            BackgroundColor = LyColor.Black,
            BackgroundAlpha = 0.9f,
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
        _uiRoot.Root.Add(window);

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
        _uiRoot.Root.Add(staticWindow);

        var nineSliceWindow = new UINinePatchWindow(_nineSliceManager, _textureManager)
        {
            Position = new(520, 240),
            Size = new(420, 240),
            Title = "Nine-Slice Window",
            TitleFont = new("default_font", 16, FontKind.TrueType),
            TitleMargin = new(20f, 12f, 0f, 0f),
            ContentMargin = new(20f, 40f, 0f, 0f),
            NineSliceScale = 1f,
            CenterTint = LyColor.FromHex("#e8d7b0"),
            BorderTint = LyColor.FromHex("#a67c52"),
            NineSliceKey = "simple_ui",
            ZIndex = 3
        };

        var nineSliceLabel = new UILabel
        {
            Text = "Nine-slice content",
            Position = new(0, 0),
            Color = LyColor.White
        };
        nineSliceWindow.Add(nineSliceLabel);
        _uiRoot.Root.Add(nineSliceWindow);

        var buttonWindow = new UINinePatchWindow(_nineSliceManager, _textureManager)
        {
            Position = new(100, 260),
            Size = new(260, 160),
            Title = "UIButton Demo",
            TitleFont = new("default_font", 16, FontKind.TrueType),
            TitleMargin = new(20f, 12f, 0f, 0f),
            ContentMargin = new(20f, 40f, 0f, 0f),
            NineSliceScale = 1f,
            CenterTint = LyColor.FromHex("#e8d7b0"),
            BorderTint = LyColor.FromHex("#a67c52"),
            NineSliceKey = "simple_ui",
            ZIndex = 4
        };

        var sampleButton = new UIButton(_nineSliceManager, _textureManager)
        {
            Position = new(20, 10),
            Size = new(180, 48),
            Text = "Click Me",
            Font = new("default_font", 14, FontKind.TrueType),
            TextColor = LyColor.Black,

            NineSliceKey = "simple_ui",
            IdleTint = new(200, 200, 200, 255),
            HoveredTint = new(255, 255, 255, 255),
            PressedTint = new(160, 160, 160, 255),
            TransitionTime = 0.2f
        };
        sampleButton.OnClick = () => Log.Logger.Information("UIButton clicked");

        buttonWindow.Add(sampleButton);
        _uiRoot.Root.Add(buttonWindow);

        var scrollLabel = new UILabel
        {
            Text = "UIScrollContent Demo",
            Position = new(520, 40),
            Color = LyColor.White,
            ZIndex = 10
        };
        _uiRoot.Root.Add(scrollLabel);

        var scrollContent = new UIScrollContent(_nineSliceManager, _textureManager)
        {
            Position = new(520, 70),
            Size = new(260, 160),
            ContentSize = new(600, 420),
            ScrollbarTextureName = "n9_ui_simple_ui",
            ScrollbarTint = LyColor.FromHex("#c8b27a"),
            ScrollSpeed = 24f,
            ZIndex = 10
        };

        for (var row = 0; row < 6; row++)
        {
            for (var col = 0; col < 5; col++)
            {
                var entry = new UILabel
                {
                    Text = $"Item {col},{row}",
                    Position = new(12 + col * 110, 12 + row * 60),
                    Color = LyColor.Yellow
                };
                scrollContent.Add(entry);
            }
        }

        _uiRoot.Root.Add(scrollContent);

        _screenManager.PushScreen(_uiRoot);
        base.OnLoad();
    }

    public override void OnUnload()
    {
        if (_screen != null)
        {
            _screenManager.PopScreen(_screen);
            _screen = null;
        }

        if (_uiRoot != null)
        {
            _screenManager.PopScreen(_uiRoot);
            _uiRoot = null;
        }

        if (_subscribed)
        {
            _bootstrap.WindowResize -= OnWindowResize;
            _subscribed = false;
        }

        base.OnUnload();
    }

    private void OnWindowResize(Vector2 size)
    {
        if (_screen == null)
        {
            return;
        }

        var availableSize = new Vector2(
            MathF.Max(0f, size.X - _screen.Position.X),
            MathF.Max(0f, size.Y - _screen.Position.Y)
        );

        _screen.ApplyTileViewScaleToScreen(availableSize, includeMargins: true);

        Log.Logger.Information(
            "TilesetSurfaceScreen state: Size={Size}, TileViewSize={TileViewSize}, Layer0Scale={Scale}",
            _screen.Size,
            _screen.TileViewSize,
            _screen.GetLayerRenderScale(0)
        );
    }

    public override void OnInitialize(ISceneManager sceneManager)
    {
        base.OnInitialize(sceneManager);
    }

    /// <summary>
    /// Populates the tileset surface with random tiles and colors.
    /// </summary>
    private void PopulateWithRandomTiles(TilesetSurfaceScreen screen)
    {
        var tileset = _tilesetManager.GetTileset("alloy");

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

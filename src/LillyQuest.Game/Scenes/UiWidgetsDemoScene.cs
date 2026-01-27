using System.Numerics;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Managers.Scenes.Base;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Game.Scenes;

public class UiWidgetsDemoScene : BaseScene
{
    private readonly IScreenManager _screenManager;
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;
    private UIRootScreen? _screen;

    public UiWidgetsDemoScene(
        IScreenManager screenManager,
        INineSliceAssetManager nineSliceManager,
        ITextureManager textureManager
    )
        : base("ui_widgets_demo")
    {
        _screenManager = screenManager;
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
    }

    public override void OnLoad()
    {
        _screen = new UIRootScreen
        {
            Position = Vector2.Zero,
            Size = new(1280, 720)
        };

        var horizontalBar = new UIProgressBar(_nineSliceManager, _textureManager)
        {
            Position = new(40, 260),
            Size = new(320, 28),
            Min = 0f,
            Max = 1f,
            Value = 0f,
            Orientation = ProgressOrientation.Horizontal,
            NineSliceKey = "simple_ui",
            BackgroundTint = LyColor.FromHex("#d9caa3"),
            ProgressTint = LyColor.FromHex("#8f6f4a"),
            ShowText = true
        };

        var verticalBar = new UIProgressBar(_nineSliceManager, _textureManager)
        {
            Position = new(380, 190),
            Size = new(28, 140),
            Min = 0f,
            Max = 1f,
            Value = 0f,
            Orientation = ProgressOrientation.Vertical,
            NineSliceKey = "simple_ui",
            BackgroundTint = LyColor.FromHex("#d9caa3"),
            ProgressTint = LyColor.FromHex("#8f6f4a"),
            ShowText = true
        };

        var autoSizeLabel = new UILabel
        {
            Position = Vector2.Zero,
            Color = LyColor.White,
            Font = new("default_font", 14, FontKind.TrueType)
        };

        var autoSizeWindow = new UINinePatchWindow(_nineSliceManager, _textureManager)
        {
            Position = new(40, 40),
            Title = "Autosize Window",
            TitleFont = new("default_font", 16, FontKind.TrueType),
            IsTitleBarEnabled = true,
            AutoSizeEnabled = true,
            ContentMargin = new(16f, 40f, 16f, 16f),
            NineSliceScale = 1f,
            NineSliceKey = "simple_ui",
            CenterTint = LyColor.FromHex("#e8d7b0"),
            BorderTint = LyColor.FromHex("#a67c52")
        };
        autoSizeWindow.Add(autoSizeLabel);

        _screen.Root.Add(autoSizeWindow);
        _screen.Root.Add(horizontalBar);
        _screen.Root.Add(verticalBar);
        _screen.Root.Add(new UiWidgetsDemoController(horizontalBar, verticalBar, autoSizeLabel));

        _screenManager.PushScreen(_screen);
    }

    public override void OnUnload()
    {
        if (_screen != null)
        {
            _screenManager.PopScreen(_screen);
            _screen = null;
        }
    }

    private sealed class UiWidgetsDemoController : UIScreenControl
    {
        private const float LabelSwitchInterval = 1.5f;
        private const float LabelCharWidth = 8f;
        private const float LabelLineHeight = 16f;

        private readonly UIProgressBar _horizontalBar;
        private readonly UIProgressBar _verticalBar;
        private readonly UILabel _autoSizeLabel;
        private readonly string[] _labelTexts =
        [
            "Short",
            "A bit longer text",
            "This is a much longer label for autosize"
        ];

        private float _elapsed;
        private float _labelTimer;
        private int _labelIndex;

        public UiWidgetsDemoController(UIProgressBar horizontalBar, UIProgressBar verticalBar, UILabel autoSizeLabel)
        {
            _horizontalBar = horizontalBar;
            _verticalBar = verticalBar;
            _autoSizeLabel = autoSizeLabel;
            UpdateLabelText(_labelTexts[0]);
        }

        public override void Update(GameTime gameTime)
        {
            _elapsed += (float)gameTime.Elapsed.TotalSeconds;
            var t = (MathF.Sin(_elapsed) + 1f) * 0.5f;
            _horizontalBar.Value = _horizontalBar.Min + t * (_horizontalBar.Max - _horizontalBar.Min);
            _verticalBar.Value = _verticalBar.Min + (1f - t) * (_verticalBar.Max - _verticalBar.Min);

            _labelTimer += (float)gameTime.Elapsed.TotalSeconds;
            if (_labelTimer >= LabelSwitchInterval)
            {
                _labelTimer = 0f;
                _labelIndex = (_labelIndex + 1) % _labelTexts.Length;
                UpdateLabelText(_labelTexts[_labelIndex]);
            }
        }

        private void UpdateLabelText(string text)
        {
            _autoSizeLabel.Text = text;
            var width = MathF.Max(1f, text.Length * LabelCharWidth);
            _autoSizeLabel.Size = new(width, LabelLineHeight);
        }
    }
}

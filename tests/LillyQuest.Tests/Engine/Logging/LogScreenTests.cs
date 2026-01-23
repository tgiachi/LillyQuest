using System.Numerics;
using FontStashSharp;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Logging;
using LillyQuest.Engine.Screens.Logging;
using Serilog.Events;

namespace LillyQuest.Tests.Engine.Logging;

public class LogScreenTests
{
    private sealed class FakeFontManager : IFontManager
    {
        public void Dispose() { }

        public BitmapFont GetBitmapFont(string assetName)
            => throw new NotSupportedException();

        public DynamicSpriteFont GetFont(string assetName, int size)
            => throw new NotSupportedException();

        public bool HasFont(string assetName)
            => false;

        public void LoadBmpFont(
            string assetName,
            string filePath,
            int tileWidth,
            int tileHeight,
            int spacing = 0,
            LyColor? transparentColor = null,
            string? characterMap = null
        )
            => throw new NotSupportedException();

        public void LoadBmpFont(
            string assetName,
            Span<byte> data,
            int tileWidth,
            int tileHeight,
            int spacing = 0,
            LyColor? transparentColor = null,
            string? characterMap = null
        )
            => throw new NotSupportedException();

        public void LoadFont(string assetName, string filePath)
            => throw new NotSupportedException();

        public void LoadFont(string assetName, Span<byte> data)
            => throw new NotSupportedException();

        public Vector2 MeasureText(string fontAssetName, int fontSize, string text)
            => new(text.Length * 10f, 10f);

        public bool TryGetBitmapFont(string assetName, out BitmapFont font)
        {
            font = null!;

            return false;
        }

        public bool TryGetFont(string assetName, out DynamicSpriteFont font)
        {
            font = null!;

            return false;
        }

        public void UnloadFont(string assetName)
            => throw new NotSupportedException();
    }

    private sealed class TestLogScreen : LogScreen
    {
        public TestLogScreen(ILogEventDispatcher dispatcher, IFontManager fontManager)
            : base(dispatcher, fontManager) { }

        public IReadOnlyList<LogLine> Lines => GetLines();
    }

    [Test]
    public void Update_DispatchesEntries_And_Trims_To_MaxLines()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        for (var i = 0; i < 6; i++)
        {
            dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, $"Line {i}", null));
        }

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(4));
        Assert.That(screen.Lines[0].Text, Is.EqualTo("Line 2"));
        Assert.That(screen.Lines[3].Text, Is.EqualTo("Line 5"));
    }

    [Test]
    public void Update_Wraps_Long_Words()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);
        screen.Size = new(70, 60);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, "AAAAAAAAA", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(2));
        Assert.That(screen.Lines[0].Text, Is.EqualTo("AAAAA"));
        Assert.That(screen.Lines[1].Text, Is.EqualTo("AAAA"));
    }

    [Test]
    public void Update_Assigns_Level_Color()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Warning, "Warn", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(1));
        Assert.That(screen.Lines[0].Color, Is.EqualTo(LogScreen.WarningColor));
    }

    [Test]
    public void Update_Fatal_Enables_Blink_For_One_Second()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Fatal, "Boom", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.Zero));

        Assert.That(screen.Lines[0].BlinkRemaining, Is.EqualTo(1f));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(1.1)));

        Assert.That(screen.Lines[0].BlinkRemaining, Is.EqualTo(0f));
    }

    private static TestLogScreen CreateScreen(ILogEventDispatcher dispatcher)
    {
        var fontManager = new FakeFontManager();
        var screen = new TestLogScreen(dispatcher, fontManager)
        {
            Position = Vector2.Zero,
            Size = new(100, 60),
            Margin = new(10, 10, 10, 10),
            FontName = "default_log_font",
            FontSize = 12
        };

        return screen;
    }
}

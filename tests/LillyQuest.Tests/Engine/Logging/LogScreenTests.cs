using System.Numerics;
using System.Linq;
using LillyQuest.Core.Graphics.Rendering2D;
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

        public IFontHandle GetFontHandle(FontRef fontRef)
            => new FakeFontHandle();

        public bool TryGetFontHandle(FontRef fontRef, out IFontHandle handle)
        {
            handle = new FakeFontHandle();

            return true;
        }

        public void UnloadFont(string assetName)
            => throw new NotSupportedException();
    }

    private sealed class FakeFontHandle : IFontHandle
    {
        public Vector2 MeasureText(string text)
            => new(text.Length * 10f, 10f);

        public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, LyColor color, float depth = 0f) { }
    }

    private sealed class TestLogScreen : LogScreen
    {
        public TestLogScreen(ILogEventDispatcher dispatcher, IFontManager fontManager)
            : base(dispatcher, fontManager) { }

        public IReadOnlyList<RenderedLine> Lines => GetLines();
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
        Assert.That(GetLineText(screen.Lines[0]), Is.EqualTo("Line 2"));
        Assert.That(GetLineText(screen.Lines[3]), Is.EqualTo("Line 5"));
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
        Assert.That(GetLineText(screen.Lines[0]), Is.EqualTo("AAAAA"));
        Assert.That(GetLineText(screen.Lines[1]), Is.EqualTo("AAAA"));
    }

    [Test]
    public void Update_Rewraps_Lines_When_Size_Changes()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);
        screen.Margin = new(0, 0, 0, 0);
        screen.Size = new(50, 60);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, "AAAAAAAAAA", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(2));

        screen.Size = new(200, 60);
        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(1));
    }

    [Test]
    public void Update_Assigns_Level_Color()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Warning, "Warn", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(1));
        Assert.That(screen.Lines[0].Spans[0].Foreground, Is.EqualTo(LogScreen.WarningColor));
    }

    [Test]
    public void Update_Fatal_Enables_Blink_For_One_Second()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Fatal, "Boom", null));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.01)));

        Assert.That(screen.Lines[0].BlinkRemaining, Is.EqualTo(1f).Within(0.05f));

        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(1.1)));

        Assert.That(screen.Lines[0].BlinkRemaining, Is.EqualTo(0f));
    }

    [Test]
    public void Update_CarriageReturn_Overwrites_Last_Line()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);
        screen.Size = new(240, 60);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, "Loading 0%", null));
        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, "Loading 10%\r", null));
        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

        Assert.That(screen.Lines.Count, Is.EqualTo(1));
        Assert.That(GetLineText(screen.Lines[0]), Is.EqualTo("Loading 10%"));
    }

    [Test]
    public void Update_Renders_Immediately_Without_Typewriter_Delay()
    {
        var dispatcher = new LogEventDispatcher();
        var screen = CreateScreen(dispatcher);

        dispatcher.Enqueue(new(DateTimeOffset.UtcNow, LogEventLevel.Information, "Hello", null));
        screen.Update(new(TimeSpan.Zero, TimeSpan.FromSeconds(0.01)));

        Assert.That(screen.Lines.Count, Is.EqualTo(1));
        Assert.That(GetLineText(screen.Lines[0]), Is.EqualTo("Hello"));
    }

    private static TestLogScreen CreateScreen(ILogEventDispatcher dispatcher)
    {
        var fontManager = new FakeFontManager();
        var screen = new TestLogScreen(dispatcher, fontManager)
        {
            Position = Vector2.Zero,
            Size = new(100, 60),
            Margin = new(10, 10, 10, 10),
            Font = new("default_log_font", 12, FontKind.TrueType)
        };

        return screen;
    }

    private static string GetLineText(RenderedLine line)
        => string.Concat(line.Spans.Select(span => span.Text));
}

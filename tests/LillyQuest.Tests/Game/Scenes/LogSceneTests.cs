using System.Numerics;
using FontStashSharp;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Screens;
using LillyQuest.Engine.Logging;
using LillyQuest.Engine.Managers.Screens;
using LillyQuest.Engine.Scenes;
using LillyQuest.Engine.Screens.Logging;

namespace LillyQuest.Tests.Game.Scenes;

public class LogSceneTests
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

    [Test]
    public void Name_Is_LogScene()
    {
        var scene = CreateScene();

        Assert.That(scene.Name, Is.EqualTo("log_scene"));
    }

    [Test]
    public void OnLoad_Pushes_LogScreen_And_OnUnload_Pops()
    {
        var screenManager = new ScreenManager();
        var dispatcher = new LogEventDispatcher();
        var fontManager = new FakeFontManager();
        var scene = new LogScene(screenManager, dispatcher, fontManager, new EngineRenderContext());

        scene.OnLoad();

        Assert.That(screenManager.ScreenStack.Count, Is.EqualTo(1));
        Assert.That(screenManager.FocusedScreen, Is.InstanceOf<LogScreen>());

        scene.OnUnload();

        Assert.That(screenManager.ScreenStack.Count, Is.EqualTo(0));
    }

    private static LogScene CreateScene()
        => new(new ScreenManager(), new LogEventDispatcher(), new FakeFontManager(), new EngineRenderContext());
}

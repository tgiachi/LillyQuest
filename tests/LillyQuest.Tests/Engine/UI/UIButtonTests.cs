using System.Numerics;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Managers.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Screens.UI;

namespace LillyQuest.Tests.Engine.UI;

public class UIButtonTests
{
    private sealed class FakeTextureManager : ITextureManager
    {
        public Texture2D DefaultWhiteTexture => throw new NotSupportedException();
        public Texture2D DefaultBlackTexture => throw new NotSupportedException();

        public void Dispose() { }

        public IReadOnlyDictionary<string, Texture2D> GetAllTextures()
            => throw new NotSupportedException();

        public Texture2D GetTexture(string assetName)
            => throw new NotSupportedException();

        public bool HasTexture(string assetName)
            => false;

        public void LoadTexture(string assetName, string filePath)
            => throw new NotSupportedException();

        public void LoadTexture(string assetName, Span<byte> data, uint width, uint height)
            => throw new NotSupportedException();

        public void LoadTextureFromPng(string assetName, Span<byte> pngData)
            => throw new NotSupportedException();

        public void LoadTextureFromPngWithChromaKey(string assetName, Span<byte> pngData, byte tolerance = 0)
            => throw new NotSupportedException();

        public void LoadTextureWithChromaKey(string assetName, string filePath, byte tolerance = 0)
            => throw new NotSupportedException();

        public bool TryGetTexture(string assetName, out Texture2D texture)
        {
            texture = null!;

            return false;
        }

        public void UnloadTexture(string assetName)
            => throw new NotSupportedException();
    }

    [Test]
    public void Click_Invokes_OnClick_When_Pressed_And_Released_Inside()
    {
        var button = CreateButton();
        var clicks = 0;
        button.OnClick = () => clicks++;

        button.HandleMouseDown(new(10, 10));
        button.HandleMouseUp(new(10, 10));

        Assert.That(clicks, Is.EqualTo(1));

        button.HandleMouseDown(new(10, 10));
        button.HandleMouseUp(new(200, 200));

        Assert.That(clicks, Is.EqualTo(1));
    }

    [Test]
    public void Hover_Invokes_OnHover_On_Enter()
    {
        var button = CreateButton();
        var hoverCount = 0;
        button.OnHover = () => hoverCount++;

        button.HandleMouseMove(new(10, 10));
        button.HandleMouseMove(new(11, 11));
        button.HandleMouseMove(new(200, 200));
        button.HandleMouseMove(new(10, 10));

        Assert.That(hoverCount, Is.EqualTo(2));
    }

    [Test]
    public void TransitionTime_Lerps_Tint_Toward_Target()
    {
        var button = CreateButton();
        button.TransitionTime = 1f;
        button.IdleTint = new(0, 0, 0, 255);
        button.HoveredTint = new(255, 255, 255, 255);

        button.Update(new(TimeSpan.Zero, TimeSpan.Zero));
        button.HandleMouseMove(new(10, 10));
        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
        button.Update(gameTime);

        Assert.That(button.CurrentTint.R, Is.GreaterThan(0));
        Assert.That(button.CurrentTint.R, Is.LessThan(255));
    }

    [Test]
    public void Update_Advances_Transition()
    {
        var button = CreateButton();
        button.TransitionTime = 1f;
        button.IdleTint = new(0, 0, 0, 255);
        button.HoveredTint = new(255, 255, 255, 255);

        button.Update(new(TimeSpan.Zero, TimeSpan.Zero));
        button.HandleMouseMove(new(10, 10));
        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        button.Update(gameTime);

        Assert.That(button.CurrentTint.R, Is.EqualTo(255));
    }

    private static UIButton CreateButton()
    {
        var textureManager = new FakeTextureManager();
        var nineSliceManager = new NineSliceAssetManager(textureManager);

        return new(nineSliceManager, textureManager)
        {
            Position = Vector2.Zero,
            Size = new(100, 40)
        };
    }
}

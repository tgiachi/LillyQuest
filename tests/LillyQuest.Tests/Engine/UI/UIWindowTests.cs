using System.Numerics;
using LillyQuest.Core.Data.Assets;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class UIWindowTests
{
    private sealed class FakeNineSliceManager : INineSliceAssetManager
    {
        public NineSliceDefinition GetNineSlice(string key)
            => throw new NotSupportedException();

        public TexturePatch GetTexturePatch(string textureName, string elementName)
            => throw new NotSupportedException();

        public void LoadNineSlice(
            string key,
            string textureName,
            string filePath,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
            => throw new NotSupportedException();

        public void LoadNineSlice(
            string key,
            string textureName,
            Span<byte> data,
            uint width,
            uint height,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
            => throw new NotSupportedException();

        public void LoadNineSliceFromPng(
            string key,
            string textureName,
            Span<byte> pngData,
            Rectangle<int> sourceRect,
            Vector4D<float> margins
        )
            => throw new NotSupportedException();

        public void RegisterNineSlice(string key, string textureName, Rectangle<int> sourceRect, Vector4D<float> margins)
            => throw new NotSupportedException();

        public void RegisterTexturePatches(string textureName, IReadOnlyList<TexturePatchDefinition> patches)
            => throw new NotSupportedException();

        public bool TryGetNineSlice(string key, out NineSliceDefinition definition)
        {
            definition = default;

            return false;
        }

        public bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch)
        {
            patch = default;

            return false;
        }
    }

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
    public void Add_Uses_Base_Children()
    {
        var window = new UIWindow();
        var child = new UIScreenControl();

        window.Add(child);

        Assert.That(window.Children.Count, Is.EqualTo(1));
        Assert.That(window.Children[0], Is.EqualTo(child));
    }

    [Test]
    public void AutoSizeEnabled_Reacts_To_Child_Resize_On_Update()
    {
        var window = new UIWindow { AutoSizeEnabled = true, IsTitleBarEnabled = false };
        var child = new UIScreenControl { Position = new(0, 0), Size = new(10, 10) };
        window.Add(child);

        window.Update(new(TimeSpan.Zero, TimeSpan.Zero));
        Assert.That(window.Size, Is.EqualTo(new Vector2(10, 10)));

        child.Size = new(20, 15);
        window.Update(new(TimeSpan.Zero, TimeSpan.Zero));
        Assert.That(window.Size, Is.EqualTo(new Vector2(20, 15)));
    }

    [Test]
    public void BackgroundColor_UsesAlphaMultiplier()
    {
        var window = new UIWindow
        {
            BackgroundColor = new(255, 10, 20, 30),
            BackgroundAlpha = 0.5f
        };

        var color = window.GetBackgroundColorWithAlpha();

        Assert.That(color.A, Is.EqualTo(128));
    }

    [Test]
    public void Drag_Clamps_To_Parent_Bounds()
    {
        var parent = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 100)
        };
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(40, 40),
            Parent = parent,
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        window.HandleMouseDown(new(5, 5));
        window.HandleMouseMove(new(200, 200));

        Assert.That(window.Position, Is.EqualTo(new Vector2(60, 60)));
    }

    [Test]
    public void MouseDown_Delegates_To_Children_Topmost_First()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        window.TitleBarHeight = 10f;
        var a = new UIScreenControl { Position = Vector2.Zero, Size = new(100, 50), ZIndex = 0 };
        var b = new UIScreenControl { Position = Vector2.Zero, Size = new(100, 50), ZIndex = 1 };
        var hit = string.Empty;
        a.OnMouseDown = _ =>
                        {
                            hit = "a";

                            return true;
                        };
        b.OnMouseDown = _ =>
                        {
                            hit = "b";

                            return true;
                        };
        window.Add(a);
        window.Add(b);

        var handled = window.HandleMouseDown(new(10, 20));

        Assert.That(handled, Is.True);
        Assert.That(hit, Is.EqualTo("b"));
    }

    [Test]
    public void MouseDown_TitleBar_StartsDrag_WhenMovable()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50),
            IsTitleBarEnabled = true,
            IsWindowMovable = true
        };
        window.TitleBarHeight = 10f;

        var handled = window.HandleMouseDown(new(5, 5));
        var moved = window.HandleMouseMove(new(20, 20));

        Assert.That(handled, Is.True);
        Assert.That(moved, Is.True);
        Assert.That(window.Position, Is.EqualTo(new Vector2(15, 15)));
    }

    [Test]
    public void MouseMove_Forwards_To_Active_Child()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var child = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var moves = 0;
        child.OnMouseDown = _ => true;
        child.OnMouseMove = _ =>
                            {
                                moves++;

                                return true;
                            };
        window.Add(child);

        window.HandleMouseDown(new(10, 10));
        window.HandleMouseMove(new(12, 12));

        Assert.That(moves, Is.EqualTo(1));
    }

    [Test]
    public void MouseUp_Forwards_To_Active_Child()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var child = new UIScreenControl
        {
            Position = Vector2.Zero,
            Size = new(100, 50)
        };
        var ups = 0;
        child.OnMouseDown = _ => true;
        child.OnMouseUp = _ =>
                          {
                              ups++;

                              return true;
                          };
        window.Add(child);

        window.HandleMouseDown(new(10, 10));
        window.HandleMouseUp(new(12, 12));

        Assert.That(ups, Is.EqualTo(1));
    }

    [Test]
    public void RecalculateAutoSize_Includes_TitleBar_And_ContentMargin()
    {
        var window = new UINinePatchWindow(new FakeNineSliceManager(), new FakeTextureManager())
        {
            AutoSizeEnabled = true,
            IsTitleBarEnabled = true,
            TitleBarHeight = 20f,
            ContentMargin = new(10f, 8f, 6f, 4f)
        };

        var child = new UIScreenControl { Position = new(0, 0), Size = new(100, 40) };
        window.Add(child);

        window.RecalculateAutoSize();

        Assert.That(window.Size.X, Is.EqualTo(10f + 100f + 6f));
        Assert.That(window.Size.Y, Is.EqualTo(20f + 40f + 4f));
    }

    [Test]
    public void Resize_Clamps_To_Min_And_Max_Size()
    {
        var window = new UIWindow
        {
            Position = Vector2.Zero,
            Size = new(100, 50),
            IsResizable = true,
            ResizeHandleSize = new(10, 10),
            MinSize = new(80, 40),
            MaxSize = new(120, 60)
        };

        var start = new Vector2(window.Position.X + window.Size.X - 1, window.Position.Y + window.Size.Y - 1);

        window.HandleMouseDown(start);
        window.HandleMouseMove(new(200, 200));
        window.HandleMouseUp(new(200, 200));

        Assert.That(window.Size, Is.EqualTo(new Vector2(120, 60)));

        start = new(window.Position.X + window.Size.X - 1, window.Position.Y + window.Size.Y - 1);

        window.HandleMouseDown(start);
        window.HandleMouseMove(new(-200, -200));
        window.HandleMouseUp(new(-200, -200));

        Assert.That(window.Size, Is.EqualTo(new Vector2(80, 40)));
    }

    [Test]
    public void Resize_Is_Ignored_When_Disabled()
    {
        var window = new UIWindow
        {
            Position = new(10, 10),
            Size = new(100, 50),
            IsResizable = false,
            ResizeHandleSize = new(10, 10)
        };

        var start = new Vector2(10 + 100 - 1, 10 + 50 - 1);

        var pressed = window.HandleMouseDown(start);
        window.HandleMouseMove(new(200, 200));
        window.HandleMouseUp(new(200, 200));

        Assert.That(pressed, Is.False);
        Assert.That(window.Size, Is.EqualTo(new Vector2(100, 50)));
    }

    [Test]
    public void ResizeHandle_Drag_Resizes_Window()
    {
        var window = new UIWindow
        {
            Position = new(10, 10),
            Size = new(100, 50),
            IsResizable = true,
            ResizeHandleSize = new(10, 10)
        };

        var start = new Vector2(10 + 100 - 1, 10 + 50 - 1);
        var moved = new Vector2(10 + 130, 10 + 80);

        var pressed = window.HandleMouseDown(start);
        var drag = window.HandleMouseMove(moved);
        var released = window.HandleMouseUp(moved);

        Assert.That(pressed, Is.True);
        Assert.That(drag, Is.True);
        Assert.That(released, Is.True);
        Assert.That(window.Size, Is.EqualTo(new Vector2(130, 80)));
    }

    [Test]
    public void TitleFontSettings_AreConfigurable()
    {
        var window = new UIWindow
        {
            TitleFont = new("alloy", 18, FontKind.TrueType)
        };

        Assert.That(window.TitleFont, Is.EqualTo(new FontRef("alloy", 18, FontKind.TrueType)));
    }
}

using System.Numerics;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Engine.Screens.UI;
using Silk.NET.Maths;

namespace LillyQuest.Engine.Screens.UI;

public sealed class UINinePatchWindow : UIScreenControl
{
    private readonly List<UIScreenControl> _children = [];
    private readonly INineSliceAssetManager _nineSliceManager;
    private readonly ITextureManager _textureManager;

    public IReadOnlyList<UIScreenControl> Children => _children;

    public string NineSliceKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleFontName { get; set; } = "default_font";
    public int TitleFontSize { get; set; } = 14;
    public Vector4D<float> TitleMargin { get; set; } = Vector4D<float>.Zero;
    public Vector4D<float> ContentMargin { get; set; } = Vector4D<float>.Zero;
    public float NineSliceScale { get; set; } = 1f;

    public UINinePatchWindow(INineSliceAssetManager nineSliceManager, ITextureManager textureManager)
    {
        _nineSliceManager = nineSliceManager;
        _textureManager = textureManager;
    }

    public void Add(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        control.Parent = this;
        _children.Add(control);
    }

    public void Remove(UIScreenControl control)
    {
        if (control == null)
        {
            return;
        }

        _children.Remove(control);
        if (control.Parent == this)
        {
            control.Parent = null;
        }
    }

    public Vector2 GetTitlePosition()
    {
        var world = GetWorldPosition();
        return new Vector2(world.X + TitleMargin.X, world.Y + TitleMargin.Y);
    }

    public Vector2 GetContentOrigin()
    {
        var world = GetWorldPosition();
        return new Vector2(world.X + ContentMargin.X, world.Y + ContentMargin.Y);
    }
}

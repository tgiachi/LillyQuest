using LillyQuest.Core.Managers.Assets;
using NUnit.Framework;
using Silk.NET.Maths;

namespace LillyQuest.Tests.Engine.UI;

public class NineSliceAssetManagerTests
{
    [Test]
    public void RegisterNineSlice_ComputesRects()
    {
        var manager = new NineSliceAssetManager();
        manager.RegisterNineSlice(
            "window",
            "ui",
            new Rectangle<int>(0, 0, 32, 32),
            new Vector4D<float>(8, 8, 8, 8)
        );

        var def = manager.GetNineSlice("window");

        Assert.That(def.Center.Size.X, Is.EqualTo(16));
        Assert.That(def.Center.Size.Y, Is.EqualTo(16));
    }
}

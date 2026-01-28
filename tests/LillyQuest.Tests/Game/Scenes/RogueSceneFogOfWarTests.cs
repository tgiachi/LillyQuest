using System.Reflection;
using System.Runtime.CompilerServices;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Primitives;
using LillyQuest.Game.Scenes;
using NUnit.Framework;

namespace LillyQuest.Tests.Game.Scenes;

public class RogueSceneFogOfWarTests
{
    [Test]
    public void DarkenTile_DarkensForegroundAndBackground()
    {
        var scene = (RogueScene)RuntimeHelpers.GetUninitializedObject(typeof(RogueScene));
        var method = typeof(RogueScene).GetMethod("DarkenTile", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, "DarkenTile should exist for fog-of-war shading.");

        var originalForeground = new LyColor(255, 100, 150, 200);
        var originalBackground = new LyColor(255, 50, 100, 150);
        var tile = new TileRenderData(1, originalForeground, originalBackground);

        Assert.That(tile.ForegroundColor, Is.EqualTo(originalForeground));
        Assert.That(tile.BackgroundColor, Is.EqualTo(originalBackground));

        var result = (TileRenderData)method!.Invoke(scene, new object[] { tile })!;

        Assert.That(result.ForegroundColor, Is.EqualTo(new LyColor(255, 50, 75, 100)));
        Assert.That(result.BackgroundColor, Is.EqualTo(new LyColor(255, 25, 50, 75)));
    }
}

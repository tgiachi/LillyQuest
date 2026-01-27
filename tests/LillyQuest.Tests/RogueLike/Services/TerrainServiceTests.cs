using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.Tests.RogueLike.Services;

public class TerrainServiceTests
{
    [Test]
    public void ResolvedTerrainData_StoresAllFields()
    {
        var fg = new LyColor(0xFF, 0x01, 0x02, 0x03);
        var bg = new LyColor(0xFF, 0x10, 0x20, 0x30);
        var flags = new List<string> { "walkable" };
        var tags = new List<string> { "floor" };

        var data = new ResolvedTerrainData(
            "floor",
            "Floor",
            "Stone floor",
            flags,
            1,
            "comment",
            tags,
            ".",
            fg,
            bg,
            null
        );

        Assert.That(data.Id, Is.EqualTo("floor"));
        Assert.That(data.Name, Is.EqualTo("Floor"));
        Assert.That(data.Description, Is.EqualTo("Stone floor"));
        Assert.That(data.Flags, Is.EquivalentTo(flags));
        Assert.That(data.MovementCost, Is.EqualTo(1));
        Assert.That(data.Comment, Is.EqualTo("comment"));
        Assert.That(data.Tags, Is.EquivalentTo(tags));
        Assert.That(data.TileSymbol, Is.EqualTo("."));
        Assert.That(data.TileFgColor, Is.EqualTo(fg));
        Assert.That(data.TileBgColor, Is.EqualTo(bg));
    }
}

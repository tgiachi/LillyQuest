using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Json.Entities.Tiles;

namespace LillyQuest.Tests.RogueLike.Services;

public class TileSetServiceTests
{
    [Test]
    public void ResolvedTileData_StoresResolvedColors()
    {
        var fg = new LyColor(0xFF, 0x01, 0x02, 0x03);
        var bg = new LyColor(0xFF, 0x10, 0x20, 0x30);
        var data = new ResolvedTileData("t1", ".", fg, bg, null);

        Assert.That(data.Id, Is.EqualTo("t1"));
        Assert.That(data.Symbol, Is.EqualTo("."));
        Assert.That(data.FgColor, Is.EqualTo(fg));
        Assert.That(data.BgColor, Is.EqualTo(bg));
    }
}

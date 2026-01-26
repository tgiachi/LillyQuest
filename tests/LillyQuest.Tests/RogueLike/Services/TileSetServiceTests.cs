using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Json.Entities.Tiles;
using LillyQuest.RogueLike.Services;

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

    [Test]
    public async Task TryGetTile_UsesDefaultTileset_ResolvesColors()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "fg", Color = new LyColor(0xFF, 0x01, 0x02, 0x03) },
                    new ColorSchemaJson { Id = "bg", Color = new LyColor(0xFF, 0x10, 0x20, 0x30) }
                }
            }
        });

        var tileSetService = new TileSetService(colorService) { DefaultTileset = "main" };
        await tileSetService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TilesetDefinitionJson
            {
                Id = "tileset-1",
                Name = "main",
                TextureName = "tiles.png",
                Tiles = new List<TileDefinition>
                {
                    new TileDefinition { Id = "t1", Symbol = ".", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var success = tileSetService.TryGetTile("t1", out var tile);

        Assert.That(success, Is.True);
        Assert.That(tile.Id, Is.EqualTo("t1"));
        Assert.That(tile.FgColor, Is.EqualTo(new LyColor(0xFF, 0x01, 0x02, 0x03)));
        Assert.That(tile.BgColor, Is.EqualTo(new LyColor(0xFF, 0x10, 0x20, 0x30)));
    }

    [Test]
    public async Task TryGetTile_ReturnsFalse_WhenTilesetMissing()
    {
        var tileSetService = new TileSetService(new ColorService()) { DefaultTileset = "missing" };

        var success = tileSetService.TryGetTile("t1", out _);

        Assert.That(success, Is.False);
    }

    [Test]
    public async Task TryGetTile_ReturnsFalse_WhenTileMissing()
    {
        var tileSetService = new TileSetService(new ColorService()) { DefaultTileset = "main" };
        await tileSetService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TilesetDefinitionJson { Name = "main", Tiles = new List<TileDefinition>() }
        });

        var success = tileSetService.TryGetTile("missing", out _);

        Assert.That(success, Is.False);
    }

    [Test]
    public async Task TryGetTile_ReturnsFalse_WhenColorUnresolved()
    {
        var colorService = new ColorService { DefaultColorSet = "schema" };
        await colorService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson { Id = "schema", Colors = new List<ColorSchemaJson>() }
        });

        var tileSetService = new TileSetService(colorService) { DefaultTileset = "main" };
        await tileSetService.LoadDataAsync(new List<BaseJsonEntity>
        {
            new TilesetDefinitionJson
            {
                Name = "main",
                Tiles = new List<TileDefinition>
                {
                    new TileDefinition { Id = "t1", Symbol = ".", FgColor = "fg", BgColor = "bg" }
                }
            }
        });

        var success = tileSetService.TryGetTile("t1", out _);

        Assert.That(success, Is.False);
    }
}

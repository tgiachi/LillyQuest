using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using LillyQuest.RogueLike.Services;

namespace LillyQuest.Tests.RogueLike.Services;

public class ColorServiceTests
{
    private ColorService _colorService = null!;

    [SetUp]
    public void Setup()
    {
        _colorService = new ColorService();
    }

    [Test]
    public void Constructor_InitializesEmptyColorSets()
    {
        var service = new ColorService();

        Assert.That(service.DefaultColorSet, Is.Null.Or.Empty);
    }

    [Test]
    public async Task LoadDataAsync_LoadsColorSchemas()
    {
        var color1 = new LyColor(0xFF, 0xAA, 0xBB, 0xCC);
        var color2 = new LyColor(0xFF, 0x11, 0x22, 0x33);
        _colorService.DefaultColorSet = "schema1";

        var entities = new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema1",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "red", Color = color1 },
                    new ColorSchemaJson { Id = "blue", Color = color2 }
                }
            }
        };

        await _colorService.LoadDataAsync(entities);

        Assert.That(_colorService.GetColorById("red"), Is.EqualTo(color1));
        Assert.That(_colorService.GetColorById("blue"), Is.EqualTo(color2));
    }

    [Test]
    public async Task LoadDataAsync_MultipleSchemas_LoadsAllColors()
    {
        var color1 = new LyColor(0xFF, 0x12, 0x34, 0x56);
        var color2 = new LyColor(0xFF, 0xAA, 0xBB, 0xCC);
        var color3 = new LyColor(0xFF, 0x99, 0x88, 0x77);
        _colorService.DefaultColorSet = "schema1";

        var entities = new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema1",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "color1", Color = color1 },
                    new ColorSchemaJson { Id = "color2", Color = color2 }
                }
            },
            new ColorSchemaDefintionJson
            {
                Id = "schema2",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "color3", Color = color3 }
                }
            }
        };

        await _colorService.LoadDataAsync(entities);

        Assert.That(_colorService.GetColorById("color1"), Is.EqualTo(color1));
        Assert.That(_colorService.GetColorById("color2"), Is.EqualTo(color2));
        Assert.That(_colorService.GetColorById("color3"), Is.EqualTo(color3));
    }

    [Test]
    public void GetColorById_WithNoDefaultColorSet_ReturnsNull()
    {
        _colorService.DefaultColorSet = null;

        var result = _colorService.GetColorById("red");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetColorById_WithEmptyDefaultColorSet_ReturnsNull()
    {
        _colorService.DefaultColorSet = string.Empty;

        var result = _colorService.GetColorById("red");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetColorById_WithDefaultColorSet_ReturnsColor()
    {
        var testColor = new LyColor(0xFF, 0x12, 0x34, 0x56);
        _colorService.DefaultColorSet = "default-schema";

        var entities = new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "default-schema",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "primary", Color = testColor }
                }
            }
        };

        await _colorService.LoadDataAsync(entities);

        var result = _colorService.GetColorById("primary");

        Assert.That(result, Is.EqualTo(testColor));
    }

    [Test]
    public async Task GetColorById_WithNonExistentColorId_ReturnsNull()
    {
        _colorService.DefaultColorSet = "schema1";

        var entities = new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema1",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "red", Color = new LyColor(0xFF, 0xFF, 0x00, 0x00) }
                }
            }
        };

        await _colorService.LoadDataAsync(entities);

        var result = _colorService.GetColorById("non-existent");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLoadTypes_ReturnsColorSchemaDefinitionJsonType()
    {
        var types = _colorService.GetLoadTypes();

        Assert.That(types, Is.Not.Empty);
        Assert.That(types[0], Is.EqualTo(typeof(ColorSchemaDefintionJson)));
    }

    [Test]
    public async Task ClearData_RemovesAllLoadedColors()
    {
        _colorService.DefaultColorSet = "schema1";

        var entities = new List<BaseJsonEntity>
        {
            new ColorSchemaDefintionJson
            {
                Id = "schema1",
                Colors = new List<ColorSchemaJson>
                {
                    new ColorSchemaJson { Id = "red", Color = new LyColor(0xFF, 0xFF, 0x00, 0x00) }
                }
            }
        };

        await _colorService.LoadDataAsync(entities);
        _colorService.ClearData();

        var result = _colorService.GetColorById("red");

        Assert.That(result, Is.Null);
    }
}

using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class ColorService : IDataLoaderReceiver
{
    private readonly ILogger _logger = Log.ForContext<ColorService>();

    private readonly Dictionary<string, ColorData> _colorSets = new();

    public string DefaultColorSet { get; set; }

    public void ClearData()
    {
        _colorSets.Clear();
    }

    public bool VerifyLoadedData()
    {
        return true;
    }

    public LyColor? GetColorById(string colorId, string? colorSet = null)
    {
        if (colorId.StartsWith('#'))
        {
            return LyColor.FromHex(colorId);
        }

        var effectiveColorSet = colorSet ?? DefaultColorSet;

        if (string.IsNullOrEmpty(effectiveColorSet))
        {
            _logger.Warning("No color set specified and no default color set defined");

            return null;
        }

        if (_colorSets.TryGetValue(colorId, out var colorData))
        {
            return colorData.Color;
        }

        _logger.Warning("Color with ID {ColorId} not found", colorId);

        return null;
    }

    public Type[] GetLoadTypes()
        => [typeof(ColorSchemaDefintionJson)];

    public async Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        var colorSchemas = entities.Cast<ColorSchemaDefintionJson>().ToList();

        foreach (var colorSchema in colorSchemas)
        {
            foreach (var color in colorSchema.Colors)
            {
                _colorSets[color.Id] = new(color.Id, color.Color);
            }

            _logger.Information(
                "Loaded Color Schema: {Id} with {ColorCount} colors",
                colorSchema.Id,
                colorSchema.Colors.Count
            );
        }
    }
}

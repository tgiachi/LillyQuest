using LillyQuest.RogueLike.Data.Internal;
using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Colorschemas;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class ColorService : IDataLoaderReceiver
{
    private readonly ILogger _logger = Log.ForContext<ColorService>();

    private readonly Dictionary<string, ColorData> _colorSets = new();

    private readonly IDataLoaderService _dataLoader;

    public ColorService(IDataLoaderService dataLoader)
    {
        _dataLoader = dataLoader;
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
                _colorSets[color.Id] = new ColorData(color.Id, color.Color);
            }

            _logger.Information(
                "Loaded Color Schema: {Id} with {ColorCount} colors",
                colorSchema.Id,
                colorSchema.Colors.Count
            );
        }
    }

    public void ClearData()
    {
        _colorSets.Clear();
    }
}

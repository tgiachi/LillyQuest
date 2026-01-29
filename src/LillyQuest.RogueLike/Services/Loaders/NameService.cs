using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Json.Entities.Names;

namespace LillyQuest.RogueLike.Services.Loaders;

public class NameService : IDataLoaderReceiver
{
    private readonly Dictionary<CreatureGenderType, List<string>> _firstNames = new();
    private readonly Dictionary<CreatureGenderType, List<string>> _lastNames = new();

    public void ClearData()
    {
        _firstNames.Clear();
        _lastNames.Clear();
    }

    public Type[] GetLoadTypes()
        => [typeof(NameDefinitionJson)];

    public string GetRandomName(CreatureGenderType gender, Random? rng = null)
        => GetRandomName("%firstName %lastName", gender, rng);

    public string GetRandomName(string template, CreatureGenderType gender, Random? rng = null)
    {
        if (string.IsNullOrEmpty(template))
        {
            template = "%firstName %lastName";
        }

        rng ??= Random.Shared;

        _firstNames.TryGetValue(gender, out var firstNames);
        _lastNames.TryGetValue(gender, out var lastNames);

        if ((firstNames is null || firstNames.Count == 0) && (lastNames is null || lastNames.Count == 0))
        {
            return string.Empty;
        }

        firstNames ??= [];
        lastNames ??= [];

        var first = firstNames.Count > 0 ? firstNames[rng.Next(firstNames.Count)] : string.Empty;
        var last = lastNames.Count > 0 ? lastNames[rng.Next(lastNames.Count)] : string.Empty;

        return template.Replace("%firstName", first).Replace("%lastName", last);
    }

    public Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        foreach (var entity in entities.Cast<NameDefinitionJson>())
        {
            if (!_firstNames.TryGetValue(entity.Gender, out var firstNames))
            {
                firstNames = [];
                _firstNames[entity.Gender] = firstNames;
            }

            if (entity.FirstNames.Count > 0)
            {
                firstNames.AddRange(entity.FirstNames);
            }

            if (!_lastNames.TryGetValue(entity.Gender, out var lastNames))
            {
                lastNames = [];
                _lastNames[entity.Gender] = lastNames;
            }

            if (entity.LastNames.Count > 0)
            {
                lastNames.AddRange(entity.LastNames);
            }
        }

        return Task.CompletedTask;
    }

    public bool VerifyLoadedData()
        => true;
}

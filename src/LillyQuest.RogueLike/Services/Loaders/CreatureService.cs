using LillyQuest.RogueLike.Interfaces;
using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Utils;

namespace LillyQuest.RogueLike.Services.Loaders;

public class CreatureService : IDataLoaderReceiver
{
    private readonly Dictionary<string, CreatureDefinitionJson> _creaturesById = new();

    public Type[] GetLoadTypes()
        => [typeof(CreatureDefinitionJson)];

    public Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        foreach (var entity in entities.Cast<CreatureDefinitionJson>())
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                continue;
            }

            _creaturesById[entity.Id] = entity;
        }

        return Task.CompletedTask;
    }
    public void ClearData()
    {
        _creaturesById.Clear();
    }

    public bool VerifyLoadedData()
    {
        foreach (var creature in _creaturesById.Values)
        {
            if (string.IsNullOrWhiteSpace(creature.Id))
            {
                throw new InvalidOperationException("Creature with missing ID found during verification");
            }

            if (!Enum.IsDefined<CreatureGenderType>(creature.Gender))
            {
                throw new InvalidOperationException($"Creature {creature.Id} has invalid gender value");
            }
        }

        return true;
    }

    public bool TryGetCreature(string creatureId, out CreatureDefinitionJson creature)
    {
        creature = null!;

        if (_creaturesById.TryGetValue(creatureId, out var resolved))
        {
            creature = resolved;
            return true;
        }

        return false;
    }

    public CreatureDefinitionJson? GetRandomCreature(string categoryFilter, Random? rng = null)
        => GetRandomCreature(categoryFilter, null, rng);

    public CreatureDefinitionJson? GetRandomCreature(
        string categoryFilter,
        string? subcategoryFilter,
        Random? rng = null
    )
    {
        if (string.IsNullOrWhiteSpace(categoryFilter))
        {
            return null;
        }

        var candidates = _creaturesById.Values
            .Where(creature => PatternMatchUtils.Matches(creature.Category, categoryFilter));

        if (!string.IsNullOrWhiteSpace(subcategoryFilter))
        {
            candidates = candidates.Where(
                creature => PatternMatchUtils.Matches(creature.Subcategory, subcategoryFilter)
            );
        }

        var list = candidates.ToList();

        if (list.Count == 0)
        {
            return null;
        }

        rng ??= Random.Shared;
        return list[rng.Next(list.Count)];
    }
}

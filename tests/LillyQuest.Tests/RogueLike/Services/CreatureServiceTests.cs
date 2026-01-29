using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class CreatureServiceTests
{
    [Test]
    public async Task TryGetCreature_ReturnsCreatureById()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new CreatureDefinitionJson
            {
                Id = "orc",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "orc"
            }
        });

        var success = service.TryGetCreature("orc", out var creature);

        Assert.That(success, Is.True);
        Assert.That(creature.Id, Is.EqualTo("orc"));
    }

    [Test]
    public async Task TryGetCreature_ReturnsFalseWhenMissing()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>());

        var success = service.TryGetCreature("missing", out var creature);

        Assert.That(success, Is.False);
        Assert.That(creature, Is.Null);
    }

    [Test]
    public async Task VerifyLoadedData_ThrowsWhenGenderInvalid()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new CreatureDefinitionJson
            {
                Id = "mystery",
                Gender = (CreatureGenderType)999,
                Category = "unknown",
                Subcategory = "unknown"
            }
        });

        Assert.Throws<InvalidOperationException>(() => service.VerifyLoadedData());
    }

    [Test]
    public async Task GetRandomCreature_ByCategory_ReturnsMatch()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new CreatureDefinitionJson
            {
                Id = "orc",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "orc"
            },
            new CreatureDefinitionJson
            {
                Id = "goblin",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "goblin"
            },
            new CreatureDefinitionJson
            {
                Id = "wolf",
                Gender = CreatureGenderType.Animal,
                Category = "beast",
                Subcategory = "canine"
            }
        });

        var creature = service.GetRandomCreature("humanoid", null, new Random(2));

        Assert.That(creature, Is.Not.Null);
        Assert.That(creature!.Category, Is.EqualTo("humanoid"));
    }

    [Test]
    public async Task GetRandomCreature_ByCategoryAndSubcategory_WildcardContains()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new CreatureDefinitionJson
            {
                Id = "orc_warrior",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "orc"
            },
            new CreatureDefinitionJson
            {
                Id = "orc_shaman",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "orc-shaman"
            }
        });

        var creature = service.GetRandomCreature("humanoid", "*sham*", new Random(1));

        Assert.That(creature, Is.Not.Null);
        Assert.That(creature!.Subcategory, Is.EqualTo("orc-shaman"));
    }

    [Test]
    public async Task GetRandomCreature_NoMatches_ReturnsNull()
    {
        var service = new CreatureService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new CreatureDefinitionJson
            {
                Id = "orc",
                Gender = CreatureGenderType.Male,
                Category = "humanoid",
                Subcategory = "orc"
            }
        });

        var creature = service.GetRandomCreature("beast", null, new Random(4));

        Assert.That(creature, Is.Null);
    }
}

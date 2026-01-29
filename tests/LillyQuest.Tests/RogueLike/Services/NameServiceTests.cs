using LillyQuest.RogueLike.Json.Entities.Base;
using LillyQuest.RogueLike.Json.Entities.Creatures;
using LillyQuest.RogueLike.Json.Entities.Names;
using LillyQuest.RogueLike.Services.Loaders;

namespace LillyQuest.Tests.RogueLike.Services;

public class NameServiceTests
{
    [Test]
    public async Task GetRandomName_DefaultTemplate_ReturnsFirstAndLast()
    {
        var service = new NameService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new NameDefinitionJson
            {
                Id = "female",
                Gender = CreatureGenderType.Female,
                FirstNames = new List<string> { "Ada" },
                LastNames = new List<string> { "Lovelace" }
            }
        });

        var name = service.GetRandomName(CreatureGenderType.Female, new Random(1));

        Assert.That(name, Is.EqualTo("Ada Lovelace"));
    }

    [Test]
    public async Task GetRandomName_CustomTemplate_ReplacesTokens()
    {
        var service = new NameService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new NameDefinitionJson
            {
                Id = "female",
                Gender = CreatureGenderType.Female,
                FirstNames = new List<string> { "Ada" },
                LastNames = new List<string> { "Lovelace" }
            }
        });

        var name = service.GetRandomName("%lastName, %firstName", CreatureGenderType.Female, new Random(1));

        Assert.That(name, Is.EqualTo("Lovelace, Ada"));
    }

    [Test]
    public async Task GetRandomName_EmptyLastNames_ReplacesWithEmptyString()
    {
        var service = new NameService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new NameDefinitionJson
            {
                Id = "female",
                Gender = CreatureGenderType.Female,
                FirstNames = new List<string> { "Ada" },
                LastNames = new List<string>()
            }
        });

        var name = service.GetRandomName("%firstName-%lastName", CreatureGenderType.Female, new Random(1));

        Assert.That(name, Is.EqualTo("Ada-"));
    }

    [Test]
    public async Task GetRandomName_NoMatchingGender_ReturnsEmpty()
    {
        var service = new NameService();
        await service.LoadDataAsync(new List<BaseJsonEntity>
        {
            new NameDefinitionJson
            {
                Id = "male",
                Gender = CreatureGenderType.Male,
                FirstNames = new List<string> { "Alan" },
                LastNames = new List<string> { "Turing" }
            }
        });

        var name = service.GetRandomName(CreatureGenderType.Female, new Random(1));

        Assert.That(name, Is.Empty);
    }
}

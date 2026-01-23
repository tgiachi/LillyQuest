namespace LillyQuest.RogueLike.Json.Entities.Base;

public class BaseJsonEntity
{
    public string Id { get; set; }

    public List<string> Tags { get; set; } = [];
}

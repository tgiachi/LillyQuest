using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities;

public class TestGameEntity : GameEntity, IIMGuiEntity
{
    private readonly Action? _drawAction;

    public TestGameEntity(string? name = null, Action? drawAction = null)
    {
        Name = name ?? "Test Entity";
        _drawAction = drawAction;
    }

    public void DrawIMGui()
    {
        _drawAction?.Invoke();
    }
}

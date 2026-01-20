using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Engine.Entities;

public class TestGameEntity : GameEntity, IIMGuiEntity
{

    public TestGameEntity()
    {
        Name = "TestGame";
    }
    public void DrawIMGui()
    {

    }
}

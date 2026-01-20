using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Managers.Screens.Base;
using LillyQuest.Game.Entities;

namespace LillyQuest.Game.Screens;

public class TestScreen : BaseScreen
{
    public TestScreen()
    {
        Size = new(400, 300);
        Position = new(100, 100);
    }

    private SpriteGameEntity _spriteGameEntity;

    public override void OnLoad()
    {
        _spriteGameEntity = new SpriteGameEntity
        {
            Position = Position
        };

        AddEntity(_spriteGameEntity);

        base.OnLoad();
    }

    public override void Update(GameTime gameTime)
    {
        _spriteGameEntity.Position = Position;

        base.Update(gameTime);
    }




}

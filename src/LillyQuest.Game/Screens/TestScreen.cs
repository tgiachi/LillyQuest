using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Managers.Screens.Base;
using LillyQuest.Game.Entities;

namespace LillyQuest.Game.Screens;

public class TestScreen : BaseScreen
{

    private SpriteGameEntity _spriteGameEntity;

    private float accumulator = 0f;
    private const float interval = 1f; // 1 second interval

    public override void OnLoad()
    {
        _spriteGameEntity = new SpriteGameEntity
        {
            Position = new Vector2(50, 50),
            Size = new Vector2(30,30)
        };

        AddEntity(_spriteGameEntity);

        base.OnLoad();
    }

    public override void Update(GameTime gameTime)
    {
        accumulator += (float)gameTime.Elapsed.TotalSeconds;

        if (accumulator >= interval)
        {
            // Move the sprite entity by 10 units to the right every second
            Position += new Vector2(10, 0);
            accumulator -= interval; // Reset the accumulator
        }

        base.Update(gameTime);

    }



}

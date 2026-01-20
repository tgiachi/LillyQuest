using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Entities;
using LillyQuest.Engine.Interfaces.Features;

namespace LillyQuest.Game.Entities;

public class SpriteGameEntity : GameEntity, IRenderableEntity
{
    public Vector2 Position { get; set; }

    public void Render(SpriteBatch spriteBatch, EngineRenderContext context)
    {
        spriteBatch.DrawTexture("logo", Position, new(810, 847), LyColor.White);
    }
}

using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Features;

public interface IFixedUpdateFeature : IGameObjectFeature
{
    void FixedUpdate(GameTime gameTime);
}

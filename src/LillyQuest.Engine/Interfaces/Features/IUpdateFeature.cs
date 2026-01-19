using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.GameObjects.Features;

namespace LillyQuest.Engine.Interfaces.Features;

public interface IUpdateFeature : IGameObjectFeature
{
    void Update(GameTime gameTime);
}

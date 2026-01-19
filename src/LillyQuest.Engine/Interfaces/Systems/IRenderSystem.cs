using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Interfaces.Systems;

/// <summary>
/// Interface for systems that handle rendering operations.
/// Render systems are called every frame to draw graphics to the screen.
/// They are executed after Update systems in the game loop.
/// </summary>
public interface IRenderSystem : ISystem
{
    /// <summary>
    /// Called once per frame to perform rendering.
    /// This is where all drawing operations should be performed.
    /// </summary>
    /// <param name="gameTime">Timing information for the current frame (delta time, elapsed time, etc)</param>
    void Render(GameTime gameTime);
}

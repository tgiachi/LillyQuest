using LillyQuest.Core.Primitives;

namespace LillyQuest.Engine.Interfaces.Systems;

/// <summary>
/// Interface for systems that handle game logic and updates.
/// Update systems are called every frame for variable updates and at fixed timesteps for physics.
/// They are executed before Render systems in the game loop.
/// </summary>
public interface IUpdateSystem : ISystem
{
    /// <summary>
    /// Called once per frame to perform variable timestep updates.
    /// This is where most game logic should be implemented (input handling, AI, game state, etc).
    /// Delta time varies based on frame rate.
    /// </summary>
    /// <param name="gameTime">Timing information for the current frame (delta time, elapsed time, etc)</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Called at a fixed timestep for physics and deterministic updates.
    /// This is where physics simulations, collisions, and frame-independent logic should be performed.
    /// Fixed timestep ensures consistent behavior regardless of frame rate.
    /// </summary>
    /// <param name="gameTime">Timing information with fixed delta time for physics</param>
    void FixedUpdate(GameTime gameTime);
}

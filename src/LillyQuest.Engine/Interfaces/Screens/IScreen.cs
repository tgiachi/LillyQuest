using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Input;
using LillyQuest.Engine.Interfaces.Managers;

namespace LillyQuest.Engine.Interfaces.Screens;

/// <summary>
/// Represents a screen in the game UI hierarchy.
/// Screens contain UI entities (GameEntities) similar to how Scenes contain game entities.
/// Screens have position, size, and can consume input events.
/// </summary>
public interface IScreen : IInputConsumer
{
    /// <summary>
    /// Gets the screen's position on the viewport (top-left).
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// Gets the screen's size (width, height).
    /// </summary>
    Vector2 Size { get; }

    /// <summary>
    /// Gets whether this screen is modal (blocks input to screens below it).
    /// </summary>
    bool IsModal { get; }

    /// <summary>
    /// Returns all entities that belong to this screen (UI components).
    /// Used similar to IScene.GetSceneGameObjects().
    /// </summary>
    IEnumerable<IGameEntity> GetScreenGameObjects();

    /// <summary>
    /// Called once when the screen is first created/initialized.
    /// Use this to create UI entities and set up the screen layout.
    /// </summary>
    void OnInitialize(IScreenManager screenManager);

    /// <summary>
    /// Called when the screen becomes active/visible.
    /// </summary>
    void OnLoad();

    /// <summary>
    /// Called when the screen is being deactivated/hidden.
    /// </summary>
    void OnUnload();

    /// <summary>
    /// Called to update the screen and its entities (once per frame).
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    /// Called to render the screen and its entities.
    /// </summary>
    void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext);
}

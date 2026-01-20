using System.Numerics;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Entities;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Input;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using Serilog;
using Silk.NET.Input;

namespace LillyQuest.Engine.Managers.Screens.Base;

/// <summary>
/// Base class for screens that manage UI entities within a defined screen area.
/// Screens handle input dispatch to their child entities and manage entity lifecycles.
/// </summary>
public abstract class BaseScreen : IScreen
{
    private readonly ILogger _logger = Log.ForContext<BaseScreen>();
    private readonly List<IGameEntity> _entities = [];

    /// <summary>
    /// Gets or sets the screen's top-left position on the viewport.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the screen's width and height.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Gets or sets whether this screen is modal (blocks input to screens below).
    /// </summary>
    public bool IsModal { get; set; }

    /// <summary>
    /// Gets or sets whether this screen is currently active and processing input/updates.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the unique identifier for this screen (used for logging and debugging).
    /// </summary>
    public string ConsumerId { get; protected set; }

    protected BaseScreen()
    {
        ConsumerId = GetType().Name;
    }

    /// <summary>
    /// Adds a UI entity to this screen.
    /// </summary>
    public void AddEntity(IGameEntity entity)
    {
        if (!_entities.Contains(entity))
        {
            _entities.Add(entity);
            _logger.Debug("Entity {EntityId} added to screen {ScreenId}", entity.Id, ConsumerId);
        }
    }

    /// <summary>
    /// Clears all entities from this screen.
    /// </summary>
    public void ClearEntities()
    {
        _entities.Clear();
        _logger.Debug("All entities cleared from screen {ScreenId}", ConsumerId);
    }

    /// <summary>
    /// Gets screen entities that implement IInputConsumer.
    /// Used by InputSystem for hierarchical input dispatch.
    /// </summary>
    public IReadOnlyList<IInputConsumer>? GetChildren()
    {
        var consumers = _entities.OfType<IInputConsumer>().ToList();

        return consumers.Count > 0 ? consumers : null;
    }

    /// <summary>
    /// Gets all UI entities contained in this screen.
    /// </summary>
    public IEnumerable<IGameEntity> GetScreenGameObjects()
        => _entities;

    /// <summary>
    /// Performs hit-testing at the given screen coordinates.
    /// Returns true if the point is inside this screen's bounds.
    /// </summary>
    public virtual bool HitTest(int x, int y)
    {
        var minX = (int)Position.X;
        var minY = (int)Position.Y;
        var maxX = minX + (int)Size.X;
        var maxY = minY + (int)Size.Y;

        return x >= minX && x < maxX && y >= minY && y < maxY;
    }

    /// <summary>
    /// Called once when the screen is first created/initialized.
    /// Override this to create UI entities and set up the screen layout.
    /// </summary>
    public virtual void OnInitialize(IScreenManager screenManager)
    {
        _logger.Debug("Screen {ScreenId} initialized", ConsumerId);
    }

    /// <summary>
    /// Called when a key is pressed. Can be overridden for screen-level input handling.
    /// Returns true if the input was consumed (prevents propagation to entities).
    /// </summary>
    public virtual bool OnKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
        => false;

    /// <summary>
    /// Called when a key is released. Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
        => false;

    /// <summary>
    /// Called when a key repeats (after initial delay). Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
        => false;

    /// <summary>
    /// Called when the screen becomes active/visible.
    /// Override to perform startup logic (animations, resource loading, etc).
    /// </summary>
    public virtual void OnLoad()
    {
        _logger.Debug("Screen {ScreenId} loaded", ConsumerId);
    }

    /// <summary>
    /// Called when a mouse button is pressed. Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
        => false;

    /// <summary>
    /// Called when the mouse moves. Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnMouseMove(int x, int y)
        => false;

    /// <summary>
    /// Called when a mouse button is released. Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
        => false;

    /// <summary>
    /// Called when the mouse wheel scrolls. Can be overridden for screen-level input handling.
    /// </summary>
    public virtual bool OnMouseWheel(int x, int y, float delta)
        => false;

    /// <summary>
    /// Called when the screen is being deactivated/hidden.
    /// Override to perform cleanup logic.
    /// </summary>
    public virtual void OnUnload()
    {
        _logger.Debug("Screen {ScreenId} unloaded", ConsumerId);
    }

    /// <summary>
    /// Removes a UI entity from this screen.
    /// </summary>
    public void RemoveEntity(IGameEntity entity)
    {
        if (_entities.Remove(entity))
        {
            _logger.Debug("Entity {EntityId} removed from screen {ScreenId}", entity.Id, ConsumerId);
        }
    }

    /// <summary>
    /// Renders the screen and all its entities.
    /// </summary>
    public virtual void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext)
    {
        spriteBatch.SetScissor((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        foreach (var entity in _entities)
        {
            if (entity is IRenderableEntity renderable && entity.IsActive)
            {
                renderable.Render(spriteBatch, renderContext);
            }
        }

        spriteBatch.DisableScissor();
    }

    /// <summary>
    /// Updates the screen and all its entities each frame.
    /// </summary>
    public virtual void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            if (entity is IUpdateableEntity updateable && entity.IsActive)
            {
                updateable.Update(gameTime);
            }
        }
    }
}

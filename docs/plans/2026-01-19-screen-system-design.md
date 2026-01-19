# Screen System Architecture Design

**Date:** 2026-01-19
**Status:** Approved

## Overview

Sistema di Screen per UI con supporto integrato per positioning, sizing, visibility, input focus, e scissor rendering automatico. Gli Screen sono entità specializzate (`IGameEntity`) che si integrano naturalmente con l'architettura entity-feature esistente.

## Architecture

### Core Concept

Gli **Screen** sono `IGameEntity` specializzati per UI layer con:
- Positioning e sizing (coordinate world space)
- Visibility toggling (IsVisible)
- Input focus globale unico (mouse + keyboard insieme)
- Scissor test automatico per clipping rendering
- Coordinate transformation automatica (world → local)

### Components

**1. Screen class** (extends `BaseGameEntity`)
- Properties: Position (Vector2), Size (Vector2), IsVisible (bool)
- Name ereditato da IGameEntity
- Order ereditato (controlla z-order rendering - manuale)
- Helper methods: WorldToLocal(), LocalToWorld(), ContainsPoint()

**2. ScreenRenderFeature** (implements `IRenderFeature`)
- RenderOrder = int.MinValue (renderizza per primo tra le feature dello screen)
- Applica scissor test automatico usando Position/Size
- Rispetta IsVisible dello screen
- Usa EngineRenderContext.GL per scissor API

**3. ScreenInputFeature** (implements `IMouseInputFeature`, `IKeyboardInputFeature`)
- Flags: IsMouseEnabled, IsKeyboardEnabled
- Trasforma mouse coordinate da world → local (origin top-left)
- Espone eventi "Local" con coordinate trasformate
- Riceve input solo se screen ha focus

**4. InputFocusSystem** (nuovo sistema, Priority 5)
- Mantiene focus globale unico (un solo screen alla volta)
- Hit-test del mouse su screen visibili (bounding box check)
- Click su screen → acquisisce focus automaticamente
- Dispatcha input solo allo screen con focus
- Hit-test da top (Order più alto) a bottom

## Rendering Flow

```
RenderSystem query IRenderFeature (sorted by Entity.Order, then RenderOrder)

  For each Screen (ordered by Entity.Order):
    → ScreenRenderFeature (RenderOrder = int.MinValue)
      → if (!Screen.IsVisible) skip
      → spriteBatch.End()
      → GL.Enable(EnableCap.ScissorTest)
      → GL.Scissor(Position, Size)
      → spriteBatch.Begin()

    → Other Screen Features (RenderOrder > int.MinValue)
      → Render content inside scissor rect
      → Content automatically clipped

  Next Screen or batch end → scissor disabled
```

**Rendering Order Strategy:**
- Entity.Order: Controlla quale screen renderizza sopra (higher = top)
- ScreenRenderFeature.RenderOrder = int.MinValue: Applica scissor per primo
- Other features RenderOrder > int.MinValue: Renderizzano dentro scissor

## Input Flow

```
Silk.NET Input Events (in LillyQuestBootstrap)
  ↓
Mouse Click → InputFocusSystem.HandleMouseClick(x, y)
  ↓
Hit-test screens (top to bottom by Order)
  ↓
Screen hit? → SetFocus(screen)
  ↓
InputFocusSystem.DispatchMouseInput() or DispatchKeyboardInput()
  ↓
ScreenInputFeature (only if focused)
  ↓
Transform world → local coordinates
  ↓
Fire OnLocalMouse*/OnLocalKey* events
  ↓
User code receives local coordinates
```

**Focus Model:**
- Focus globale unico: solo un screen alla volta ha focus
- Click su screen → acquisisce focus (keyboard + mouse insieme)
- Click fuori da tutti gli screen → focus cleared (null)
- Input dispatched solo a focused screen

## Coordinate System

**World Coordinates:**
- Sistema globale della finestra
- Mouse events arrivano in world coordinates
- Screen.Position è in world space

**Local Coordinates:**
- Origin: top-left dello screen
- Local (0,0) = Screen.Position
- Transform: `localPos = worldPos - Screen.Position`

**Helper Methods:**
```csharp
Screen.WorldToLocal(worldPos) → localPos
Screen.LocalToWorld(localPos) → worldPos
Screen.ContainsPoint(worldPos) → bool (hit-test)
Screen.Bounds → Rectangle (computed property)
```

## Implementation Details

### Screen Class

```csharp
public class Screen : BaseGameEntity
{
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public bool IsVisible { get; set; } = true;

    public Rectangle Bounds => new Rectangle(
        (int)Position.X, (int)Position.Y,
        (int)Size.X, (int)Size.Y
    );

    public Vector2 WorldToLocal(Vector2 worldPos) => worldPos - Position;
    public Vector2 LocalToWorld(Vector2 localPos) => localPos + Position;

    public bool ContainsPoint(Vector2 worldPos)
    {
        return worldPos.X >= Position.X &&
               worldPos.X <= Position.X + Size.X &&
               worldPos.Y >= Position.Y &&
               worldPos.Y <= Position.Y + Size.Y;
    }
}
```

### ScreenRenderFeature

```csharp
public class ScreenRenderFeature : IRenderFeature
{
    private readonly Screen _screen;

    public bool IsEnabled { get; set; } = true;
    public int RenderOrder { get; set; } = int.MinValue;

    public void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!_screen.IsVisible) return;

        var gl = spriteBatch.RenderContext.GL;

        spriteBatch.End();
        gl.Enable(EnableCap.ScissorTest);
        gl.Scissor(
            (int)_screen.Position.X,
            (int)_screen.Position.Y,
            (int)_screen.Size.X,
            (int)_screen.Size.Y
        );
        spriteBatch.Begin();
    }
}
```

### ScreenInputFeature

```csharp
public class ScreenInputFeature : IMouseInputFeature, IKeyboardInputFeature
{
    private readonly Screen _screen;

    public bool IsEnabled { get; set; } = true;
    public bool IsMouseEnabled { get; set; } = true;
    public bool IsKeyboardEnabled { get; set; } = true;

    // Events with local coordinates
    public event Action<Vector2, IReadOnlyList<MouseButton>>? OnLocalMouseDown;
    public event Action<Vector2>? OnLocalMouseMove;
    public event Action<Vector2, IReadOnlyList<MouseButton>>? OnLocalMouseUp;
    public event Action<Vector2, float>? OnLocalMouseWheel;

    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyPress;
    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyRelease;
    public event Action<KeyModifierType, IReadOnlyList<Key>>? OnLocalKeyRepeat;

    // IMouseInputFeature - transform and dispatch
    public void OnMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseDown?.Invoke(localPos, buttons);
    }

    public void OnMouseMove(int x, int y)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseMove?.Invoke(localPos);
    }

    public void OnMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseUp?.Invoke(localPos, buttons);
    }

    public void OnMouseWheel(int x, int y, float delta)
    {
        if (!IsMouseEnabled) return;
        var localPos = _screen.WorldToLocal(new Vector2(x, y));
        OnLocalMouseWheel?.Invoke(localPos, delta);
    }

    // IKeyboardInputFeature - dispatch directly
    public void OnKeyPress(KeyModifierType modifiers, IReadOnlyList<Key> keys)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyPress?.Invoke(modifiers, keys);
    }

    public void OnKeyRelease(KeyModifierType modifiers, IReadOnlyList<Key> keys)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyRelease?.Invoke(modifiers, keys);
    }

    public void OnKeyRepeat(KeyModifierType modifiers, IReadOnlyList<Key> keys)
    {
        if (!IsKeyboardEnabled) return;
        OnLocalKeyRepeat?.Invoke(modifiers, keys);
    }
}
```

### InputFocusSystem

```csharp
public class InputFocusSystem : BaseSystem, IUpdateSystem
{
    private readonly ISceneManager _sceneManager;
    private Screen? _focusedScreen;

    public Screen? FocusedScreen => _focusedScreen;

    public InputFocusSystem(
        IGameEntityManager entityManager,
        ISceneManager sceneManager
    ) : base("Input Focus System", 5, entityManager)
    {
        _sceneManager = sceneManager;
    }

    public void Update(GameTime gameTime) { }
    public void FixedUpdate(GameTime gameTime) { }

    public void HandleMouseClick(int x, int y)
    {
        var clickPos = new Vector2(x, y);
        var currentScene = _sceneManager.CurrentScene;
        if (currentScene == null) return;

        // Get screens sorted by Order (highest first - top-most)
        var screens = currentScene.GetSceneGameEntities()
            .OfType<Screen>()
            .Where(s => s.IsVisible)
            .OrderByDescending(s => s.Order)
            .ToList();

        // Hit-test from top to bottom
        foreach (var screen in screens)
        {
            if (screen.ContainsPoint(clickPos))
            {
                SetFocus(screen);
                return;
            }
        }

        // No screen hit - clear focus
        SetFocus(null);
    }

    public void SetFocus(Screen? screen)
    {
        if (_focusedScreen == screen) return;
        _focusedScreen = screen;
    }

    public void DispatchMouseInput(int x, int y, Action<ScreenInputFeature> action)
    {
        if (_focusedScreen == null) return;

        if (_focusedScreen.TryGetFeature<ScreenInputFeature>(out var inputFeature))
        {
            if (inputFeature.IsEnabled && inputFeature.IsMouseEnabled)
            {
                action(inputFeature);
            }
        }
    }

    public void DispatchKeyboardInput(Action<ScreenInputFeature> action)
    {
        if (_focusedScreen == null) return;

        if (_focusedScreen.TryGetFeature<ScreenInputFeature>(out var inputFeature))
        {
            if (inputFeature.IsEnabled && inputFeature.IsKeyboardEnabled)
            {
                action(inputFeature);
            }
        }
    }
}
```

### Bootstrap Integration

In `LillyQuestBootstrap.cs`, aggiungi input wiring:

```csharp
private void SetupInputHandlers()
{
    var mouse = RenderContext.InputContext.Mice[0];
    var keyboard = RenderContext.InputContext.Keyboards[0];

    // Mouse events
    mouse.MouseDown += (mouse, button) =>
    {
        var pos = mouse.Position;
        _inputFocusSystem.HandleMouseClick((int)pos.X, (int)pos.Y);
        _inputFocusSystem.DispatchMouseInput((int)pos.X, (int)pos.Y,
            feature => feature.OnMouseDown((int)pos.X, (int)pos.Y, new[] { button }));
    };

    mouse.MouseMove += (mouse, pos) =>
    {
        _inputFocusSystem.DispatchMouseInput((int)pos.X, (int)pos.Y,
            feature => feature.OnMouseMove((int)pos.X, (int)pos.Y));
    };

    mouse.MouseUp += (mouse, button) =>
    {
        var pos = mouse.Position;
        _inputFocusSystem.DispatchMouseInput((int)pos.X, (int)pos.Y,
            feature => feature.OnMouseUp((int)pos.X, (int)pos.Y, new[] { button }));
    };

    mouse.Scroll += (mouse, wheel) =>
    {
        var pos = mouse.Position;
        _inputFocusSystem.DispatchMouseInput((int)pos.X, (int)pos.Y,
            feature => feature.OnMouseWheel((int)pos.X, (int)pos.Y, wheel.Y));
    };

    // Keyboard events
    keyboard.KeyDown += (keyboard, key, scancode) =>
    {
        _inputFocusSystem.DispatchKeyboardInput(
            feature => feature.OnKeyPress(KeyModifierType.None, new[] { key }));
    };

    keyboard.KeyUp += (keyboard, key, scancode) =>
    {
        _inputFocusSystem.DispatchKeyboardInput(
            feature => feature.OnKeyRelease(KeyModifierType.None, new[] { key }));
    };
}
```

## Usage Example

```csharp
public class GameScene : BaseScene
{
    public override void OnLoad()
    {
        // Create screen
        var menuScreen = new Screen
        {
            Name = "MainMenu",
            Position = new Vector2(100, 100),
            Size = new Vector2(400, 300),
            Order = 100  // Z-order: higher = on top
        };

        // Add scissor/render feature
        var renderFeature = new ScreenRenderFeature(menuScreen)
        {
            RenderOrder = int.MinValue  // Apply scissor first
        };
        menuScreen.AddFeature(renderFeature);

        // Add custom content render feature
        var contentFeature = new MyMenuRenderFeature()
        {
            RenderOrder = 0  // Render after scissor applied
        };
        menuScreen.AddFeature(contentFeature);

        // Add input feature
        var inputFeature = new ScreenInputFeature(menuScreen);
        inputFeature.OnLocalMouseDown += (localPos, buttons) =>
        {
            Console.WriteLine($"Clicked at local: {localPos}");
            // Handle click in local coordinate space
        };
        menuScreen.AddFeature(inputFeature);

        // Add to scene
        AddEntity(menuScreen);
    }
}
```

## Integration with Existing Systems

**Scene Management:**
- Screen è normale entity, gestito da scene come qualsiasi entity
- PushScene/PopScene funziona normalmente con screen
- Screen entities aggiunti/rimossi dal scene lifecycle

**Rendering System:**
- RenderSystem già ordina feature per Entity.Order poi RenderOrder
- ScreenRenderFeature.RenderOrder = int.MinValue garantisce scissor first
- Content features hanno RenderOrder > int.MinValue

**Entity Manager:**
- Screen registrato come normale entity
- Feature indicizzate automaticamente
- Query funzionano normalmente

**Input Systems:**
- InputFocusSystem è nuovo sistema (Priority 5)
- Si integra con Silk.NET input via Bootstrap
- Dispatcha a ScreenInputFeature esistenti

## Edge Cases

**Overlapping Screens:**
- Hit-test da top (Order più alto) a bottom
- Primo screen hit ottiene focus
- Screen sotto non ricevono input

**Invisible Screens:**
- IsVisible = false → skip rendering (ScreenRenderFeature)
- Hit-test ignora screen invisibili
- Non possono ricevere focus

**No Focus:**
- FocusedScreen = null → nessun input dispatched
- Click fuori da tutti screen → clear focus

**Screen Hierarchy:**
- Screen può essere parent/child di altri screen
- Order controlla rendering (non hierarchy)
- Focus indipendente da hierarchy

**Scissor Stacking:**
- Ogni ScreenRenderFeature applica proprio scissor
- Non nested scissor (ogni screen indipendente)
- Scissor cleared implicitamente dal prossimo screen

## Files to Create/Modify

**New Files:**
- `src/LillyQuest.Engine/Entities/Screen.cs`
- `src/LillyQuest.Engine/Features/ScreenRenderFeature.cs`
- `src/LillyQuest.Engine/Features/ScreenInputFeature.cs`
- `src/LillyQuest.Engine/Systems/InputFocusSystem.cs`

**Modified Files:**
- `src/LillyQuest.Engine/LillyQuestBootstrap.cs` - Add input wiring
- `src/LillyQuest.Engine/LillyQuestBootstrap.cs` - Register InputFocusSystem

## Testing Strategy

**Unit Tests:**
- Screen.WorldToLocal / LocalToWorld coordinate transform
- Screen.ContainsPoint hit-test
- InputFocusSystem.HandleMouseClick focus logic
- ScreenInputFeature coordinate transformation

**Integration Tests:**
- Screen rendering with scissor
- Multiple screens with different Order
- Input focus switching between screens
- IsVisible = false behavior

**Manual Tests:**
- Click on overlapping screens
- Keyboard input to focused screen
- Scissor clipping visual verification
- Screen visibility toggle

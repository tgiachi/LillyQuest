# Multi-Scene Stack Architecture Design

**Date:** 2026-01-19
**Status:** Approved

## Overview

Replace the single-scene-at-a-time model with a stack-based scene management system. This enables:
- Game scene to stay frozen (render only, no update) in background
- Menu scenes to overlay on top
- Smooth fade transitions when switching between complete scenes
- Natural pause behavior (ESC opens menu, game pauses visually)

## Architecture

### Dual-Mode Operations

**SwitchScene(sceneName)** - Complete scene replacement with fade transition
```
[GameScene] → [fade out] → [GameScene2]
Stack after: [GameScene2]
```
- Clears the entire stack
- Triggers fade transition effect
- OnUnload called on previous scene
- OnLoad called on new scene
- All previous scenes removed from memory management

**PushScene(sceneName)** - Add scene to stack without transition
```
[GameScene2, MenuScene]
```
- Previous scene moves behind current scene (paused)
- No fade effect (instant appearance)
- OnLoad called on new scene
- Previous scene stays in update/render loop but paused

**PopScene()** - Remove top scene, return to previous
```
[GameScene2, MenuScene] → [GameScene2]
```
- Top scene removed from stack
- OnUnload called on removed scene
- Previous scene becomes active again (resumes from frozen state)

### Rendering Strategy

RenderSystem renders all scenes in stack from bottom to top:
- Game scene renders first (appears in background)
- Menu scene renders on top (appears in foreground)
- Creates natural layering effect

**Pseudocode:**
```csharp
foreach (var scene in sceneStack.Reverse())
{
    foreach (var entity in scene.GetSceneGameEntities())
    {
        RenderEntity(entity);
    }
}
```

### Update Strategy

UpdateSystem only updates the top scene in the stack:
- Top scene: updates all entities
- Background scenes: no updates (frozen)
- No background processing or animations in paused scenes

**Pseudocode:**
```csharp
if (sceneManager.CurrentScene != null)
{
    foreach (var entity in sceneManager.CurrentScene.GetSceneGameEntities())
    {
        UpdateEntity(entity);
    }
}
```

## Scene Lifecycle

### Initialization (Bootstrap)
```csharp
RegisterScene<GameScene>();      // OnInitialize() called once
RegisterScene<MenuScene>();      // OnInitialize() called once
RegisterScene<SettingsScene>();  // OnInitialize() called once
```

All scenes initialized once at bootstrap as singletons.

### PushScene Lifecycle
```
1. Get scene from registered scenes
2. Call scene.OnLoad()
3. Add scene entities to EntityManager
4. Scene becomes CurrentScene (top of stack)
```

### PopScene Lifecycle
```
1. Call scene.OnUnload()
2. Remove scene entities from EntityManager
3. Previous scene becomes CurrentScene
```

### SwitchScene Lifecycle (with fade)
```
1. Call current scene.OnUnload()
2. Remove current scene entities
3. Clear entire stack
4. Play fade transition effect
5. Call new scene.OnLoad()
6. Add new scene entities
7. Push new scene to now-empty stack
```

## State Preservation

When a scene is popped from the stack:
- All game state preserved in memory
- Entities maintain their state
- Next push of that scene resumes from exact state
- No need to re-initialize or reload data

Example: Game scene pauses when Menu pushed, resumes exactly when Menu popped

## Global Entities

RegisterGlobals() behavior:
- Called when scene first pushed/switched to
- Persists across all scenes in stack
- Not called again if scene popped and pushed back (already registered)
- Survives entire session

## Edge Cases

**Empty Stack:**
- CurrentScene returns null
- UpdateSystem and RenderSystem handle null gracefully (skip processing)

**Duplicate Scenes in Stack:**
- Same scene type can appear multiple times
- Each instance has separate OnLoad/OnUnload cycle
- Example: [GameScene, MenuScene, SettingsScene] (nested menus)

**Unregistered Scene:**
- PushScene/SwitchScene with unregistered scene throws exception
- Forces scenes to be registered at bootstrap (no lazy loading)

**Input Handling:**
- ImGuiSystem renders on top (priority 1000) with input capture
- Only top scene updates, so receives all input events
- Background scenes don't process input

## API

```csharp
// SceneManager interface
void PushScene(string sceneName);              // Add to stack, no transition
void PopScene();                               // Remove top scene
void SwitchScene(string sceneName);            // Clear stack, fade transition, push
IScene CurrentScene { get; }                   // Top of stack (or null)
IReadOnlyList<IScene> SceneStack { get; }      // All scenes in stack order
```

## System Changes Required

**RenderSystem:**
- Loop through all scenes in stack (bottom to top)
- Render each scene's entities

**UpdateSystem:**
- Only update CurrentScene's entities

**SceneManager:**
- Replace single-scene with Stack<IScene>
- Remove SceneTransitionState machine (use for SwitchScene fade only)
- Implement PushScene, PopScene, SwitchScene
- Maintain scene lifecycle callbacks

**No Changes Needed:**
- EntityManager
- SystemManager
- GameEntity or Feature system
- Bootstrap game loop

## Use Cases

### Use Case 1: Press ESC to Open Pause Menu
```
Initial: Stack = [GameScene]
Press ESC:
  1. PushScene("PauseMenuScene")
  2. Stack = [GameScene, PauseMenuScene]
  3. GameScene renders (frozen background)
  4. PauseMenuScene renders + updates
  5. PauseMenuScene captures input

Press ESC or Resume:
  1. PopScene()
  2. Stack = [GameScene]
  3. GameScene resumes updates from exact state
```

### Use Case 2: Switch to Different Level
```
Initial: Stack = [GameLevel1]
Select Level 2:
  1. SwitchScene("GameLevel2")
  2. GameLevel1 unloads (fade effect)
  3. Stack = [GameLevel2]
  4. GameLevel2 loads with fade
  5. Play new level
```

### Use Case 3: Nested Menus
```
Initial: Stack = [GameScene]
Press ESC → Settings:
  1. PushScene("PauseMenu")
  2. Stack = [GameScene, PauseMenu]
  3. Press Settings:
  4. PushScene("SettingsMenu")
  5. Stack = [GameScene, PauseMenu, SettingsMenu]
  6. Back from Settings:
  7. PopScene()
  8. Stack = [GameScene, PauseMenu]
  9. Back from Pause:
  10. PopScene()
  11. Stack = [GameScene]
```

## Implementation Priority

1. Refactor SceneManager to use Stack<IScene>
2. Implement PushScene, PopScene, SwitchScene
3. Update RenderSystem to loop through stack
4. Update UpdateSystem to only update top scene
5. Adapt scene lifecycle (OnLoad/OnUnload) for stack operations
6. Test nested scenes and state preservation
7. Add input handling to prevent background scene input

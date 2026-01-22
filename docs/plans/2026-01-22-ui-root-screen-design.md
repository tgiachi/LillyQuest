# UI Root Screen Design

Date: 2026-01-22

## Goal
Remove UIScreenOverlay and manage all UI controls via a single UIRootScreen that lives in the ScreenManager stack, so focus and input routing are handled in one place without an extra overlay layer.

## Architecture Summary
- Introduce `UIRootScreen : BaseScreen` that hosts a `UIScreenRoot` and dispatches input to `UIScreenControl` children.
- `UIScreenControl` remains a lightweight UI element with parent/child relationships for layout and hit-testing.
- ScreenManager keeps a unified stack of screens; UI is just another screen, typically the top-most.
- Input routing: ScreenManager sends input to the top screen; if not consumed, it can propagate to the next screen (existing behavior).

## Components
### UIRootScreen
- Inherits `BaseScreen` (scissor/size already supported).
- Holds `UIScreenRoot Root`.
- Tracks `UIScreenControl? ActiveControl` for mouse capture.
- Input behavior:
  - OnMouseDown → hit-test Root (top-most control) → if handled, set ActiveControl.
  - OnMouseMove/OnMouseUp → forward to ActiveControl while set.
- Render → draw controls in ZIndex order (skip invisible).

### UIScreenControl
- No change to role (parent/children remain).
- Handles its own input and rendering.

### ScreenManager
- Remove all UIScreenOverlay special handling.
- Treat UIRootScreen as a normal screen in the stack.
- Retain pass-through logic: if top screen does not consume, route to next.

## Migration
- Remove `UIScreenOverlay` and its tests.
- Add `UIRootScreen` and tests.
- Update scenes to push `UIRootScreen` instead of `UIScreenOverlay`.

## Risks
- Any feature relying on overlay-specific behavior must be migrated.
- Input capture must correctly forward move/up to maintain drag behavior.

## Testing
- Tests for UIRootScreen mouse capture and render ordering.
- Update ScreenManager tests if needed for pass-through.
- Update scene demos to use UIRootScreen.

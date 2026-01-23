# UI Button + StackPanel Design

## Summary
Introduce a nine-slice `UIButton` control with hover/click states, tint transitions, and centered text, plus a `UIStackPanel` layout container with vertical/horizontal orientation, spacing, padding, and cross-axis alignment. Add a sample `UINinePatchWindow` with a button in `TilesetSurfaceEditorScene`.

## Goals
- `UIButton` renders a 9-slice background with configurable font and centered label.
- Button states: `Idle`, `Hovered`, `Pressed` (clicked while mouse down).
- State changes use `TransitionTime` to lerp tint colors (0 = immediate).
- Events: `OnClick` and `OnHover` (fire on hover enter).
- `UIStackPanel` arranges children vertically/horizontally with spacing + padding.
- Cross-axis alignment: `Left`, `Center`, `Right` (relative to panel size).
- Hierarchy: all new controls support `Parent` (already in `UIScreenControl`) and `Children` (explicit list in container controls; button exposes list for hierarchy even if unused).
- Add a new `UINinePatchWindow` example with a `UIButton` in `TilesetSurfaceEditorScene`.

## Non-goals
- Rich layout system (no flex/wrap).
- Keyboard focus/activation shortcuts.
- Multiple hover callbacks per frame (only enter event).

## Architecture
- **UIButton**: `UIScreenControl` subclass with NineSlice rendering (via `INineSliceAssetManager` + `ITextureManager`), state machine, tint interpolation, and centered text.
- **UIStackPanel**: `UIScreenControl` subclass + children list; layout logic applies positions on add/remove and when properties change.
- **Hierarchy**: `UIStackPanel` manages `Children`, assigns `Parent` on add; `UIButton` exposes `Children` list for hierarchy compliance (no layout by default). `UIWindow` remains a free-position container.

## UIButton Design
**Properties**
- `string NineSliceKey`
- `float NineSliceScale`
- `string Text`, `string FontName`, `int FontSize`, `LyColor TextColor`
- `LyColor IdleTint`, `HoveredTint`, `PressedTint`
- `float TransitionTime`
- `Action? OnClick`, `Action? OnHover`

**State**
- `enum UIButtonState { Idle, Hovered, Pressed }`
- `LyColor _currentTint`, `LyColor _targetTint`, `UIButtonState _state`
- `float _transitionElapsed`

**Input**
- `HandleMouseMove`: detect hover enter; update state to Hovered/Idle; fire `OnHover` on enter.
- `HandleMouseDown`: if inside -> Pressed; return true.
- `HandleMouseUp`: if Pressed and inside -> invoke `OnClick`; state becomes Hovered if still inside, otherwise Idle.

**Tint Transition**
- If `TransitionTime <= 0`, set `_currentTint = _targetTint` immediately.
- Else lerp per frame/time; use `renderContext.GameTime` or `Update` call (decide in impl plan).

**Render**
- Nine-slice draw using `_currentTint` for all slices.
- Text centered in `Size` (draw at `GetWorldPosition() + (Size - textSize)/2`). If text size not easily measurable, approximate by offsetting with half font height; keep consistent with existing font drawing patterns.

## UIStackPanel Design
**Properties**
- `enum UIStackOrientation { Vertical, Horizontal }`
- `enum UICrossAlignment { Left, Center, Right }`
- `Vector4 Padding` (Left, Top, Right, Bottom)
- `float Spacing`

**Layout**
- Vertical: accumulate Y from `Padding.Top`, apply `Spacing` between children. Cross-axis uses `Padding.Left/Right` and `Size.X`.
- Horizontal: accumulate X from `Padding.Left`, apply `Spacing` between children. Cross-axis uses `Padding.Top/Bottom` and `Size.Y`.
- For each child, set `Position` relative to panel origin; do not overwrite child `Size`.

**Update Strategy**
- `ApplyLayout()` called on `Add`, `Remove`, and property setters (or explicit `InvalidateLayout` + call in `Render`). Prefer direct call for simplicity.

## Error Handling / Edge Cases
- If panel size is smaller than child size, alignment clamps to padding bounds.
- Button without `NineSliceKey` renders nothing (consistent with nine-slice window behavior).
- Hover detection uses bounds from `GetBounds()`.

## Tests
- `UIButton`:
  - Hover enter sets state + fires `OnHover` once.
  - Click invokes `OnClick` when mouse down/up inside.
  - Tint transitions move toward target (transition > 0) and immediate when 0.
- `UIStackPanel`:
  - Vertical layout with spacing/padding.
  - Horizontal layout with spacing/padding.
  - Cross-axis alignment Left/Center/Right.

## Example Usage
- Add a second `UINinePatchWindow` in `TilesetSurfaceEditorScene` containing a sample `UIButton` with `NineSliceKey` and `Text`.


# UIScrollContent Design

**Goal:** Add a scrollable UI container with vertical + horizontal scrollbars using texture patches from `INineSliceAssetManager`.

## Overview
`UIScrollContent` is a `UIScreenControl` that renders a clipped viewport and offsets child rendering by `ScrollOffset`. It renders optional scrollbars using texture patches (track/thumb). Input supports mouse wheel scrolling and thumb drag. Content size is independent from viewport size and determines scroll range.

## Core Concepts
- **Viewport:** The visible region inside the control bounds. If scrollbars are enabled, the viewport is reduced by scrollbar thickness.
- **Content Size:** Logical size of child content (`ContentSize`).
- **Scroll Offset:** The current scroll position, clamped to `[0, MaxOffset]`.
- **Patches:** Scrollbar visuals come from texture patches (e.g., `scroll.v.track`, `scroll.v.thumb`).

## Public API (Proposed)

```csharp
public sealed class UIScrollContent : UIScreenControl
{
    public Vector2 ContentSize { get; set; }
    public Vector2 ScrollOffset { get; set; }

    public bool EnableVerticalScroll { get; set; } = true;
    public bool EnableHorizontalScroll { get; set; } = true;
    public float ScrollSpeed { get; set; } = 24f;

    public float ScrollbarThickness { get; set; } = 12f;
    public float MinThumbSize { get; set; } = 16f;

    public string ScrollbarTextureName { get; set; } = string.Empty;
    public string VerticalTrackElement { get; set; } = "scroll.v.track";
    public string VerticalThumbElement { get; set; } = "scroll.v.thumb";
    public string HorizontalTrackElement { get; set; } = "scroll.h.track";
    public string HorizontalThumbElement { get; set; } = "scroll.h.thumb";

    public IReadOnlyList<UIScreenControl> Children { get; }
    public void Add(UIScreenControl control);
    public void Remove(UIScreenControl control);
}
```

## Rendering Behavior
1. Compute viewport rectangle from `Size`, subtracting scrollbar thickness if enabled.
2. Set scissor to viewport.
3. Render children in Z-order with translation `-ScrollOffset`.
4. Disable scissor.
5. Render scrollbars on top (track then thumb) using patches from `INineSliceAssetManager`.

## Scrollbar Geometry
- `MaxOffset.X = max(0, ContentSize.X - ViewportSize.X)`
- `MaxOffset.Y = max(0, ContentSize.Y - ViewportSize.Y)`
- Thumb length (v/h): `thumb = max(MinThumbSize, viewportLen * (viewportLen / contentLen))`
- Thumb position: `thumbPos = trackStart + (offset / maxOffset) * (trackLen - thumbLen)`

## Input Behavior
- Mouse wheel: scroll Y by `delta * ScrollSpeed`.
- If Shift pressed (or horizontal enabled + vertical disabled), scroll X.
- Thumb drag: map mouse delta in track space to scroll offset delta.

## Error Handling
- If texture patches are missing, skip drawing that part.
- If `ScrollbarTextureName` is empty, skip scrollbar rendering entirely.

## Testing
- Unit test for geometry: given `ContentSize` + `ViewportSize`, verify computed `MaxOffset` and thumb size.
- Unit test for patch retrieval path (missing patch should not throw if in render path).


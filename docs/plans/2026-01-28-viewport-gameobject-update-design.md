# Viewport-Scoped GameObject Update Design

## Goal
Add `Update(GameTime)` behavior for game objects while avoiding global per-tick cost by updating only objects inside the **tile viewport**. When an object changes its appearance during update (e.g., animated torch), the change must be visible via the existing chunk-based render rebuild.

## Summary of Approach (Recommended: Option A)
Extend the current render pipeline with a viewport-aware update pass:
- A system (new `ViewportUpdateSystem` or an extension of `MapRenderSystem`) computes the **tile viewport bounds** each tick.
- Only objects **inside the viewport** receive `Update(GameTime)`.
- Objects that change appearance call into the system to **mark their tile/chunk dirty** so the render rebuild reflects the update.

This keeps updates proportional to visible content and integrates with the chunk-based redraw flow.

## Architecture
### Systems
- **MapRenderSystem** (already exists): tracks dirty chunks and rebuilds them during Update.
- **ViewportUpdateSystem** (new, or merged into MapRenderSystem):
  - Computes viewport bounds in tile coordinates.
  - Iterates map objects and invokes `Update(GameTime)` for visible ones.
  - Provides `MarkDirtyForTile` and/or `MarkDirtyForObject` for appearance changes.

### Contracts
- **IViewportUpdateable** (new interface):
  - `void Update(GameTime gameTime)`
  - Optional: `OnEnterViewport` / `OnExitViewport` if needed later.

Game objects that need animation (e.g., torches) implement `IViewportUpdateable`. Regular objects do nothing.

## Viewport Bounds (Tile Space)
Viewport is defined in tiles, not pixels:
- Use `TilesetSurfaceScreen`/layer view data to compute a tile rectangle: `minX, minY, maxX, maxY`.
- This rectangle is the **visibility window** for updates.

If a helper like `GetVisibleTileBounds(layerIndex)` doesn’t exist, it can be added to compute bounds from:
- `TileViewSize`
- View center/offset
- Layer render scale (if relevant)

## Data Flow
1. **Tick:** `ViewportUpdateSystem` computes viewport tile rect.
2. **Visibility filter:** Only map objects inside the rect are considered.
3. **Update:** Call `Update(GameTime)` for visible objects implementing `IViewportUpdateable`.
4. **Appearance change:** Object updates its `VisualTile` (symbol/colors).
5. **Dirty marking:** Object calls `MarkDirtyForTile(x,y)` to force redraw.
6. **Render rebuild:** `MapRenderSystem.Update` rebuilds dirty chunks next tick.

## Example: Torch
- Torch is visible → `Update(GameTime)` cycles frame / color.
- Torch calls `MarkDirtyForTile` for its position.
- Chunk rebuild updates the tile in surface → animation visible in viewport.

## Alternatives Considered
- **GoRogue Components:** useful for modular behaviors, but viewport filtering must still be done in our system. Components can be used to implement `IViewportUpdateable` if desired.
- **Update all objects:** simple, but wastes CPU and violates the “visible only” requirement.

## Testing
- Unit test for viewport bounds calculation (tile-space).
- Unit test that only objects inside viewport are updated.
- Integration test: object updates appearance and triggers dirty chunk; tile changes visible on rebuild.

## Open Questions
- Should viewport bounds be calculated by `TilesetSurfaceScreen` or by the new system?
- Should `IViewportUpdateable` live in Engine or Game layer?

# Viewport GameObject Update Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Update GameObjects each tick only when they are inside the tile viewport, and ensure appearance changes trigger dirty chunk rebuilds.

**Architecture:** Add a `ViewportUpdateSystem` (GameEntity + IUpdateableEntity) that computes tile viewport bounds from `TilesetSurfaceScreen`, iterates objects only inside that rect, calls `Update(GameTime)` on objects implementing `IViewportUpdateable`, and marks their tiles dirty via `MapRenderSystem`.

**Tech Stack:** C# (.NET 10), LillyQuest Engine (GameEntity/IUpdateableEntity), GoRogue Map, TilesetSurfaceScreen.

---

### Task 1: Add IViewportUpdateable interface

**Files:**
- Create: `src/LillyQuest.RogueLike/GameObjects/IViewportUpdateable.cs`
- Test: `tests/LillyQuest.Tests/RogueLike/GameObjects/IViewportUpdateableTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void ViewportUpdateable_Interface_CanBeImplemented()
{
    var obj = new TestViewportObject(new Point(1, 1));
    obj.Update(new GameTime());

    Assert.That(obj.UpdateCount, Is.EqualTo(1));
}

private sealed class TestViewportObject : CreatureGameObject, IViewportUpdateable
{
    public int UpdateCount { get; private set; }

    public TestViewportObject(Point position) : base(position) { }

    public void Update(GameTime gameTime)
        => UpdateCount++;
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~IViewportUpdateableTests.ViewportUpdateable_Interface_CanBeImplemented`

Expected: FAIL (IViewportUpdateable missing)

**Step 3: Write minimal implementation**

Create `IViewportUpdateable` interface:

```csharp
public interface IViewportUpdateable
{
    void Update(GameTime gameTime);
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~IViewportUpdateableTests.ViewportUpdateable_Interface_CanBeImplemented`

Expected: PASS

**Step 5: Commit**

```bash
git add src/LillyQuest.RogueLike/GameObjects/IViewportUpdateable.cs tests/LillyQuest.Tests/RogueLike/GameObjects/IViewportUpdateableTests.cs
git commit -m "feat: add viewport updateable interface"
```

---

### Task 2: Add viewport bounds helper in ViewportUpdateSystem

**Files:**
- Create: `src/LillyQuest.Game/Systems/ViewportUpdateSystem.cs`
- Create: `tests/LillyQuest.Tests/Game/Systems/ViewportUpdateSystemTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void GetViewportBounds_UsesTileViewSizeAndOffset()
{
    var screen = BuildTestSurface();
    screen.TileViewSize = new Vector2(10, 6);
    screen.SetLayerViewTileOffset(0, new Vector2(3, 4));

    var bounds = ViewportUpdateSystem.GetViewportBounds(screen, layerIndex: 0);

    Assert.That(bounds.MinX, Is.EqualTo(3));
    Assert.That(bounds.MinY, Is.EqualTo(4));
    Assert.That(bounds.MaxX, Is.EqualTo(12)); // 3 + 10 - 1
    Assert.That(bounds.MaxY, Is.EqualTo(9));  // 4 + 6 - 1
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~ViewportUpdateSystemTests.GetViewportBounds_UsesTileViewSizeAndOffset`

Expected: FAIL (ViewportUpdateSystem missing)

**Step 3: Write minimal implementation**

Create `ViewportUpdateSystem` with a static bounds helper and a small bounds struct:

```csharp
public readonly record struct TileViewportBounds(int MinX, int MinY, int MaxX, int MaxY);

public static TileViewportBounds GetViewportBounds(TilesetSurfaceScreen screen, int layerIndex)
{
    var offset = screen.GetLayerViewTileOffset(layerIndex);
    var minX = (int)MathF.Floor(offset.X);
    var minY = (int)MathF.Floor(offset.Y);
    var maxX = minX + (int)screen.TileViewSize.X - 1;
    var maxY = minY + (int)screen.TileViewSize.Y - 1;
    return new(minX, minY, maxX, maxY);
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~ViewportUpdateSystemTests.GetViewportBounds_UsesTileViewSizeAndOffset`

Expected: PASS

**Step 5: Commit**

```bash
git add src/LillyQuest.Game/Systems/ViewportUpdateSystem.cs tests/LillyQuest.Tests/Game/Systems/ViewportUpdateSystemTests.cs
git commit -m "feat: add viewport bounds helper"
```

---

### Task 3: Update only visible objects and mark dirty tiles

**Files:**
- Modify: `src/LillyQuest.Game/Systems/ViewportUpdateSystem.cs`
- Modify: `tests/LillyQuest.Tests/Game/Systems/ViewportUpdateSystemTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void Update_OnlyUpdatesObjectsInsideViewport_AndMarksDirty()
{
    var map = BuildTestMap();
    var screen = BuildTestSurface();
    screen.TileViewSize = new Vector2(4, 4);
    screen.SetLayerViewTileOffset(0, new Vector2(0, 0));

    var renderSystem = new MapRenderSystem(chunkSize: 4);
    renderSystem.RegisterMap(map, screen, fovService: null);

    var system = new ViewportUpdateSystem(layerIndex: 0);
    system.RegisterMap(map, screen, renderSystem);

    var inside = new TestViewportObject(new Point(1, 1));
    var outside = new TestViewportObject(new Point(10, 10));
    map.AddEntity(inside);
    map.AddEntity(outside);

    system.Update(new GameTime());

    Assert.That(inside.UpdateCount, Is.EqualTo(1));
    Assert.That(outside.UpdateCount, Is.EqualTo(0));
    Assert.That(renderSystem.GetDirtyChunks(map), Does.Contain(new ChunkCoord(0, 0)));
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~ViewportUpdateSystemTests.Update_OnlyUpdatesObjectsInsideViewport_AndMarksDirty`

Expected: FAIL (Update not implemented)

**Step 3: Write minimal implementation**

Implement `ViewportUpdateSystem.Update`:
- Compute viewport bounds.
- Iterate tile positions within bounds (clamped to map).
- For each object at position, if it implements `IViewportUpdateable`, call `Update`.
- After update, call `renderSystem.MarkDirtyForTile` for that position.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~ViewportUpdateSystemTests.Update_OnlyUpdatesObjectsInsideViewport_AndMarksDirty`

Expected: PASS

**Step 5: Commit**

```bash
git add src/LillyQuest.Game/Systems/ViewportUpdateSystem.cs tests/LillyQuest.Tests/Game/Systems/ViewportUpdateSystemTests.cs
git commit -m "feat: update visible objects and mark dirty tiles"
```

---

### Task 4: Wire ViewportUpdateSystem into RogueScene

**Files:**
- Modify: `src/LillyQuest.Game/Scenes/RogueScene.cs`
- Modify: `tests/LillyQuest.Tests/Game/Scenes/RogueSceneTests.cs`

**Step 1: Write the failing test**

```csharp
[Test]
public void RogueScene_RegistersViewportUpdateSystem()
{
    var scene = CreateSceneWithFakeServices();

    scene.OnLoad();

    Assert.That(scene.GetSceneGameObjects().OfType<ViewportUpdateSystem>().Any(), Is.True);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~RogueSceneTests.RogueScene_RegistersViewportUpdateSystem`

Expected: FAIL

**Step 3: Write minimal implementation**

- Instantiate `ViewportUpdateSystem` in `RogueScene`.
- Register map/screen/renderSystem on load.
- Add to scene entities.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~RogueSceneTests.RogueScene_RegistersViewportUpdateSystem`

Expected: PASS

**Step 5: Commit**

```bash
git add src/LillyQuest.Game/Scenes/RogueScene.cs tests/LillyQuest.Tests/Game/Scenes/RogueSceneTests.cs
git commit -m "feat: add viewport update system to rogue scene"
```

---

### Task 5: Full test run

Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj`

Expected: PASS (existing warnings ok)

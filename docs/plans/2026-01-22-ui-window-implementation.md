# UI Window Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a pixel-based `UIWindow` control with optional title bar, draggable movement with parent clamp, and child controls support.

**Architecture:** Implement `UIWindow : UIScreenControl` in the UI screen layer; it renders via SpriteBatch, handles input for dragging and focus, and renders/dispatches to child controls. Tests cover dragging, clamping, titlebar behavior, and child hit-test ordering.

**Tech Stack:** C# (.NET 10), LillyQuest Engine UI layer, xUnit tests.

### Task 1: Add UIWindow tests (titlebar + drag)

**Files:**
- Create: `tests/LillyQuest.Tests/Engine/UI/UIWindowTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public void MouseDown_TitleBar_StartsDrag_WhenMovable()
{
    var window = new UIWindow { Position = Vector2.Zero, Size = new Vector2(100, 50), IsTitleBarEnabled = true, IsWindowMovable = true };
    window.TitleBarHeight = 10;

    var handled = window.OnMouseDownInternal(new Vector2(5, 5));
    var moved = window.OnMouseMoveInternal(new Vector2(20, 20));

    Assert.True(handled);
    Assert.True(moved);
    Assert.Equal(new Vector2(15, 15), window.Position);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: FAIL (UIWindow missing).

**Step 3: Write minimal implementation**
Create `UIWindow` with internal helpers used by tests (`OnMouseDownInternal`, `OnMouseMoveInternal`) or expose by virtual overrides; make test use public methods (final shape determined by implementation).

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: PASS.

**Step 5: Commit**
```bash
git add tests/LillyQuest.Tests/Engine/UI/UIWindowTests.cs
git commit -m "test: add UIWindow drag test"
```

### Task 2: Add UIWindow tests (clamp to parent + child hit testing)

**Files:**
- Modify: `tests/LillyQuest.Tests/Engine/UI/UIWindowTests.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public void Drag_Clamps_To_Parent_Bounds()
{
    var parent = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(100, 100) };
    var window = new UIWindow { Position = Vector2.Zero, Size = new Vector2(40, 40), Parent = parent, IsTitleBarEnabled = true, IsWindowMovable = true };
    window.TitleBarHeight = 10;

    window.OnMouseDownInternal(new Vector2(5, 5));
    window.OnMouseMoveInternal(new Vector2(200, 200));

    Assert.Equal(new Vector2(60, 60), window.Position);
}

[Fact]
public void MouseDown_Delegates_To_Children_Topmost_First()
{
    var window = new UIWindow { Position = Vector2.Zero, Size = new Vector2(100, 50) };
    var a = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(100, 50), ZIndex = 0 };
    var b = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(100, 50), ZIndex = 1 };
    var hit = "";
    a.OnMouseDown = _ => { hit = "a"; return true; };
    b.OnMouseDown = _ => { hit = "b"; return true; };
    window.Add(a);
    window.Add(b);

    var handled = window.OnMouseDownInternal(new Vector2(10, 10));

    Assert.True(handled);
    Assert.Equal("b", hit);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: FAIL.

**Step 3: Write minimal implementation**
Add clamp logic using `Parent.Size`. Add `Children` list with `Add/Remove` and input delegation by ZIndex.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: PASS.

**Step 5: Commit**
```bash
git add tests/LillyQuest.Tests/Engine/UI/UIWindowTests.cs src/LillyQuest.Engine/Screens/UI/UIWindow.cs
git commit -m "feat: add UIWindow drag clamp and child input"
```

### Task 3: Implement rendering (background, border, titlebar)

**Files:**
- Modify: `src/LillyQuest.Engine/Screens/UI/UIWindow.cs`

**Step 1: Write the failing test**
```csharp
[Fact]
public void Render_DoesNotThrow_WhenSpriteBatchNull()
{
    var window = new UIWindow { Size = new Vector2(50, 50) };
    window.Render(null, null);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: FAIL.

**Step 3: Write minimal implementation**
Guard against null spriteBatch/renderContext, then draw background rect (alpha), border, and optional titlebar + title text using same approach as UILabel (DrawFont).

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~UIWindowTests`
Expected: PASS.

**Step 5: Commit**
```bash
git add tests/LillyQuest.Tests/Engine/UI/UIWindowTests.cs src/LillyQuest.Engine/Screens/UI/UIWindow.cs
git commit -m "feat: add UIWindow rendering"
```

### Task 4: Add example usage in TilesetSurfaceEditorScene

**Files:**
- Modify: `src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs`

**Step 1: Write the failing test**
Skip (demo wiring only; no behavior test).

**Step 2: Implement minimal change**
Create a `UIWindow` instance, add a `UILabel` child, add to `UIScreenOverlay.Root`.

**Step 3: Run targeted tests**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter FullyQualifiedName~Engine.UI`
Expected: PASS.

**Step 4: Commit**
```bash
git add src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs
git commit -m "feat: add UIWindow demo"
```

### Task 5: Full verification

**Step 1: Run full test suite**
Run: `dotnet test --nologo`
Expected: PASS (0 failures).

**Step 2: Commit any remaining changes**
```bash
git status -s
```


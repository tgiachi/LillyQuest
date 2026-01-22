# UI Root Screen Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace UIScreenOverlay with UIRootScreen so all UI controls are managed via the ScreenManager stack with a single focus/dispatch path.

**Architecture:** Introduce `UIRootScreen : BaseScreen` that hosts `UIScreenRoot` and handles mouse capture. Remove UIScreenOverlay and its tests. Update scenes and ScreenManager routing to treat UI as a normal screen in the stack.

**Tech Stack:** C# (.NET 10), LillyQuest Engine UI layer, NUnit tests.

### Task 1: Add UIRootScreen tests (mouse capture)

**Files:**
- Create: `tests/LillyQuest.Tests/Engine/UI/UIRootScreenTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void MouseDown_Captures_Control_And_Forwards_Move_Up()
{
    var root = new UIRootScreen();
    var control = new UIScreenControl { Position = Vector2.Zero, Size = new Vector2(20, 20) };
    var moves = 0;
    var ups = 0;

    control.OnMouseDown = _ => true;
    control.OnMouseMove = _ => { moves++; return true; };
    control.OnMouseUp = _ => { ups++; return true; };
    root.Root.Add(control);

    Assert.That(root.OnMouseDown(5, 5, Array.Empty<MouseButton>()), Is.True);
    Assert.That(root.OnMouseMove(10, 10), Is.True);
    Assert.That(root.OnMouseUp(10, 10, Array.Empty<MouseButton>()), Is.True);
    Assert.That(moves, Is.EqualTo(1));
    Assert.That(ups, Is.EqualTo(1));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIRootScreenTests"`
Expected: FAIL (UIRootScreen missing).

**Step 3: Write minimal implementation**
Create `src/LillyQuest.Engine/Screens/UI/UIRootScreen.cs` with Root + ActiveControl and basic mouse routing.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIRootScreenTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UIRootScreen.cs tests/LillyQuest.Tests/Engine/UI/UIRootScreenTests.cs
git commit -m "feat: add UIRootScreen mouse capture"
```

### Task 2: Replace UIScreenOverlay usage in scenes

**Files:**
- Modify: `src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs`

**Step 1: Write the failing test**
Skip (demo wiring only).

**Step 2: Implement minimal change**
Replace `UIScreenOverlay` with `UIRootScreen` and add controls via `uiRoot.Root.Add(...)`.

**Step 3: Run targeted tests**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIRootScreenTests"`
Expected: PASS.

**Step 4: Commit**
```bash
git add src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs
git commit -m "feat: migrate scene to UIRootScreen"
```

### Task 3: Remove UIScreenOverlay and update tests

**Files:**
- Delete: `src/LillyQuest.Engine/Screens/UI/UIScreenOverlay.cs`
- Delete: `tests/LillyQuest.Tests/Engine/UI/UIScreenOverlayTests.cs`
- Modify: `src/LillyQuest.Engine/Managers/Screens/ScreenManager.cs`

**Step 1: Write the failing test**
Add test to `tests/LillyQuest.Tests/Engine/Screens/ScreenManagerTests.cs` ensuring normal top-screen dispatch still works when UI is a normal screen (no overlay special cases).

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~ScreenManagerTests"`
Expected: FAIL before ScreenManager changes.

**Step 3: Write minimal implementation**
Remove overlay-specific logic in ScreenManager (GetUiOverlay + branches) and keep top-screen dispatch + pass-through logic.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~ScreenManagerTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Managers/Screens/ScreenManager.cs src/LillyQuest.Engine/Screens/UI/UIRootScreen.cs
git add tests/LillyQuest.Tests/Engine/Screens/ScreenManagerTests.cs

git rm src/LillyQuest.Engine/Screens/UI/UIScreenOverlay.cs tests/LillyQuest.Tests/Engine/UI/UIScreenOverlayTests.cs

git commit -m "refactor: remove UIScreenOverlay"
```

### Task 4: Full verification

**Step 1: Run full test suite**
Run: `dotnet test --nologo`
Expected: PASS (0 failures).

**Step 2: Commit any remaining changes**
```bash
git status -s
```

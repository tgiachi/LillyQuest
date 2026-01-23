# UI Nine-Slice Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a nine-slice asset manager and a UINinePatchWindow control that renders tiled 9-slice images with title and content margins.

**Architecture:** Introduce a `NineSliceAssetManager` that registers a texture region plus pixel margins and computes 9 source rectangles. `UINinePatchWindow` uses a `NineSliceKey` to render corners/edges/center with tiling (cropped last tile) and supports title font settings and content margins for child layout. Add `NineSliceScale` (float) to scale the 9-slice tile sizes without changing the window size; title font is not scaled.

**Tech Stack:** C# (.NET 10), SpriteBatch rendering, NUnit tests.

### Task 1: Add NineSliceDefinition + manager tests

**Files:**
- Create: `src/LillyQuest.Engine/Screens/UI/NineSliceDefinition.cs`
- Create: `src/LillyQuest.Engine/Managers/Assets/NineSliceAssetManager.cs`
- Create: `tests/LillyQuest.Tests/Engine/UI/NineSliceAssetManagerTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void RegisterNineSlice_ComputesRects()
{
    var manager = new NineSliceAssetManager();
    manager.RegisterNineSlice(
        "window",
        "ui",
        new Rectangle(0, 0, 32, 32),
        new Vector4(8, 8, 8, 8)
    );

    var def = manager.GetNineSlice("window");

    Assert.That(def.Center.Width, Is.EqualTo(16));
    Assert.That(def.Center.Height, Is.EqualTo(16));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~NineSliceAssetManagerTests"`
Expected: FAIL (missing types).

**Step 3: Write minimal implementation**
Implement `NineSliceDefinition` with 9 rectangles computed from source rect + margins. Implement manager with dictionary keyed by string.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~NineSliceAssetManagerTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/NineSliceDefinition.cs src/LillyQuest.Engine/Managers/Assets/NineSliceAssetManager.cs tests/LillyQuest.Tests/Engine/UI/NineSliceAssetManagerTests.cs
git commit -m "feat: add nine-slice asset manager"
```

### Task 2: Add UINinePatchWindow tests

**Files:**
- Create: `src/LillyQuest.Engine/Screens/UI/UINinePatchWindow.cs`
- Create: `tests/LillyQuest.Tests/Engine/UI/UINinePatchWindowTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
    public void TitleAndContentMargins_AreApplied()
    {
        var window = new UINinePatchWindow
        {
            Position = Vector2.Zero,
            Size = new Vector2(100, 60),
            TitleMargin = new Vector4(6, 4, 0, 0),
            ContentMargin = new Vector4(8, 10, 0, 0),
            NineSliceScale = 2f
        };

    Assert.That(window.GetTitlePosition(), Is.EqualTo(new Vector2(6, 4)));
    Assert.That(window.GetContentOrigin(), Is.EqualTo(new Vector2(8, 10)));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UINinePatchWindowTests"`
Expected: FAIL (missing type).

**Step 3: Write minimal implementation**
Implement `UINinePatchWindow` with `TitleMargin`, `ContentMargin`, `TitleFontName`, `TitleFontSize`, `NineSliceKey`, and helper methods `GetTitlePosition()` and `GetContentOrigin()`.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UINinePatchWindowTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UINinePatchWindow.cs tests/LillyQuest.Tests/Engine/UI/UINinePatchWindowTests.cs
git commit -m "feat: add nine-patch window basics"
```

### Task 3: Implement nine-slice rendering with tiling

**Files:**
- Modify: `src/LillyQuest.Engine/Screens/UI/UINinePatchWindow.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void Render_DoesNotThrow_WhenMissingSpriteBatch()
{
    var window = new UINinePatchWindow();
    window.Render(null, null);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UINinePatchWindowTests"`
Expected: FAIL.

**Step 3: Write minimal implementation**
Implement `Render` to:
- Resolve nine-slice from manager by key.
- Draw corners + edges + center using tiled draw loops, cropping the last tile when needed.
- Render title with `TitleFontName`/`TitleFontSize` at `TitleMargin`.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UINinePatchWindowTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UINinePatchWindow.cs tests/LillyQuest.Tests/Engine/UI/UINinePatchWindowTests.cs
git commit -m "feat: render nine-slice window"
```

### Task 4: Add example usage in TilesetSurfaceEditorScene

**Files:**
- Modify: `src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs`

**Step 1: Implement minimal change**
Create a `NineSliceAssetManager` instance, register the `images/lillyquest_cover.jpg` 9-slice, create a `UINinePatchWindow` in the UI root, and add a child label to demonstrate `ContentMargin`.

**Step 2: Commit**
```bash
git add src/LillyQuest.Game/Scenes/TilesetSurfaceEditorScene.cs
git commit -m "feat: add nine-slice window demo"
```

### Task 5: Full verification

**Step 1: Run full test suite**
Run: `dotnet test --nologo`
Expected: PASS.

**Step 2: Commit any remaining changes**
```bash
git status -s
```

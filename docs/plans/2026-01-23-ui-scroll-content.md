# UIScrollContent Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement `UIScrollContent` with scrollable children, viewport clipping, and scrollbar rendering using texture patches unified in `INineSliceAssetManager`.

**Architecture:** Extend `INineSliceAssetManager` to manage named texture patches and fetch them by `(textureName, elementName)`. Implement `UIScrollContent` to compute viewport + scroll ranges, apply scissor + translation during child render, and draw scrollbars using patch sections. Keep input handling minimal with `HandleMouseWheel` in UI root dispatch.

**Tech Stack:** C# (.NET 10), SpriteBatch rendering, NUnit tests.

### Task 1: Add texture patch types + manager API (unification)

**Files:**
- Create: `src/LillyQuest.Core/Data/Assets/TexturePatchDefinition.cs`
- Create: `src/LillyQuest.Core/Data/Assets/TexturePatch.cs`
- Modify: `src/LillyQuest.Core/Interfaces/Assets/INineSliceAssetManager.cs`
- Modify: `src/LillyQuest.Core/Managers/Assets/NineSliceAssetManager.cs`
- Test: `tests/LillyQuest.Tests/Engine/UI/NineSliceAssetManagerPatchTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void RegisterTexturePatches_StoresAndRetrieves()
{
    var textureManager = new FakeTextureManager();
    var manager = new NineSliceAssetManager(textureManager);
    var patches = new[]
    {
        new TexturePatchDefinition("scroll.track", new Rectangle<int>(0, 0, 16, 64)),
        new TexturePatchDefinition("scroll.thumb", new Rectangle<int>(16, 0, 16, 32))
    };

    manager.RegisterTexturePatches("ui_atlas", patches);

    var track = manager.GetTexturePatch("ui_atlas", "scroll.track");
    var thumb = manager.GetTexturePatch("ui_atlas", "scroll.thumb");

    Assert.That(track.TextureName, Is.EqualTo("ui_atlas"));
    Assert.That(track.Section.Size.Y, Is.EqualTo(64));
    Assert.That(thumb.Section.Origin.X, Is.EqualTo(16));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~NineSliceAssetManagerPatchTests"`
Expected: FAIL (missing types/methods).

**Step 3: Write minimal implementation**
```csharp
public readonly struct TexturePatchDefinition
{
    public string ElementName { get; }
    public Rectangle<int> Section { get; }
}

public readonly struct TexturePatch
{
    public string TextureName { get; }
    public string ElementName { get; }
    public Rectangle<int> Section { get; }
}
```
Add to interface:
```csharp
void RegisterTexturePatches(string textureName, IReadOnlyList<TexturePatchDefinition> patches);
TexturePatch GetTexturePatch(string textureName, string elementName);
bool TryGetTexturePatch(string textureName, string elementName, out TexturePatch patch);
```
Implement in manager with dictionary key `"{texture}:{element}"`.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~NineSliceAssetManagerPatchTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Core/Data/Assets/TexturePatchDefinition.cs \
    src/LillyQuest.Core/Data/Assets/TexturePatch.cs \
    src/LillyQuest.Core/Interfaces/Assets/INineSliceAssetManager.cs \
    src/LillyQuest.Core/Managers/Assets/NineSliceAssetManager.cs \
    tests/LillyQuest.Tests/Engine/UI/NineSliceAssetManagerPatchTests.cs

git commit -m "feat: add texture patch support"
```

### Task 2: Add UIScrollContent geometry helpers

**Files:**
- Create: `src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs`
- Test: `tests/LillyQuest.Tests/Engine/UI/UIScrollContentTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void ViewportAndThumb_ComputeExpectedSizes()
{
    var control = new UIScrollContent(new FakeNineSliceManager(), new FakeTextureManager())
    {
        Size = new Vector2(200, 100),
        ContentSize = new Vector2(400, 300),
        EnableVerticalScroll = true,
        EnableHorizontalScroll = true,
        ScrollbarThickness = 10f,
        MinThumbSize = 16f
    };

    var viewport = control.GetViewportBounds();
    Assert.That(viewport.Size.X, Is.EqualTo(190f));
    Assert.That(viewport.Size.Y, Is.EqualTo(90f));

    var vThumb = control.GetVerticalThumbRect();
    Assert.That(vThumb.Size.Y, Is.GreaterThanOrEqualTo(16f));
    var hThumb = control.GetHorizontalThumbRect();
    Assert.That(hThumb.Size.X, Is.GreaterThanOrEqualTo(16f));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: FAIL (missing type/methods).

**Step 3: Write minimal implementation**
Implement `UIScrollContent` with:
```csharp
public Rectangle<float> GetViewportBounds();
public Rectangle<float> GetVerticalThumbRect();
public Rectangle<float> GetHorizontalThumbRect();
private Vector2 GetMaxScrollOffset();
private Vector2 ClampScroll(Vector2 offset);
```
No rendering yet; just geometry and clamps.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs \
    tests/LillyQuest.Tests/Engine/UI/UIScrollContentTests.cs

git commit -m "feat: add UIScrollContent geometry"
```

### Task 3: Render children with scissor + scrollbars

**Files:**
- Modify: `src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void Render_DoesNotThrow_WhenMissingPatches()
{
    var control = new UIScrollContent(new FakeNineSliceManager(), new FakeTextureManager());
    control.Render(null, null);
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: FAIL.

**Step 3: Write minimal implementation**
Implement `Render` to:
- Set scissor to viewport bounds.
- `spriteBatch.PushTranslation(-ScrollOffset);` render children in Z order; `PopTranslation()`.
- Disable scissor.
- Draw scrollbars using patches if available and texture exists.

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs \
    tests/LillyQuest.Tests/Engine/UI/UIScrollContentTests.cs

git commit -m "feat: render UIScrollContent"
```

### Task 4: Mouse wheel scrolling (minimal)

**Files:**
- Modify: `src/LillyQuest.Engine/Screens/UI/UIScreenControl.cs`
- Modify: `src/LillyQuest.Engine/Screens/UI/UIRootScreen.cs`
- Modify: `src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs`
- Test: `tests/LillyQuest.Tests/Engine/UI/UIScrollContentTests.cs`

**Step 1: Write the failing test**
```csharp
[Test]
public void HandleMouseWheel_ScrollsVerticalByDefault()
{
    var control = new UIScrollContent(new FakeNineSliceManager(), new FakeTextureManager())
    {
        Size = new Vector2(100, 100),
        ContentSize = new Vector2(100, 200),
        ScrollSpeed = 10f
    };

    control.HandleMouseWheel(new Vector2(10, 10), 1f);

    Assert.That(control.ScrollOffset.Y, Is.GreaterThan(0f));
}
```

**Step 2: Run test to verify it fails**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: FAIL.

**Step 3: Write minimal implementation**
- Add `public virtual bool HandleMouseWheel(Vector2 point, float delta)` to `UIScreenControl` (default false).
- In `UIRootScreen.OnMouseWheel`, hit test and route to `HandleMouseWheel`.
- In `UIScrollContent.HandleMouseWheel`, adjust `ScrollOffset` (Y if vertical enabled, else X if horizontal only).

**Step 4: Run test to verify it passes**
Run: `dotnet test tests/LillyQuest.Tests/LillyQuest.Tests.csproj --filter "Name~UIScrollContentTests"`
Expected: PASS.

**Step 5: Commit**
```bash
git add src/LillyQuest.Engine/Screens/UI/UIScreenControl.cs \
    src/LillyQuest.Engine/Screens/UI/UIRootScreen.cs \
    src/LillyQuest.Engine/Screens/UI/UIScrollContent.cs \
    tests/LillyQuest.Tests/Engine/UI/UIScrollContentTests.cs

git commit -m "feat: add UIScrollContent mouse wheel"
```

### Task 5: Full verification

**Step 1: Run full test suite**
Run: `dotnet test --nologo`
Expected: PASS (existing warnings ok).

**Step 2: Commit any remaining changes**
```bash
git status -s
```


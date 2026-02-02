# AGENTS.md

LillyQuest is a 2D roguelike game engine built with .NET 10, Silk.NET (OpenGL), and GoRogue 3.0. Layered architecture with DI (DryIoc).

## Mandatory Workflow

1. **Use superpowers skills** - Before any task, invoke the appropriate skill:
   - `brainstorming` - Before creating features
   - `test-driven-development` - Before implementing
   - `systematic-debugging` - Before fixing bugs
   - `verification-before-completion` - Before claiming "done"

2. **TDD always** - Write tests BEFORE implementation:
   ```
   Red test → Implementation → Green test → Refactor
   ```

## Project Structure

```
src/
├── LillyQuest.Core/           # Primitives, OpenGL, asset managers
├── LillyQuest.Engine/         # Scenes, screens, systems, entities, DI
├── LillyQuest.RogueLike/      # Plugin: data loading, map, creatures
├── LillyQuest.Scripting.Lua/  # Lua scripting
└── LillyQuest.Game/           # Executable, scenes, entry point

tests/
└── LillyQuest.Tests/          # All tests, mirrors src/ structure
```

**Where to put things:**

| Type | Location |
|------|----------|
| New service interface | `src/[Layer]/Interfaces/Services/I*.cs` |
| Service implementation | `src/[Layer]/Services/*.cs` |
| JSON entity | `src/LillyQuest.RogueLike/Json/Entities/[Domain]/*Json.cs` |
| Data loader/receiver | `src/LillyQuest.RogueLike/Services/Loaders/*Service.cs` |
| New scene | `src/LillyQuest.Game/Scenes/*Scene.cs` |
| Tests | `tests/LillyQuest.Tests/[Layer]/[Type]/*Tests.cs` |

## Data Loading Pattern

**JSON Entity** - Derive from `BaseJsonEntity`, add discriminator:

```csharp
// In BaseJsonEntity.cs add:
[JsonDerivedType(typeof(MyEntityJson), typeDiscriminator: "my_entity")]

// Create file: Json/Entities/[Domain]/MyEntityJson.cs
public sealed class MyEntityJson : BaseJsonEntity
{
    public string Name { get; set; } = string.Empty;
}
```

**Data Receiver** - Implement `IDataLoaderReceiver`:

```csharp
public sealed class MyService : IDataLoaderReceiver
{
    private readonly Dictionary<string, MyEntityJson> _entities = [];

    public Type[] GetLoadTypes() => [typeof(MyEntityJson)];

    public Task LoadDataAsync(List<BaseJsonEntity> entities)
    {
        foreach (var entity in entities.OfType<MyEntityJson>())
            _entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public void ClearData() => _entities.Clear();
    public bool VerifyLoadedData() => _entities.Count > 0;
}
```

Register in DI within the plugin.

## Scenes & Screens

**Scene** - Derive from `BaseScene`:

```csharp
public sealed class MyScene : BaseScene
{
    public MyScene(IService dependency) : base("my_scene")
    {
        // inject dependencies
    }

    public override void OnLoad()
    {
        AddEntity(new MyEntity());
    }
}
```

**Register**: `container.RegisterScene<MyScene>();`

**Screen** - Derive from `BaseScreen` for UI containers.

## Testing Conventions

**Naming**: `[Method]_[Scenario]_[Expected]`

```csharp
[Fact]
public void LoadDataAsync_WithValidEntities_PopulatesDictionary()
```

**File structure**: `tests/LillyQuest.Tests/[Layer]/[Type]/[Class]Tests.cs`

**AAA Pattern**:

```csharp
[Fact]
public void Method_Scenario_Expected()
{
    // Arrange
    var sut = new MyService();

    // Act
    var result = sut.DoSomething();

    // Assert
    Assert.Equal(expected, result);
}
```

**Mocking**: Use inline `Fake*` classes or `NSubstitute` for interfaces.

## Code Style

- **One type per file**: Each `.cs` file must contain exactly 1 class, 1 record, 1 enum, or 1 interface
- **Namespace by domain**: Organize types in domain-specific folders that match namespaces:

| Type | Folder | Namespace example |
|------|--------|-------------------|
| Enums | `*/Types/` | `LillyQuest.Core.Types` |
| Interfaces | `*/Interfaces/[Domain]/` | `LillyQuest.Engine.Interfaces.Services` |
| Services | `*/Services/[Domain]/` | `LillyQuest.RogueLike.Services.Loaders` |
| JSON entities | `*/Json/Entities/[Domain]/` | `LillyQuest.RogueLike.Json.Entities.Creatures` |
| Data/configs | `*/Data/[Domain]/` | `LillyQuest.Engine.Data.Input` |

- **Naming**: PascalCase for public, _camelCase for private fields
- **Interfaces**: Prefix with `I` (e.g., `IMyService`)
- **File-scoped namespaces**: `namespace X;`
- **Primary constructors**: Preferred for DI
- **Nullable**: Enabled, use `?` and `!` explicitly
- **Sealed**: Use `sealed` on classes not intended for inheritance
- **Expression body**: For one-liner methods (`=> expr;`)
- **Types folders**: `*/Types/` is reserved for enums only

## Common Mistakes

| Don't | Do instead |
|-------|------------|
| Create files without tests | TDD: test first |
| Service without interface | Always `IMyService` + `MyService` |
| Forget `sealed` | Add `sealed` if inheritance not needed |
| JSON entity without discriminator | Add `[JsonDerivedType]` in `BaseJsonEntity` |
| Test without AAA pattern | Separate Arrange/Act/Assert |
| Skip verification skill | Always verify before claiming "done" |
| Multiple types in one file | One type per `.cs` file |
| Wrong namespace/folder | Match folder structure to namespace domain |

# GameComponentCollection Design

**Date:** 2026-01-18
**Status:** Approved
**Purpose:** High-performance component storage with type caching and fast lookups

## Overview

A specialized collection for storing and querying `IGameComponent` instances with two key optimizations:
1. **O(1) per-entity component lookup** via `GetComponent<T>()`
2. **O(k) global type iteration** via `GetAllComponentsOfType<T>()` on the manager

Used to implement:
- Entity-level storage: `IGameEntity.Components`
- Manager-level storage: `IGameEntityManager` type queries

## Design Constraints

- **Access Pattern:** Primarily read-heavy during gameplay (every frame)
- **Mutations:** Only at scene changes, not every frame
- **Component Types:** 50-100 different types expected
- **Uniqueness:** Max 1 component per type per entity
- **Performance Priority:** `GetComponent<T>()` is most critical

## Architecture

### Per-Entity Storage: `GameComponentCollection`

```csharp
public class GameComponentCollection : IEnumerable<IGameComponent>
{
    private List<IGameComponent> _components;              // Primary store
    private Dictionary<Type, IGameComponent> _typeIndex;   // Type cache

    // Initialization
    public GameComponentCollection() { }

    // Mutations
    public void Add(IGameComponent component);
    public bool Remove(IGameComponent component);
    public void Clear();

    // O(1) Queries
    public TComponent? GetComponent<TComponent>()
        where TComponent : IGameComponent;
    public bool HasComponent<TComponent>()
        where TComponent : IGameComponent;

    // Enumeration
    public IEnumerator<IGameComponent> GetEnumerator();
    public int Count { get; }
}
```

### Manager-Level Index: `IGameEntityManager`

The manager maintains:
```csharp
private Dictionary<Type, List<IGameComponent>> _globalTypeIndex;
```

Responsibilities:
- Track all components globally by type
- Support fast iteration via `GetAllComponentsOfType<T>()`
- Rebuild index on scene changes

## Behavior Specification

### `Add(IGameComponent component)`
- **Time Complexity:** O(1)
- **Precondition:** Component's type not already present
- **Behavior:** Add to `_components` list and `_typeIndex` dictionary
- **On Violation:** Throw `InvalidOperationException` with clear message
- **Manager Update:** Manager adds to its global type index

### `Remove(IGameComponent component)`
- **Time Complexity:** O(n)* where n = component count (List.Remove performance)
- **Return:** `true` if removed, `false` if not found (idempotent)
- **Behavior:** Remove from both `_components` and `_typeIndex`
- **Manager Update:** Manager removes from global type index

### `GetComponent<T>()`
- **Time Complexity:** O(1)
- **Return:** Component of type T, or `null` if not found
- **No Exceptions:** Type-safe lookup, never throws

### `GetAllComponentsOfType<T>()` (Manager)
- **Time Complexity:** O(1) to retrieve, O(k) to iterate (k = count of type)
- **Return:** Non-null `List<T>` (empty list if no components of that type)
- **No Allocations:** During gameplay (list reused from index)

### `Clear()`
- **Time Complexity:** O(n)
- **Behavior:** Empty both `_components` and `_typeIndex`
- **Manager Update:** Clear all type entries from global index or rebuild

## Error Handling

| Scenario | Behavior | Rationale |
|----------|----------|-----------|
| Add component when type exists | Throw `InvalidOperationException` | One component per type per entity |
| Remove non-existent component | Return `false` | Idempotent operation |
| GetComponent<T>() not found | Return `null` | Nullable return, no exceptions |
| GetAllComponentsOfType<T>() empty | Return empty list (never null) | Safe iteration for systems |
| Clear() on empty collection | No-op, succeeds | Idempotent |

## Memory Model

**Per-Entity Storage:**
- `List<IGameComponent>`: ~50-100 components average (can grow)
- `Dictionary<Type, IGameComponent>`: ~50-100 entries (type count)
- **Total:** ~200-400 bytes per entity (minimal overhead)

**Manager-Level Index:**
- `Dictionary<Type, List<IGameComponent>>`: One list per type
- **Total:** 50-100 type entries, each list proportional to component count across all entities

**Allocation Strategy:**
- Collections are allocated on entity/manager creation
- No allocations during gameplay (scene change only)
- Dictionary load factor: .NET default (acceptable for 50-100 entries)

## Integration Points

### With `IGameEntity`
```csharp
public interface IGameEntity
{
    uint Id { get; }
    string Name { get; }
    bool IsActive { get; set; }
    uint Order { get; set; }  // Mutable depth/z-order for rendering
    IEnumerable<IGameComponent> Components { get; }
    // Implementation returns GameComponentCollection
}
```

**Note on Order:**
- Mutable `uint` property for render depth control
- Render system sorts entities by Order every frame (no events needed)
- Can be changed dynamically during gameplay

### With `IGameEntityManager`
```csharp
public interface IGameEntityManager
{
    IEnumerable<TGameComponent> GetAllComponentsOfType<TGameComponent>()
        where TGameComponent : IGameComponent;
    // Implementation uses global type index
}
```

## Testing Strategy

### Unit Tests: `GameComponentCollectionTests`
1. **Add/Remove Operations**
   - Add single component, verify presence
   - Add duplicate type, verify exception
   - Remove component, verify absence
   - Remove non-existent, verify false return
   - Clear collection, verify empty

2. **O(1) Lookups**
   - GetComponent<T> found, verify correct instance
   - GetComponent<T> not found, verify null
   - HasComponent<T> true/false cases
   - Multiple components, verify isolation

3. **Enumeration**
   - Enumerate empty collection
   - Enumerate single component
   - Enumerate multiple components in order
   - Modify during enumeration (expected behavior)

4. **Integration**
   - Add to entity, query via manager
   - Multiple entities, verify index isolation
   - Remove from one entity, verify not in manager index
   - Clear entity, verify manager index updated

### Performance Tests
- Verify GetComponent<T> is O(1)
- Verify GetAllComponentsOfType<T> iteration is O(k)
- Memory overhead measurement

## Implementation Phases

**Phase 1:** Core `GameComponentCollection` with unit tests
**Phase 2:** Integrate with `GameEntity` implementation
**Phase 3:** Integrate with `GameEntityManager` global index
**Phase 4:** Performance testing and optimization

## Open Questions

None - design is complete and validated.

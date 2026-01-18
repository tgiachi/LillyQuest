# GameEntityManager Enhancement Design

**Date:** 2026-01-18
**Status:** Approved
**Purpose:** Add ordered component indexing and lifecycle events to GameEntityManager

## Overview

Enhancement to `GameEntityManager` to:
1. **Maintain ordered component lists** - Components always sorted by their owner's Order
2. **Lifecycle events** - Notify listeners when entities are added/removed
3. **Support render system needs** - GetAllComponentsOfType returns pre-sorted components

## Design Constraints

- **Ordering:** Components always ordered by `IGameEntity.Order` within each type
- **Sorting strategy:** Append + sort (O(n log n) at add/remove, only at scene changes)
- **Threading:** Single-threaded, no concurrent access
- **Events:** Post-operation notification (after state is stable)
- **Scope:** Only OnGameEntityAdded/Removed events (no component-level events for simplicity)

## Architecture: Ordered Global Type Index

### Data Structure

```csharp
private List<IGameEntity> _entities;
private Dictionary<Type, List<IGameComponent>> _globalTypeIndex;

public event GameEntityAddedHandler? OnGameEntityAdded;
public event GameEntityRemovedHandler? OnGameEntityRemoved;

public delegate void GameEntityAddedHandler(IGameEntity entity);
public delegate void GameEntityRemovedHandler(IGameEntity entity);
```

Each `List<IGameComponent>` in `_globalTypeIndex` is **always sorted by its owner's Order**.

### Key Insight

Render system sorts every frame anyway. By maintaining order in the manager, `GetAllComponentsOfType<T>()` naturally returns pre-sorted results with zero additional cost during gameplay.

## Behavior Specification

### AddEntity(IGameEntity entity)

**Flow:**
1. Add entity to `_entities`
2. Call `IndexEntity(entity)`:
   - For each component in entity.Components:
     - Get or create list for component type
     - Add component to that list
   - For each type list: **Sort by `component.Owner.Order`**
3. Invoke `OnGameEntityAdded?.Invoke(entity)`

**Time Complexity:** O(n log n) where n = total components of all types
**When:** Scene change (rare)

### RemoveEntity(IGameEntity entity)

**Flow:**
1. Remove entity from `_entities`
2. Call `DeindexEntity(entity)`:
   - For each component in entity.Components:
     - Remove from its type list
     - If list becomes empty, remove type from index
3. Invoke `OnGameEntityRemoved?.Invoke(entity)`

**Time Complexity:** O(n) where n = entity's components
**Note:** Remaining components stay in order (no re-sort needed)

### GetAllComponentsOfType<T>()

```csharp
public IEnumerable<TGameComponent> GetAllComponentsOfType<TGameComponent>()
    where TGameComponent : IGameComponent
{
    if (_globalTypeIndex.TryGetValue(typeof(TGameComponent), out var components))
    {
        return components.Cast<TGameComponent>(); // Already sorted!
    }
    return [];
}
```

**Time Complexity:** O(1) to access + O(k) to iterate (k = components of type T)
**Guarantee:** Always returns components sorted by their owner's Order

## Event Semantics

### OnGameEntityAdded

- **When:** After entity is added, indexed, and sorted
- **Parameter:** The IGameEntity that was added
- **Usage:** Listeners can query GetAllComponentsOfType and see the new entity's components in correct order

### OnGameEntityRemoved

- **When:** After entity is removed and deindexed
- **Parameter:** The IGameEntity that was removed
- **Usage:** Listeners can clean up any cached references

**Important:** Listeners should not modify the manager during event (undefined behavior)

## Invariants

After any Add/Remove operation:
- ✅ All type lists are sorted by `component.Owner.Order`
- ✅ No empty type lists in the dictionary
- ✅ All components in `_globalTypeIndex` belong to entities in `_entities`
- ✅ All entities in `_entities` have their components in `_globalTypeIndex`

## Implementation Details

### Sorting Strategy

**Why append + sort?**
- Add/Remove only happen at scene changes (infrequent)
- Render queries components every frame (frequent)
- O(n log n) at scene change << O(1) lookup every frame
- Simpler to understand and maintain than binary search insertion

**Sort predicate:**
```csharp
componentList.Sort((a, b) => a.Owner!.Order.CompareTo(b.Owner!.Order));
```

### Empty List Cleanup

When removing components from a type list, if it becomes empty, remove the type entry from `_globalTypeIndex`. This saves memory and keeps the dictionary clean.

### Event Ordering

For multiple entities added in sequence:
1. AddEntity(entity1) → OnGameEntityAdded(entity1)
2. AddEntity(entity2) → OnGameEntityAdded(entity2)

Each event fires after that specific entity is fully indexed and sorted.

## Testing Strategy

### Unit Tests: GameEntityManager Enhancement

1. **Ordering on Add**
   - Add entities with different Orders
   - Verify GetAllComponentsOfType returns components sorted by Order
   - Verify sorting is stable

2. **Ordering on Remove**
   - Add multiple entities, remove one
   - Verify remaining components stay sorted
   - Verify no re-sort anomalies

3. **Event Lifecycle**
   - Verify OnGameEntityAdded fires after indexing
   - Verify OnGameEntityRemoved fires after deindexing
   - Verify event receives correct entity reference
   - Verify listener can query GetAllComponentsOfType during event

4. **Complex Scenarios**
   - Add/remove in different orders
   - Change Order of entities (verify next scene uses new order)
   - Multiple component types
   - Empty manager transitions

5. **Invariant Validation**
   - No empty type lists exist
   - All components in index belong to tracked entities
   - All entities' components are in index

## Backward Compatibility

- Existing `GetAllComponentsOfType<T>()` interface unchanged
- New events are optional (backward compatible if no listeners)
- No breaking changes to public API

## Performance Characteristics

| Operation | Time | Notes |
|-----------|------|-------|
| AddEntity | O(n log n) | Scene change only |
| RemoveEntity | O(n) | Scene change only |
| GetAllComponentsOfType | O(k) | Every frame, pre-sorted |
| Iteration over type list | O(k) | Already ordered |

Where n = total components, k = components of specific type

## Open Questions

None - design is complete and validated.

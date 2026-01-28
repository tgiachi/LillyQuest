using LillyQuest.Engine.Interfaces.Entities;

namespace LillyQuest.Engine.Collections;

/// <summary>
/// Collection that manages game entities in a hierarchical structure with ordering.
/// Thread-safe for concurrent reads, but mutations should be synchronized externally
/// or performed on the main thread only.
/// </summary>
public sealed class GameEntityCollection
{
    private readonly List<IGameEntity> _roots = new();
    private readonly List<IGameEntity> _ordered = new();
    private readonly Dictionary<IGameEntity, long> _insertionIndices = new();
    private readonly Dictionary<Type, object> _queryCache = new();
    private readonly object _lock = new();
    private long _nextInsertionIndex;

    public IReadOnlyList<IGameEntity> OrderedEntities
    {
        get
        {
            lock (_lock)
            {
                return _ordered.ToList();
            }
        }
    }

    public void Add(IGameEntity entity, IGameEntity? parent = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_lock)
        {
            EnsureChildrenList(entity);
            AssignInsertionIndices(entity);

            if (parent is null)
            {
                if (entity.Parent is not null)
                {
                    entity.Parent.Children.Remove(entity);
                    entity.Parent = null;
                }

                AddRoot(entity);
            }
            else
            {
                EnsureChildrenList(parent);

                if (entity.Parent is not null && !ReferenceEquals(entity.Parent, parent))
                {
                    entity.Parent.Children.Remove(entity);
                }

                if (!parent.Children.Contains(entity))
                {
                    parent.Children.Add(entity);
                }

                entity.Parent = parent;
                _roots.Remove(entity);
            }

            RebuildOrderedEntities();
            _queryCache.Clear();
        }
    }

    public IReadOnlyList<TInterface> GetQueryOf<TInterface>() where TInterface : class
    {
        var type = typeof(TInterface);

        lock (_lock)
        {
            if (_queryCache.TryGetValue(type, out var cached))
            {
                return ((List<TInterface>)cached).ToList();
            }

            var results = new List<TInterface>();

            foreach (var entity in _ordered)
            {
                if (entity is TInterface match)
                {
                    results.Add(match);
                }
            }

            _queryCache[type] = results;

            return results.ToList();
        }
    }

    public void Remove(IGameEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_lock)
        {
            if (entity.Parent is not null)
            {
                entity.Parent.Children.Remove(entity);
                entity.Parent = null;
            }

            _roots.Remove(entity);
            RebuildOrderedEntities();
            _queryCache.Clear();
        }
    }

    private void AddEntityDepthFirst(IGameEntity entity)
    {
        _ordered.Add(entity);

        EnsureChildrenList(entity);

        if (entity.Children.Count == 0)
        {
            return;
        }

        var orderedChildren = new List<IGameEntity>(entity.Children);
        orderedChildren.Sort(CompareEntities);

        foreach (var child in orderedChildren)
        {
            AddEntityDepthFirst(child);
        }
    }

    private void AddRoot(IGameEntity entity)
    {
        if (!_roots.Contains(entity))
        {
            _roots.Add(entity);
        }
    }

    private void AssignInsertionIndices(IGameEntity entity)
    {
        if (!_insertionIndices.ContainsKey(entity))
        {
            _insertionIndices[entity] = _nextInsertionIndex++;
        }

        EnsureChildrenList(entity);

        foreach (var child in entity.Children)
        {
            AssignInsertionIndices(child);
        }
    }

    private int CompareEntities(IGameEntity left, IGameEntity right)
    {
        var orderCompare = left.Order.CompareTo(right.Order);

        if (orderCompare != 0)
        {
            return orderCompare;
        }

        var leftIndex = GetOrAssignInsertionIndex(left);
        var rightIndex = GetOrAssignInsertionIndex(right);

        return leftIndex.CompareTo(rightIndex);
    }

    private static void EnsureChildrenList(IGameEntity entity)
    {
        if (entity.Children == null)
        {
            entity.Children = new List<IGameEntity>();
        }
    }

    private long GetOrAssignInsertionIndex(IGameEntity entity)
    {
        if (_insertionIndices.TryGetValue(entity, out var index))
        {
            return index;
        }

        index = _nextInsertionIndex++;
        _insertionIndices[entity] = index;

        return index;
    }

    private void RebuildOrderedEntities()
    {
        _ordered.Clear();

        if (_roots.Count == 0)
        {
            return;
        }

        var orderedRoots = new List<IGameEntity>(_roots);
        orderedRoots.Sort(CompareEntities);

        foreach (var root in orderedRoots)
        {
            AddEntityDepthFirst(root);
        }
    }
}

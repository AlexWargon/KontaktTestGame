using System;
using System.Collections.Generic;

namespace Wargon.TinyEcs {
    public class Query : IEquatable<Query> {
    private int count;
    private int[] entities;
    private int[] entityMap;
    private int entityToUpdateCount;
    private EntityToUpdate[] entityToUpdates;
    private int Index;
    internal string withName;
    internal bool IsDirty;
    internal readonly HashSet<int> with;
    internal readonly HashSet<int> without;
    internal readonly HashSet<int> any;
    public Query(World world) {
        WorldInternal = world;
        entities = new int[32];
        entityMap = new int[32];
        with = new HashSet<int>();
        without = new HashSet<int>();
        any = new HashSet<int>();
        entityToUpdates = new EntityToUpdate[32];
        Index = world.QueriesCount;
        count = 0;
    }

    internal World WorldInternal;

    public int FullSize {
        get=> entities.Length;
    }

    public bool IsEmpty {
        get => count == 0;
    }

    public bool Equals(Query other) {
        return other.Index == Index;
    }

    internal void PreAddWith(int entity) {
        if (entityToUpdates.Length <= entityToUpdateCount)
            Array.Resize(ref entityToUpdates, entityToUpdateCount + 16);
        ref var e = ref entityToUpdates[entityToUpdateCount];
        e.entity = entity;
        e.add = true;
        entityToUpdateCount++;
        WorldInternal.AddDirtyQuery(this);
        IsDirty = true;
    }

    internal void PreRemoveWith(int entity) {
        if (entityToUpdates.Length <= entityToUpdateCount)
            Array.Resize(ref entityToUpdates, entityToUpdateCount + 16);
        ref var e = ref entityToUpdates[entityToUpdateCount];
        e.entity = entity;
        e.add = false;
        entityToUpdateCount++;
        WorldInternal.AddDirtyQuery(this);
        IsDirty = true;
    }

    public Entity GetEntity(int index) {
        return WorldInternal.GetEntity(entities[index]);
    }

    private void Remove(int entity) {
        if (!Has(entity)) return;
        var index = entityMap[entity] - 1;
        entityMap[entity] = 0;
        count--;
        if (count > index) {
            entities[index] = entities[count];
            entityMap[entities[index]] = index + 1;
        }
    }

    private void Add(int entity) {
        if (entities.Length - 1 <= count) {
            Array.Resize(ref entities, count + 16);
        }

        if (entityMap.Length - 1 <= entity) {
            Array.Resize(ref entityMap, entity + 16);
        }
        if (Has(entity)) return;
        entities[count++] = entity;
        entityMap[entity] = count;
    }

    internal void Update() {
        for (var i = 0; i < entityToUpdateCount; i++) {
            ref var e = ref entityToUpdates[i];
            if (e.add)
                Add(e.entity);
            else
                Remove(e.entity);
        }

        entityToUpdateCount = 0;
        IsDirty = false;
    }


    private bool Has(int entity) {
        if (entityMap.Length <= entity)
            return false;
        return entityMap[entity] > 0;
    }
    public Enumerator GetEnumerator() {
        Enumerator e;
        e.query = this;
        e.index = -1;
        return e;
    }

    internal struct EntityToUpdate {
        public int entity;
        public bool add;
    }
    public ref struct Enumerator {
        public Query query;
        public int index;

        public Enumerator(Query query) {
            this.query = query;
            index = -1;
        }

        public bool MoveNext() {
            index++;
            return index < query.count;
        }

        public void Reset() {
            index = -1;
        }

        public Entity Current {
            get => query.GetEntity(index);
        }
    }
}

public static class QueryExtensions {
    public static Query WithAll(this Query query, params Type[] types) {
        foreach (var type in types) {
            query.with.Add(ComponentType.GetIndex(type));
        }
        return query;
    }
    public static Query WithAny(this Query query, params Type[] types) {
        foreach (var type in types) {
            query.any.Add(ComponentType.GetIndex(type));
        }
        return query;
    }
    public static Query With<T>(this Query query) where T : class {
        query.with.Add(ComponentType<T>.Index);
        return query;
    }

    public static Query Without<T>(this Query query) where T : class {
        query.without.Add(ComponentType<T>.Index);
        return query;
    }

    public static Query WithAny<T1, T2>(this Query query) where T1 : class where T2 : class {
        query.any.Add(ComponentType<T1>.Index);
        query.any.Add(ComponentType<T2>.Index);
        return query;
    }
}
}

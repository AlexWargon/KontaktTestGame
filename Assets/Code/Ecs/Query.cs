using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wargon.TinyEcs {
    public class Query : IEquatable<Query> {
        private readonly int _index;
        private readonly World _world;
        internal readonly HashSet<int> Any;
        internal readonly HashSet<int> With;
        internal readonly HashSet<int> Without;
        private int[] _entities;
        private int[] _entityMap;
        private int _entityToUpdateCount;
        private EntityToUpdate[] _entityToUpdates;
        internal bool IsDirty;

        public Query(World world) {
            _world = world;
            _entities = new int[32];
            _entityMap = new int[32];
            With = new HashSet<int>();
            Without = new HashSet<int>();
            Any = new HashSet<int>();
            _entityToUpdates = new EntityToUpdate[32];
            _index = world.QueriesCount;
            Count = 0;
        }

        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        public int FullSize => _entities.Length;

        public bool IsEmpty => Count == 0;

        public bool Equals(Query other) {
            return other._index == _index;
        }

        internal void PreAddWith(int entity) {
            if (_entityToUpdates.Length <= _entityToUpdateCount)
                Array.Resize(ref _entityToUpdates, _entityToUpdateCount + 16);
            ref var e = ref _entityToUpdates[_entityToUpdateCount];
            e.entity = entity;
            e.add = true;
            _entityToUpdateCount++;
            _world.AddDirtyQuery(this);
            IsDirty = true;
        }

        internal void PreRemoveWith(int entity) {
            if (_entityToUpdates.Length <= _entityToUpdateCount)
                Array.Resize(ref _entityToUpdates, _entityToUpdateCount + 16);
            ref var e = ref _entityToUpdates[_entityToUpdateCount];
            e.entity = entity;
            e.add = false;
            _entityToUpdateCount++;
            _world.AddDirtyQuery(this);
            IsDirty = true;
        }

        public Entity GetEntity(int index) {
            return _world.GetEntity(_entities[index]);
        }

        public int GetPureEntity(int index) {
            return _entities[index];
        }

        private void Remove(int entity) {
            if (!Has(entity)) return;
            var index = _entityMap[entity] - 1;
            _entityMap[entity] = 0;
            Count--;
            if (Count > index) {
                _entities[index] = _entities[Count];
                _entityMap[_entities[index]] = index + 1;
            }
        }

        private void Add(int entity) {
            if (_entities.Length - 1 <= Count) Array.Resize(ref _entities, Count + 16);

            if (_entityMap.Length - 1 <= entity) Array.Resize(ref _entityMap, entity + 16);

            if (Has(entity)) return;
            _entities[Count++] = entity;
            _entityMap[entity] = Count;
        }

        internal void Update() {
            for (var i = 0; i < _entityToUpdateCount; i++) {
                ref var e = ref _entityToUpdates[i];
                if (e.add)
                    Add(e.entity);
                else
                    Remove(e.entity);
            }

            _entityToUpdateCount = 0;
            IsDirty = false;
        }


        private bool Has(int entity) {
            if (_entityMap.Length <= entity)
                return false;
            return _entityMap[entity] > 0;
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
                return index < query.Count;
            }

            public void Reset() {
                index = -1;
            }

            public Entity Current => query.GetEntity(index);
        }
    }

    public static class QueryExtensions {
        public static Query WithAll(this Query query, params Type[] types) {
            foreach (var type in types) query.With.Add(ComponentType.GetIndex(type));

            return query;
        }

        public static Query WithAny(this Query query, params Type[] types) {
            foreach (var type in types) query.Any.Add(ComponentType.GetIndex(type));

            return query;
        }

        public static Query With<T>(this Query query) where T : class {
            query.With.Add(ComponentType<T>.Index);
            return query;
        }

        public static Query Without<T>(this Query query) where T : class {
            query.Without.Add(ComponentType<T>.Index);
            return query;
        }

        public static Query WithAny<T1, T2>(this Query query) where T1 : class where T2 : class {
            query.Any.Add(ComponentType<T1>.Index);
            query.Any.Add(ComponentType<T2>.Index);
            return query;
        }
    }
}
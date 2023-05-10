using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Wargon.TinyEcs {
    public struct WorldData {
        public float DeltaTime;
        public float FixedDeltaTime;
    }
    public sealed class World {
        private static World _defaultWorld;
        private int[] _entityArchetypesIDs;
        private DynamicArray<Archetype> _archetypesList;
        private Dictionary<int, Archetype> _archetypesMap;
        private IDependenciesContainer _dependenciesContainer;
        private DynamicArray<Query> _dirtyQueries;
        private Entity[] _entities;
        private DynamicArray<int> _freeEntities;
        private DynamicArray<Query> _queries;
        private Systems _systems;
        private int _lastNewEntity;
        private bool _alive;
        private WorldData _data;
        public WorldData Data => _data;
        internal int QueriesCount => _queries.Count;
        private World() {
            _entityArchetypesIDs = new int[128];
            _archetypesList = new DynamicArray<Archetype>(10);
            _archetypesMap = new Dictionary<int, Archetype>();
            _archetypesMap.Add(Archetype.Empty, Archetype.EmptyRef(this));
            _dirtyQueries = new DynamicArray<Query>(4);
            _entities = new Entity[128];
            _freeEntities = new DynamicArray<int>(128);
            _systems = new Systems(this);
            _queries = new DynamicArray<Query>(8);
            _lastNewEntity = 0;
            _alive = true;
        }

        public void Destroy() {
            _alive = false;
            _archetypesMap.Clear();
            _systems = null;
            _defaultWorld = null;
        }
        public static World Default => _defaultWorld ??= new World();

        public World SetDI(IDependenciesContainer dependenciesContainer) {
            _dependenciesContainer = dependenciesContainer;
            return this;
        }

        public IDependenciesContainer GetDI() => _dependenciesContainer;
        public void Init() {
            _systems.Init();
        }

        public DynamicArray<Query> GetAllQueries() {
            return _queries;
        }
        
        public int CreatePureEntity() {
            int e;
            if (_freeEntities.Count > 0) {
                e = _freeEntities.Last();
                _freeEntities.RemoveLast();
                return e;
            }

            e = _lastNewEntity;
            SetEntityArchetypeID(e, Archetype.Empty);
            _lastNewEntity++;
            return e;
        }
        
        public Entity CreateEntity() {
            Entity e;
            if (_freeEntities.Count > 0) {
                e = _entities[_freeEntities.Last()];
                _freeEntities.RemoveLast();
                return e;
            }

            e = new GameObject($"Entity #{_lastNewEntity}").AddComponent<Entity>();
            e.Index = _lastNewEntity;
            _lastNewEntity++;
            return e;
        }

        public Entity GetEntity(int index) {
            return _entities[index];
        }
        
        internal void RegisterEntity(Entity entity) {
            entity.Archetype = _archetypesMap[Archetype.Empty];
            if (_entities.Length <= _lastNewEntity) {
                var newLen = _lastNewEntity + 16;
                Array.Resize(ref _entities, newLen);
            }

            if (_freeEntities.Count > 0) {
                entity.Index = _freeEntities.Last();
                _freeEntities.RemoveLast();
            }
            else {
                entity.Index = _lastNewEntity++;
            }
            _entities[entity.Index] = entity;
            var components = entity.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++) {
                ref var c = ref components[i];
                entity.AddBoxed(c);
            }
        }

        public Entity SpawnEntity(Entity prefab) {
            var entity = Object.Instantiate(prefab);
            entity.world = this;
            entity.Link(this);
            return entity;
        }
        public Entity SpawnEntity(Entity prefab, Vector3 position, Quaternion rotation) {
            var entity = Object.Instantiate(prefab, position, rotation);
            entity.world = this;
            entity.Link(this);
            return entity;
        }
        public void DestroyEntity(Entity entity) {
            _freeEntities.Add(entity.Index);
            entity.UnLink();
            Object.DestroyImmediate(entity.gameObject);
        }

        internal void AddDirtyQuery(Query query) {
            if (!query.IsDirty)
                _dirtyQueries.Add(query);
        }

        public void UpdateQueries() {
            for (var i = 0; i < _dirtyQueries.Count; i++) _dirtyQueries.data[i].Update();
            _dirtyQueries.Clear();
        }

        public Query GetQuery() {
            var q = new Query(this);
            _queries.Add(q);
            return q;
        }

        internal Archetype GetEntityArchetype(int entity) {
            return _archetypesMap[_entityArchetypesIDs[entity]];
        }

        internal void SetEntityArchetypeID(int entity, int archetype) {
            if (_entityArchetypesIDs.Length <= _lastNewEntity) {
                var newLen = entity + 16;
                Array.Resize(ref _entityArchetypesIDs, newLen);
            }
            _entityArchetypesIDs[entity] = archetype;
        }
        internal Archetype GetOrCreateArchetype(HashSet<int> mask) {
            var id = HashCode(mask);
            if (!_archetypesMap.ContainsKey(id)) {
                var newArchetype = new Archetype(this, mask, id);
                _archetypesMap.Add(id, newArchetype);
                _archetypesList.Add(newArchetype);
            }

            return _archetypesMap[id];

            int HashCode(HashSet<int> mask) {
                unchecked {
                    const int p = 16777619;
                    var hash = (int)2166136261;

                    foreach (var i in mask) hash = (hash ^ i) * p;
                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }

        public void OnUpdate(float deltaTime) {
            if(!_alive) return;
            _data.DeltaTime = deltaTime;
            _systems.OnUpdate(ref _data);
        }

        public void OnFixedUpdate(float deltaTime) {
            if(!_alive) return;
            _data.FixedDeltaTime = deltaTime;
            _systems.OnFixedUpdate(ref _data);
        }

        public void OnLateUpdate() {
            if(!_alive) return;
            _systems.OnLateUpdate(ref _data);
        }
        public World Add<TSystem>() where TSystem : class, ISystem, new() {
            _systems.Add<TSystem>();
            return this;
        }
    }
}
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
        private static World defaultWorld;
        private int[] entityArchetypesIDs;
        private DynamicArray<Archetype> archetypesList;
        private Dictionary<int, Archetype> archetypesMap;
        internal IDependenciesContainer DependenciesContainer;

        private DynamicArray<Query> dirtyQueries;
        private Entity[] entities;
        private readonly DynamicArray<int> freeEntities;
        private readonly DynamicArray<Query> queries;
        private readonly Systems systems;
        private int lastNewEntity;
        private WorldData data;
        public WorldData Data => data;
        internal int QueriesCount => queries.Count;
        private World() {
            entityArchetypesIDs = new int[128];
            archetypesList = new DynamicArray<Archetype>(10);
            archetypesMap = new Dictionary<int, Archetype>();
            archetypesMap.Add(Archetype.Empty, Archetype.EmptyRef(this));
            dirtyQueries = new DynamicArray<Query>(4);
            entities = new Entity[128];
            freeEntities = new DynamicArray<int>(128);
            systems = new Systems(this);
            queries = new DynamicArray<Query>(8);
            lastNewEntity = 0;
        }

        public static World Default => defaultWorld ??= new World();
        

        public World SetDI(IDependenciesContainer dependenciesContainer) {
            DependenciesContainer = dependenciesContainer;
            return this;
        }

        public void Init() {
            systems.Init();
        }

        public DynamicArray<Query> GetAllQueries() {
            return queries;
        }
        
        public int CreatePureEntity() {
            int e;
            if (freeEntities.Count > 0) {
                e = freeEntities.Last();
                freeEntities.RemoveLast();
                return e;
            }

            e = lastNewEntity;
            SetEntityArchetypeID(e, Archetype.Empty);
            lastNewEntity++;
            return e;
        }
        
        public Entity CreateEntity() {
            Entity e;
            if (freeEntities.Count > 0) {
                e = entities[freeEntities.Last()];
                freeEntities.RemoveLast();
                return e;
            }

            e = new GameObject($"Entity #{lastNewEntity}").AddComponent<Entity>();
            e.Index = lastNewEntity;
            lastNewEntity++;
            return e;
        }

        public Entity GetEntity(int index) {
            return entities[index];
        }
        
        internal void RegisterEntity(Entity entity) {
            entity.Archetype = archetypesMap[Archetype.Empty];
            if (entities.Length <= lastNewEntity) {
                var newLen = lastNewEntity + 16;
                Array.Resize(ref entities, newLen);
            }

            if (freeEntities.Count > 0) {
                entity.Index = freeEntities.Last();
                freeEntities.RemoveLast();
            }
            else {
                entity.Index = lastNewEntity++;
            }
            entities[entity.Index] = entity;
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
            freeEntities.Add(entity.Index);
            entity.UnLink();
            Object.DestroyImmediate(entity.gameObject);
        }

        internal void AddDirtyQuery(Query query) {
            if (!query.IsDirty)
                dirtyQueries.Add(query);
        }

        public void UpdateQueries() {
            for (var i = 0; i < dirtyQueries.Count; i++) dirtyQueries.data[i].Update();
            dirtyQueries.Clear();
        }

        public Query GetQuery() {
            var q = new Query(this);
            queries.Add(q);
            return q;
        }

        internal Archetype GetEntityArchetype(int entity) {
            return archetypesMap[entityArchetypesIDs[entity]];
        }

        internal void SetEntityArchetypeID(int entity, int archetype) {
            if (entityArchetypesIDs.Length <= lastNewEntity) {
                var newLen = entity + 16;
                Array.Resize(ref entityArchetypesIDs, newLen);
            }
            entityArchetypesIDs[entity] = archetype;
        }
        internal Archetype GetOrCreateArchetype(HashSet<int> mask) {
            var id = HashCode(mask);
            if (!archetypesMap.ContainsKey(id)) {
                var newArchetype = new Archetype(this, mask, id);
                archetypesMap.Add(id, newArchetype);
                archetypesList.Add(newArchetype);
            }

            return archetypesMap[id];

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
            data.DeltaTime = deltaTime;
            systems.OnUpdate(ref data);
        }

        public void OnFixedUpdate(float deltaTime) {
            data.FixedDeltaTime = deltaTime;
            systems.OnFixedUpdate(ref data);
        }

        public void OnLateUpdate() {
            systems.OnLateUpdate(ref data);
        }
        public World Add<TSystem>() where TSystem : class, ISystem, new() {
            systems.Add<TSystem>();
            return this;
        }
    }
}
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wargon.TinyEcs {
    public sealed class Archetype {
        internal const int Empty = 0;
        private readonly Dictionary<int, ArchetypeEdge> Edges;
        private readonly HashSet<int> hashMask;

        internal readonly int id;
        private readonly DynamicArray<Query> queries;
        private readonly World world;
        private int queriesCount;

        private Archetype(World world) {
            hashMask = new HashSet<int>();
            Edges = new Dictionary<int, ArchetypeEdge>();
            queries = new DynamicArray<Query>(3);
            id = 0;
            queriesCount = 0;
            hashMask = new HashSet<int>();
            this.world = world;
        }

        internal Archetype(World world, HashSet<int> hashMaskSource, int archetypeId) {
            queries = new DynamicArray<Query>(3);
            Edges = new Dictionary<int, ArchetypeEdge>();
            id = archetypeId;
            queriesCount = 0;
            hashMask = hashMaskSource;
            this.world = world;
            var worldQueries = world.GetAllQueries();
            var count = world.QueriesCount;
            for (var i = 0; i < count; i++) FilterQuery(worldQueries.data[i]);
        }

        internal static Archetype EmptyRef(World world) {
            return new(world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TransferAdd(in int entity, in int component) {
            if (Edges.TryGetValue(component, out var edge)) {
                edge.Add.Execute(in entity);
                world.GetEntity(entity).Archetype = edge.Add.archetypeTo;
                world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
                return;
            }

            CreateEdges(in component);
            edge = Edges[component];
            edge.Add.Execute(in entity);
            world.GetEntity(entity).Archetype = edge.Add.archetypeTo;
            world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void TransferRemove(in int entity, in int component) {
            if (Edges.TryGetValue(component, out var edge)) {
                edge.Remove.Execute(in entity);
                world.GetEntity(entity).Archetype = edge.Remove.archetypeTo;
                world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
                return;
            }

            CreateEdges(in component);
            edge = Edges[component];
            edge.Remove.Execute(in entity);
            world.GetEntity(entity).Archetype = edge.Remove.archetypeTo;
            world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PureTransferAdd(in int entity, in int component) {
            if (Edges.TryGetValue(component, out var edge)) {
                edge.Add.Execute(in entity);
                world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
                return;
            }

            CreateEdges(in component);
            edge = Edges[component];
            edge.Add.Execute(in entity);
            world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PureTransferRemove(in int entity, in int component) {
            if (Edges.TryGetValue(component, out var edge)) {
                edge.Remove.Execute(in entity);
                world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
                return;
            }

            CreateEdges(in component);
            edge = Edges[component];
            edge.Remove.Execute(in entity);
            world.SetEntityArchetypeID(entity, edge.Add.archetypeTo.id);
        }

        private void CreateEdges(in int component) {
            var maskAdd = new HashSet<int>(hashMask);
            maskAdd.Add(component);
            var maskRemove = new HashSet<int>(hashMask);
            maskRemove.Remove(component);

            Edges.Add(component, new ArchetypeEdge(
                GetOrCreateMigration(world.GetOrCreateArchetype(maskAdd)),
                GetOrCreateMigration(world.GetOrCreateArchetype(maskRemove))
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MigrationEdge GetOrCreateMigration(Archetype archetypeNext) {
            MigrationEdge migrationEdge = new(archetypeNext);
            for (var i = 0; i < queries.Count; i++) {
                var query = queries[i];
                if (!archetypeNext.HasQuery(query))
                    migrationEdge.AddQueryToRemoveEntity(query);
            }

            for (var i = 0; i < archetypeNext.queries.Count; i++) {
                var query = archetypeNext.queries[i];
                if (!HasQuery(query))
                    migrationEdge.AddQueryToAddEntity(query);
            }

            return migrationEdge;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddEntity(int entityId) {
            for (var i = 0; i < queriesCount; i++) queries[i].PreAddWith(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveEntity(int entityId) {
            for (var i = 0; i < queriesCount; i++) queries[i].PreRemoveWith(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FilterQuery(Query query) {
            if (QueryMatchWithArchetype(query)) {
                queries.Add(query);
                queriesCount++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool QueryMatchWithArchetype(Query query) {
            foreach (var i in query.Without)
                if (HasComponent(i))
                    return false;
            var checks = 0;
            foreach (var i in query.With)
                if (HasComponent(i)) {
                    checks++;
                    if (checks == query.With.Count) {
                        queries.Add(query);
                        queriesCount++;
                        return true;
                    }
                }

            foreach (var i in query.Any)
                if (HasComponent(i))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasQuery(Query query) {
            for (var i = 0; i < queries.Count; i++)
                if (queries[i].Equals(query))
                    return true;

            return false;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // internal void RemoveEntityFromPools(int entity) {
        //     for (var i = 0; i < maskArray.Count; i++) {
        //         world.GetPoolByIndex(maskArray.Types[i]).Remove(entity);
        //     }
        // }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasComponent(int type) {
            return hashMask.Contains(type);
        }

        private class ArchetypeEdge {
            public readonly MigrationEdge Add;
            public readonly MigrationEdge Remove;

            public ArchetypeEdge(MigrationEdge add, MigrationEdge remove) {
                Add = add;
                Remove = remove;
            }
        }

        private class MigrationEdge {
            internal readonly Archetype archetypeTo;
            private readonly DynamicArray<Query> QueriesToAddEntity;
            private readonly DynamicArray<Query> QueriesToRemoveEntity;
            private bool IsEmpty;

            internal MigrationEdge(Archetype archetypeto) {
                archetypeTo = archetypeto;
                QueriesToAddEntity = new DynamicArray<Query>(1);
                QueriesToRemoveEntity = new DynamicArray<Query>(1);
                IsEmpty = true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Execute(in int entity) {
                if (IsEmpty) return;
                for (var i = 0; i < QueriesToAddEntity.Count; i++) QueriesToAddEntity[i].PreAddWith(entity);
                for (var i = 0; i < QueriesToRemoveEntity.Count; i++) QueriesToRemoveEntity[i].PreRemoveWith(entity);
            }

            private bool HasQueryToAddEntity(Query query) {
                for (var i = 0; i < QueriesToAddEntity.Count; i++)
                    if (QueriesToAddEntity[i] == query)
                        return true;
                return false;
            }

            private bool HasQueryToRemoveEntity(Query query) {
                for (var i = 0; i < QueriesToRemoveEntity.Count; i++)
                    if (QueriesToRemoveEntity[i] == query)
                        return true;
                return false;
            }

            internal void AddQueryToRemoveEntity(Query query) {
                if (HasQueryToRemoveEntity(query)) return;
                QueriesToRemoveEntity.Add(query);
                IsEmpty = false;
            }

            internal void AddQueryToAddEntity(Query query) {
                if (HasQueryToAddEntity(query)) return;
                QueriesToAddEntity.Add(query);
                IsEmpty = false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Wargon.TinyEcs {
    public class Entity : MonoBehaviour {
        public int Index;
        internal Archetype Archetype;
        internal Dictionary<int, object> Components;
        internal World world;
        private bool created;
        private void Start() {
            Link(World.Default);
        }

        public void Link(World worldRef) {
            if(created) return;
            Components = new Dictionary<int, object>();
            world = worldRef;
            world.RegisterEntity(this);
            created = true;
        }

        public void UnLink() {
            if(!created) return;
            Archetype.RemoveEntity(Index);
        }
    }

    public static class EntityExtensions {
        public static void AddBoxed(this Entity entity, object component) {
            var componentType = ComponentType.GetIndex(component.GetType());
#if DEBUG
            if (entity.Components.ContainsKey(componentType))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} already has {ComponentType.GetType(componentType)}. It can't be added :C");
#endif
            entity.Components.Add(componentType, component);
            entity.Archetype.TransferAdd(in entity.Index, componentType);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this Entity entity, T component) where T : class {
            var componentType = ComponentType<T>.Index;
#if DEBUG
            if (entity.Components.ContainsKey(componentType))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} already has {ComponentType.GetType(componentType)}. It can't be added :C");
#endif
            entity.Components.Add(componentType, component);
            entity.Archetype.TransferAdd(in entity.Index, componentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this Entity entity) where T : class {
            var componentType = ComponentType<T>.Index;
#if DEBUG
            if (entity.Components.ContainsKey(componentType))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} already has {ComponentType.GetType(componentType)}. It can't be added :C");
#endif
            entity.Components.Add(componentType, default);
            entity.Archetype.TransferAdd(in entity.Index, componentType);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this Entity entity) where T : class {
            var componentType = ComponentType<T>.Index;
#if DEBUG
            if (!entity.Components.ContainsKey(componentType))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} has not {ComponentType.GetType(componentType)}. It can't be removed :C");
#endif
            entity.Components.Remove(componentType);
            entity.Archetype.TransferRemove(in entity.Index, componentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Entity entity, int componentType) {
#if DEBUG
            if (!entity.Components.ContainsKey(componentType))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} has not {ComponentType.GetType(componentType)}. It can't be removed :C");
#endif
            entity.Archetype.TransferRemove(in entity.Index, componentType);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : class {
            return entity.Archetype.HasComponent(ComponentType<T>.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(this Entity entity) where T : class {
#if DEBUG
            if (!entity.Components.ContainsKey(ComponentType<T>.Index))
                throw new KeyNotFoundException(
                    $"Entity {entity.name} has not {ComponentType.GetType(ComponentType<T>.Index)}. It can't be geted :C");
#endif
            return (T)entity.Components[ComponentType<T>.Index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Entity entity) {
            entity.Add<DestroyEntity>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this int entity, World world) where T: class, IPureComponent {
            world.GetEntityArchetype(entity).PureTransferAdd(in entity, ComponentType<T>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this int entity, World world) where T: class, IPureComponent {
            world.GetEntityArchetype(entity).PureTransferRemove(in entity, ComponentType<T>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this int entity, World world) {
            world.GetEntityArchetype(entity).RemoveEntity(entity);
        }
    }
    public interface IPureComponent { }
    public struct ComponentType<TComponent> where TComponent : class {
        public static readonly int Index;
        static ComponentType() {
            Index = ComponentType.GetIndex<TComponent>();
        }
    }

    public struct ComponentType {
        private static int Count;
        private static readonly Dictionary<Type, int> indexByType;
        private static readonly Dictionary<int, Type> typeByIndex;

        static ComponentType() {
            indexByType = new Dictionary<Type, int>();
            typeByIndex = new Dictionary<int, Type>();
        }
        private static void Add<TComponent>() {
            var index = Count++;
            indexByType.Add(typeof(TComponent), index);
            typeByIndex.Add(index, typeof(TComponent));
        }

        private static void Add(Type type) {
            var index = Count++;
            indexByType.Add(type, index);
            typeByIndex.Add(index, type);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex<TComponet>() where TComponet : class {
            if (indexByType.TryGetValue(typeof(TComponet), out var idx)) return idx;
            Add<TComponet>();
            return indexByType[typeof(TComponet)];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex(Type type) {
            if (indexByType.TryGetValue(type, out var idx)) return idx;
            Add(type);
            return indexByType[type];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetType(int index) {
            return typeByIndex[index];
        }
    }
}
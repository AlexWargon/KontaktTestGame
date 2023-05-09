
using System;
using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
    [DefaultExecutionOrder(ExecutionOrder.GameCore)]
    public class GameCore : MonoBehaviour {
        private World _world;
        private void Awake() {
            _world = World.Default;
            _world
                .Add<PlayerInputSystem>()
                .Add<EnemyAiSystem>()
                .Add<MineExplosionSystem>()
                .Add<MoveSystem>()
                .Add<ClearTriggersSystem>()
                .Init();
            
        }

        private void Update() {
            _world.OnUpdate(Time.deltaTime);
        }
        private void FixedUpdate() {
            _world.OnFixedUpdate(Time.fixedDeltaTime);
        }
        private void LateUpdate() {
            _world.OnLateUpdate();
        }
    }
    
    sealed class PlayerInputSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().WithAll(typeof(InputData), typeof(PlayerTag));
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var input = entity.Get<InputData>();
                input.Axises.x = Input.GetAxis("Horizontal");
                input.Axises.y = Input.GetAxis("Vertical");
            }
        }
    }

    sealed class MoveSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery()
                .WithAny(typeof(PlayerTag), typeof(EnemyTag))
                .WithAll(typeof(InputData), typeof(MoveSpeed), typeof(Transform));
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var transform = entity.Get<Transform>();
                var movespeed = entity.Get<MoveSpeed>();
                var input = entity.Get<InputData>();
                transform.position += new Vector3(input.Axises.x, 0, input.Axises.y) * movespeed.Value * world.Data.DeltaTime;
            }
        }
    }

    
    sealed class EnemyAiSystem : ISystem {
        private Query _enemies;
        private Query _players;
        public void OnCreate(World world) {
            _enemies = world.GetQuery().WithAll(typeof(EnemyTag), typeof(InputData),typeof(Transform));
            _players = world.GetQuery().WithAll(typeof(Transform), typeof(PlayerTag));
        }

        public void OnUpdate(World world) {
            foreach (var enemy in _enemies) {
                var input = enemy.Get<InputData>();
                var enemyTransform = enemy.Get<Transform>();
                foreach (var player in _players) {
                    
                    var playerTransform = player.Get<Transform>();
                    var dir = (playerTransform.position - enemyTransform.position).normalized;
                    input.Axises.x = dir.x;
                    input.Axises.y = dir.z;
                }
            }
        }
    }
    sealed class MineExplosionSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().WithAll(typeof(OnTriggerEnterEvent));
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var trigger = entity.Get<OnTriggerEnterEvent>();
                if (trigger.Other.Has<PlayerTag>()) {
                    // world.SpawnEntity(entity.Get<DeathEffect>().Value);
                    trigger.Other.Destroy();
                    Debug.Log("BOOM");
                }
            }
        }
    }

    sealed class ClearTriggersSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().With<OnTriggerEnterEvent>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                entity.Remove<OnTriggerEnterEvent>();
            }
        }
    }
    public sealed class OnTriggerEnterEvent {
        public Entity Other;
    }
    
    sealed class LifeTimeSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().With<LifeTime>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var lifeTime = entity.Get<LifeTime>();
                lifeTime.Value -= world.Data.DeltaTime;
                if(lifeTime.Value <= 0) entity.Destroy();
            }
        }
    }

    public interface IPool {
        Entity Spawn(Entity prefab, Vector3 position, Quaternion rotation);
    }
    public class Pool : IPool {
        public Entity Spawn(Entity prefab, Vector3 position, Quaternion rotation) {
            throw new NotImplementedException();
        }
    }
}


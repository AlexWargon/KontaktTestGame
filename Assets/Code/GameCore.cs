
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
                .Add<LifeTimeSystem>()
                .Add<OnPlayerDeathSystem>()
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
    
    sealed class PlayerInputSystem : ISystem{
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
            _enemies = world.GetQuery().WithAll(typeof(EnemyTag), typeof(InputData),typeof(Transform)).Without<Disabled>();
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
                    world.SpawnEntity(entity.Get<Explosion>().Value, entity.Get<Transform>().position, Quaternion.identity);
                    trigger.Other.Destroy();
                    world.CreatePureEntity().Add<PlayerDeathEvent>(world);
                    entity.Add<Disabled>();
                    if(entity.Has<InputData>())
                        entity.Get<InputData>().Axises = Vector2.zero;
                    Debug.Log("BOOM");
                }
            }
        }
    }

    sealed class OnPlayerDeathSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().With<PlayerDeathEvent>();
        }

        public void OnUpdate(World world) {
            for (var i = 0; i < _query.Count; i++) {
                var entity = _query.GetPureEntity(i);
                entity.Destroy(world);
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
    public sealed class Disabled {}
    public sealed class PlayerDeathEvent : IPureComponent {}
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
}


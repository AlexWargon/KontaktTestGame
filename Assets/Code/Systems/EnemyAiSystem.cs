using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
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
}
using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class EnemyAiSystem : ISystem {
        private Query _enemies;
        private Query _players;

        public void OnCreate(World world) {
            _enemies = world.GetQuery().WithAll(typeof(EnemyTag), typeof(InputData), typeof(Transform))
                .Without<Disabled>().Without<AiDelay>();
            _players = world.GetQuery().WithAll(typeof(Transform), typeof(PlayerTag));
        }

        public void OnUpdate(World world) {
            foreach (var enemy in _enemies) {
                var input = enemy.Get<InputData>();
                var enemyTransform = enemy.Get<Transform>();
                foreach (var player in _players) {
                    var playerTransform = player.Get<Transform>();
                    var dir = (playerTransform.position - enemyTransform.position).normalized;
                    input.Axis.x = dir.x;
                    input.Axis.y = dir.z;
                }
            }
        }
    }
}
using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class SpawnerSystem : ISystem {
        private Query _query;

        public void OnCreate(World world) {
            _query = world.GetQuery().With<CubeSpawner>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var spawner = entity.Get<CubeSpawner>();
                spawner.DelayLeft -= world.Data.DeltaTime;
                if (spawner.DelayLeft <= 0) {
                    spawner.DelayLeft = spawner.Delay;
                    var randomIndex = Random.Range(0, spawner.Points.Length);
                    var spawnPos = spawner.Points[randomIndex].position;
                    world.SpawnEntity(spawner.Prefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }
}
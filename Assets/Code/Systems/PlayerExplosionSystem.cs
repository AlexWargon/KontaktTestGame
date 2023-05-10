using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class PlayerExplosionSystem : ISystem {
        private Query _query;

        public void OnCreate(World world) {
            _query = world.GetQuery().WithAll(typeof(OnTriggerEnterEvent), typeof(Explosion));
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                world.SpawnEntity(entity.Get<Explosion>().Value, entity.Get<Transform>().position,
                    Quaternion.identity);
                entity.Destroy();
                world.CreatePureEntity().Add<PlayerDeathEvent>(world);
            }
        }
    }
}
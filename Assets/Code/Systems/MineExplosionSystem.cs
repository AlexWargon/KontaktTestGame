using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
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
                }
            }
        }
    }
}
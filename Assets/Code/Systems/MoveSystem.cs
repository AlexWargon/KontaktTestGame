using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
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
}
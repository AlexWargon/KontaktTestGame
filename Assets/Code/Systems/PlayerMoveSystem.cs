using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class PlayerMoveSystem : ISystem {
        private Query _query;

        public void OnCreate(World world) {
            _query = world.GetQuery()
                .WithAll(typeof(PlayerTag), typeof(InputData), typeof(MoveSpeed), typeof(Transform));
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var transform = entity.GetComponent<Transform>();
                var movespeed = entity.GetComponent<MoveSpeed>();
                var input = entity.GetComponent<InputData>();
                transform.position += new Vector3(input.Axis.x, 0, input.Axis.y) * movespeed.Value *
                                      world.Data.DeltaTime;
            }
        }
    }
}
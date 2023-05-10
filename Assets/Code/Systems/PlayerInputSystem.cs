using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
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
}
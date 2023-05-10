using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class ClearTriggersSystem : ISystem {
        private Query _query;

        public void OnCreate(World world) {
            _query = world.GetQuery().With<OnTriggerEnterEvent>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) entity.Remove<OnTriggerEnterEvent>();
        }
    }
}
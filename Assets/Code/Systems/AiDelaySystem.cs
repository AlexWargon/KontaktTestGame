using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class AiDelaySystem : ISystem {
        private Query _query;

        public void OnCreate(World world) {
            _query = world.GetQuery().With<AiDelay>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var delay = entity.Get<AiDelay>();
                delay.Value -= world.Data.DeltaTime;
                if (delay.Value < 0) entity.Remove<AiDelay>();
            }
        }
    }
}
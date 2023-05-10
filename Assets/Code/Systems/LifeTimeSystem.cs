using Wargon.TinyEcs;

namespace TestGame {
    sealed class LifeTimeSystem : ISystem {
        private Query _query;
        public void OnCreate(World world) {
            _query = world.GetQuery().With<LifeTime>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var lifeTime = entity.Get<LifeTime>();
                lifeTime.Value -= world.Data.DeltaTime;
                if(lifeTime.Value <= 0) entity.Destroy();
            }
        }
    }
}
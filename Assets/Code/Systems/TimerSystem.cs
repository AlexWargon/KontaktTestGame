using Wargon.TinyEcs;

namespace TestGame {
    sealed class TimerSystem : ISystem {
        private RuntimeData _runtimeData;
        public void OnCreate(World world) {
            
        }

        public void OnUpdate(World world) {
            _runtimeData.SesionTime += world.Data.DeltaTime;
        }
    }
}
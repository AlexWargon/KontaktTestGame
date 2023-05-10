using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    [DefaultExecutionOrder(ExecutionOrder.GameCore)]
    public class GameCore : MonoBehaviour {
        private RuntimeData _runtimeData;
        private World _world;

        private void Awake() {
            var di = DI.GetOrCreateContainer();
            di.Build(this);
            _world = World.Default;
            _world
                .SetDI(di)
                .Add<PlayerInputSystem>()
                .Add<EnemyAiSystem>()
                .Add<AiDelaySystem>()
                .Add<PlayerExplosionSystem>()
                .Add<CubeMoveSystem>()
                .Add<PlayerMoveSystem>()
                .Add<LifeTimeSystem>()
                .Add<TimerSystem>()
                .Add<SpawnerSystem>()
                .Add<OnPlayerDeathSystem>()
                .Add<ClearTriggersSystem>()
                .Init();
        }

        private void Update() {
            if (_runtimeData.GameOver) return;
            _world.OnUpdate(Time.deltaTime);
        }
    }
}
using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
    [DefaultExecutionOrder(ExecutionOrder.GameCore)]
    public class GameCore : MonoBehaviour {
        private World _world;
        private void Awake() {
            _world = World.Default;
            _world
                .SetDI(DI.GetOrCreateContainer())
                .Add<PlayerInputSystem>()
                .Add<EnemyAiSystem>()
                .Add<MineExplosionSystem>()
                .Add<MoveSystem>()
                .Add<LifeTimeSystem>()
                .Add<TimerSystem>()
                .Add<SpawnerSystem>()
                .Add<OnPlayerDeathSystem>()
                .Add<ClearTriggersSystem>()
                .Init();
        }

        private void Update() {
            _world.OnUpdate(Time.deltaTime);
        }
    }
}


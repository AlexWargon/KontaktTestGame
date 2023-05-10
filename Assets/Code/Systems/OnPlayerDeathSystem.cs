using UnityEngine;
using Wargon.TinyEcs;
using Wargon.UI;

namespace TestGame {
    sealed class OnPlayerDeathSystem : ISystem {
        private Query _playerDeathEvents;
        private Query _enemies;
        private IUIService _ui;
        private RuntimeData _runtimeData;
        public void OnCreate(World world) {
            _playerDeathEvents = world.GetQuery().With<PlayerDeathEvent>();
            _enemies = world.GetQuery().WithAll(typeof(EnemyTag));
        }

        public void OnUpdate(World world) {
            for (var i = 0; i < _playerDeathEvents.Count; i++) {
                var entity = _playerDeathEvents.GetPureEntity(i);
                entity.Destroy(world);
                foreach (var enemy in _enemies) {
                    enemy.Get<InputData>().Axises = Vector2.zero;
                }
                _ui.Show<GameOverPopup>().SetTime(_runtimeData.SesionTime);
            }
        }
    }
}
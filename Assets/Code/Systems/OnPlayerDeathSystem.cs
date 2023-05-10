using System;
using System.Collections;
using UnityEngine;
using Wargon.TinyEcs;
using Wargon.UI;

namespace Wargon.TestGame {
    internal sealed class OnPlayerDeathSystem : ISystem {
        private Query _enemies;
        private Query _playerDeathEvents;
        private RoutineRunner _routineRunner;
        private RuntimeData _runtimeData;
        private IUIService _ui;

        public void OnCreate(World world) {
            _playerDeathEvents = world.GetQuery().With<PlayerDeathEvent>();
            _enemies = world.GetQuery().WithAll(typeof(EnemyTag), typeof(InputData));
        }

        public void OnUpdate(World world) {
            for (var i = 0; i < _playerDeathEvents.Count; i++) {
                var entity = _playerDeathEvents.GetPureEntity(i);
                entity.Destroy(world);
                foreach (var enemy in _enemies) enemy.Get<InputData>().Axis = Vector2.zero;

                _routineRunner.Run(DelayedAction(1f,
                    () => { _ui.Show<GameOverPopup>().SetTime(_runtimeData.SesionTime); }));
                _runtimeData.GameOver = true;
            }
        }

        private IEnumerator DelayedAction(float time, Action callback) {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
    }
}
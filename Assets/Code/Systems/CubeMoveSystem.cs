using System.Collections;
using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    internal sealed class CubeMoveSystem : ISystem {
        private Query _query;
        private RoutineRunner _routineRunner;
        private World _world;

        public void OnCreate(World world) {
            _world = world;
            _query = world.GetQuery()
                .WithAll(typeof(InputData), typeof(Transform), typeof(Rolling)).Without<AiDelay>();
        }

        public void OnUpdate(World world) {
            foreach (var entity in _query) {
                var transform = entity.Get<Transform>();
                var axis = entity.Get<InputData>().Axis;
                var rolling = entity.Get<Rolling>();

                if (!rolling.Moving) Roll(rolling, GetMoveDirection(axis), transform);
            }
        }

        private Vector3 GetMoveDirection(Vector2 input) {
            var angle = Vector2.SignedAngle(Vector2.right, input);

            return angle switch {
                >= -45f and <= 45f => Vector3.right,
                >= 45f and <= 135f => Vector3.forward,
                >= -135f and <= -45f => Vector3.back,
                _ => Vector3.left
            };
        }

        private void Roll(Rolling rolling, Vector3 direction, Transform transform) {
            rolling.Anchor = transform.position + direction / 2 + Vector3.down / 2;
            rolling.Axis = Vector3.Cross(Vector3.up, direction);
            rolling.RemainingAngle = 90;
            _routineRunner.Run(RollRoutine(rolling, transform));
        }

        private IEnumerator RollRoutine(Rolling rolling, Transform transform) {
            rolling.Moving = true;

            while (rolling.RemainingAngle > 0) {
                var angle = Mathf.Min(_world.Data.DeltaTime * rolling.Speed, rolling.RemainingAngle);
                transform.RotateAround(rolling.Anchor, rolling.Axis, angle);
                rolling.RemainingAngle -= angle;
                yield return null;
            }

            rolling.Moving = false;
        }
    }
}
using UnityEngine;

namespace Wargon.TestGame {
    public sealed class Rolling : MonoBehaviour {
        public bool Moving;
        public float RemainingAngle = 90;
        public Vector3 Anchor;
        public Vector3 Axis;
        public float Speed;
    }
}
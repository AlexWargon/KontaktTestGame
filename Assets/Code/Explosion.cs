using UnityEngine;
using Wargon.TinyEcs;

namespace TestGame {
    [DisallowMultipleComponent]
    public sealed class Explosion : MonoBehaviour {
        public Entity Value;
    }
}
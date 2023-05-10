using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    [DisallowMultipleComponent]
    public sealed class Explosion : MonoBehaviour {
        public Entity Value;
    }
}
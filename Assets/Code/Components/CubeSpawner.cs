using UnityEngine;
using Wargon.TinyEcs;

namespace Wargon.TestGame {
    public sealed class CubeSpawner : MonoBehaviour {
        public float Delay;
        public float DelayLeft;
        public Transform[] Points;
        public Entity Prefab;
    }
}
using System.Collections;
using UnityEngine;

namespace Wargon.TestGame {
    public sealed class RoutineRunner : MonoBehaviour {
        public void Run(IEnumerator routine) {
            StartCoroutine(routine);
        }
    }
}
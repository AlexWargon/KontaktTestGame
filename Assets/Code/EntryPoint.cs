
using UnityEngine;

namespace TestGame {
    [DefaultExecutionOrder(ExecutionOrder.EnterPoint)]
    public class EntryPoint : MonoBehaviour
    {
        void Awake() {

        }
    }

    public struct ExecutionOrder {
        public const int EnterPoint = -25;
        public const int GameCore = -24;
    }
}

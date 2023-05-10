using UnityEngine;
using Wargon.TinyEcs;
using Wargon.UI;

namespace Wargon.TestGame {
    [DefaultExecutionOrder(ExecutionOrder.EnterPoint)]
    public class EntryPoint : MonoBehaviour {
        [SerializeField] private RuntimeData _runtimeData;
        [SerializeField] private UIRoot _uiRoot;
        [SerializeField] private Joystick _joystick;
        [SerializeField] private RoutineRunner _routineRunner;

        private void Awake() {
            Application.targetFrameRate = 60;
            var uiService = new UIService(
                new UIFactory(_uiRoot._elementsList),
                _uiRoot.MenuScreenRoot,
                _uiRoot.PopupRoot,
                _uiRoot.CanvasGroup);
            var di = DI.GetOrCreateContainer();
            di.Register(_runtimeData);
            di.Register<IUIService>().From(uiService);
            di.Register(_joystick);
            di.Register(_routineRunner);
        }
    }

    public struct ExecutionOrder {
        public const int EnterPoint = -25;
        public const int GameCore = -24;
    }
}
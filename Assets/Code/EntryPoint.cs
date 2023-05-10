using UnityEngine;
using Wargon.TinyEcs;
using Wargon.UI;

namespace TestGame {
    [DefaultExecutionOrder(ExecutionOrder.EnterPoint)]
    public class EntryPoint : MonoBehaviour {
        [SerializeField] private RuntimeData _runtimeData;
        [SerializeField] private UIRoot _uiRoot;
        void Awake() {
            var uiService = new UIService(
                new UIFactory(_uiRoot._elementsList),
                _uiRoot.MenuScreenRoot,
                _uiRoot.PopupRoot, 
                _uiRoot.CanvasGroup);
            var di = DI.GetOrCreateContainer();
            di.Register<RuntimeData>().From(_runtimeData);
            di.Register<IUIService>().From(uiService);
        }
    }

    public struct ExecutionOrder {
        public const int EnterPoint = -25;
        public const int GameCore = -24;
    }
}

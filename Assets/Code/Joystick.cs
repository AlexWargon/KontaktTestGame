using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wargon.TestGame {
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {
        [SerializeField] private float _dragTrashHold;
        [SerializeField] private float _dragMoveDistance;
        [SerializeField] private int _dragOffsetDistance;
        [SerializeField] private Image _joystickBackgroundImage;
        [SerializeField] private Image _joystickHandleImage;
        [SerializeField] private Vector2 _inputVector;
        public Vector2 Axis => _inputVector;

        private void Start() {
            _joystickBackgroundImage = GetComponent<Image>();
            _joystickHandleImage = transform.GetChild(0).GetComponent<Image>();
        }

        public void OnDrag(PointerEventData eventData) {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _joystickBackgroundImage.rectTransform,
                eventData.position,
                null,
                out pos)) {
                pos = Vector2.ClampMagnitude(pos, _dragOffsetDistance) / _dragOffsetDistance;

                _joystickHandleImage.rectTransform.anchoredPosition = pos * _dragMoveDistance;
                // var sizeDelta = joystickBackgroundImage.rectTransform.sizeDelta;
                // pos.x = (pos.x / sizeDelta.x);
                // pos.y = (pos.y / sizeDelta.y);
                //
                _inputVector.x = Mathf.Abs(pos.x) > _dragTrashHold ? pos.x : 0;
                _inputVector.y = Mathf.Abs(pos.y) > _dragTrashHold ? pos.y : 0;
                //
                // joystickHandleImage.rectTransform.anchoredPosition = new Vector3(
                //     inputVector.x * (sizeDelta.x / 3),
                //     inputVector.y * (sizeDelta.y / 3));
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            _inputVector = Vector3.zero;
            _joystickHandleImage.rectTransform.anchoredPosition = Vector3.zero;
        }
    }
}
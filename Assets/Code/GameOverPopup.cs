using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wargon.TinyEcs;
using Wargon.UI;

namespace Wargon.TestGame {
    public class GameOverPopup : Popup {
        [SerializeField] private Button _restartButton;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private RectTransform _rectTransform;

        private void Start() {
            _restartButton.onClick.AddListener(OnClickRestart);
        }

        private void OnClickRestart() {
            World.Default.Destroy();
            DI.GetOrCreateContainer().Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            UIService.Hide<GameOverPopup>();
        }

        public void SetTime(float time) {
            var minutes = (int)(time / 60);
            var remainingSeconds = (int)(time - minutes * 60);
            _resultText.text = $"YOUR TIME {minutes}:{remainingSeconds:00}";
        }

        public override void PlayShowAnimation(Action callback = null) {
            StartCoroutine(
                PuffShow(_rectTransform, 0.4f, 0f, 1.2f,
                    PuffShow(_rectTransform, 0.1f, 1.2f, 1f, null, callback))
            );
        }

        public override void PlayHideAnimation(Action callback = null) {
            StartCoroutine(
                PuffShow(_rectTransform, 0.4f, 0f, 1.2f,
                    PuffShow(_rectTransform, 0.1f, 1.2f, 1f, null, callback))
            );
        }

        private IEnumerator PuffShow(RectTransform rectTransform, float duration, float startScale, float endScale,
            IEnumerator routineOnEnd = null, Action callback = null) {
            var startScaleV = new Vector3(startScale, startScale, 0);
            var baseScale = rectTransform.localScale;
            var t = 0f;
            while (t < duration) {
                t += Time.deltaTime;
                var scaleValue = Mathf.Lerp(startScaleV.x, endScale, t / duration);
                rectTransform.localScale = new Vector3(scaleValue, scaleValue, 1f);
                yield return null;
            }

            rectTransform.localScale = baseScale;
            if (routineOnEnd != null)
                StartCoroutine(routineOnEnd);
            callback?.Invoke();
        }
    }
}
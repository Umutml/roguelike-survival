using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Scripts.GameCore.Player
{
    public class PlayerSpeechBubble : MonoBehaviour
    {
        [SerializeField] private GameObject speechBubble;
        [SerializeField] private float speechBubbleShowTime = 3f;
        [SerializeField] private TextMeshPro speechBubbleText;
        private bool _isSpeechBubbleActive;
        private Camera _mainCamera;
        private float _speechBubbleTimer;

        private void Start()
        {
            InitializeSpeechBubble();
        }

        private void Update()
        {
            UpdateSpeechBubbleTimer();
        }

        private void UpdateSpeechBubbleTimer()
        {
            if (!_isSpeechBubbleActive || !(_speechBubbleTimer > 0)) return;
            _speechBubbleTimer -= Time.deltaTime;
            if (_speechBubbleTimer <= 0)
            {
                HideSpeechBubble();
            }
        }

        public void ShowSpeechBubble(string text, float time = 3f)
        {
            if (speechBubble == null) return;

            speechBubbleText.text = text;
            speechBubbleShowTime = time;
            ResetSpeechBubbleTimer();
            ActivateSpeechBubble();
        }

        public void ShowSpeechBubble(string text)
        {
            if (speechBubble == null) return;

            speechBubbleText.text = text;
            speechBubbleShowTime = float.MaxValue;
            ResetSpeechBubbleTimer();
            ActivateSpeechBubble();
        }

        public void HideSpeechBubble()
        {
            if (speechBubble == null) return;

            speechBubble.transform.DOScale(Vector3.zero, 0.5f).OnComplete(DeactivateSpeechBubble);
        }

        private void InitializeSpeechBubble()
        {
            if (speechBubble == null) return;

            speechBubble.transform.localScale = Vector3.zero;
            _mainCamera = Camera.main;

            if (_mainCamera != null)
            {
                LookAtCamera();
            }
        }

        private void ResetSpeechBubbleTimer()
        {
            _speechBubbleTimer = speechBubbleShowTime;
        }

        private void ActivateSpeechBubble()
        {
            if (speechBubble == null) return;

            speechBubble.SetActive(true);
            _isSpeechBubbleActive = true;
            speechBubble.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero);
        }

        private void DeactivateSpeechBubble()
        {
            if (speechBubble == null) return;

            speechBubble.SetActive(false);
            _isSpeechBubbleActive = false;
        }

        private void LookAtCamera()
        {
            if (_mainCamera == null || speechBubble == null) return;

            var cameraYRotation = _mainCamera.transform.rotation.eulerAngles.y;
            speechBubble.transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
        }
    }
}
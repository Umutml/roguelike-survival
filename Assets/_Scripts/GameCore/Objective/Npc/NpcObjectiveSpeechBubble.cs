using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NpcObjectiveSpeechBubble : AdvancedRichTextManager
{
    [SerializeField] private int typingSpeedMs = 50;
    [SerializeField] private float showTime = 3f;
    [SerializeField] private float maxScale = 1f;
    [SerializeField] private Transform speechBubbleTransform;
    [SerializeField] private bool lookAtCamera = true;
    private Transform _cameraTransform;
    private void Start()
    {
        if (!lookAtCamera) return;
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if(!lookAtCamera) return;
        if (!_cameraTransform) return;
        if(!speechBubbleTransform.gameObject.activeSelf) return;
        speechBubbleTransform.LookAt(_cameraTransform);
    }

    internal async void ExecuteShowSpeechBubble(string messageKey)
    {
        speechBubbleTransform.localScale = Vector3.zero;
        speechBubbleTransform.gameObject.SetActive(true);
        speechBubbleTransform.DOScale(Vector3.one * maxScale, 0.5f);
        await Task.Delay(500,Cts.Token);
        await ShowText(messageKey, typingSpeedMs);
        await Task.Delay((int)showTime * 1000,Cts.Token);
        speechBubbleTransform.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
        {
            speechBubbleTransform.gameObject.SetActive(false);
        });
    }
}

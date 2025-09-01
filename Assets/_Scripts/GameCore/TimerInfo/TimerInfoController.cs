using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class TimerInfoController : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private Animator timerAnimator;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image timerFillImage;

    #endregion


    #region Fields

    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");
    private Tween _timerTween;
    private bool _isPlayTimer;
    private float _currentTimer;

    #endregion


    #region Public Methods

    public void SetTimer(float timer, Action onComplete)
    {
        if (_isPlayTimer) return;
        
        _currentTimer = 0;
        _isPlayTimer = true;
        timerAnimator.SetTrigger(Open);
        
        _timerTween = DOTween.To(()=> _currentTimer, x=> _currentTimer = x, timer, timer).SetEase(Ease.Linear).OnUpdate(() =>
        {
            timerText.text = _currentTimer.ToString("F1");
            timerFillImage.fillAmount = _currentTimer / timer;
        }).OnComplete(() =>
        {
            OnComplete(onComplete);
        });
    }
    
    
    public void StopTimer()
    {
        if (!_isPlayTimer) return;
        
        _timerTween.Kill();
        timerAnimator.SetTrigger(Close);
        _isPlayTimer = false;
    }

    #endregion


    #region Private Methods

    private void OnComplete(Action onComplete)
    {
        onComplete?.Invoke();
        timerAnimator.SetTrigger(Close);
        _isPlayTimer = false;
    }

    #endregion
}

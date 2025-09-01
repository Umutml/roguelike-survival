using System;
using DG.Tweening;
using GameCore.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MobHealthManager : MonoBehaviour
{
    #region Serializable Fields


    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider healthSliderV2;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private bool isShowHealthText;

    #endregion

    #region Fields
    private Transform _mainCameraTransform;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        healthSlider.value = 1f;
        healthSliderV2.value = 1f;
        _mainCameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if(_mainCameraTransform == null)
            _mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if(_mainCameraTransform == null)
            return;
        transform.LookAt(_mainCameraTransform);
    }

    private void OnDisable()
    {
        healthSlider.value = 1f;
        healthSliderV2.value = 1f;
    }

    #endregion

    #region Private Methods

    internal void SetHealthText(float currentHealth, float maxHealth)
    {
        if (isShowHealthText)
            healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
    }

    internal void OnHealthChanged(float amountValue)
    {
        DOTween.To(()=> 
            healthSlider.value, x=> healthSlider.value = x, amountValue, 0.25f).OnComplete(
            ()=>
            {
                if(amountValue <= 0)
                    gameObject.SetActive(false);
            });
        
        DOTween.To(()=> 
            healthSliderV2.value, x=> healthSliderV2.value = x, amountValue, 0.15f).SetDelay(0.15f).OnComplete(
            ()=>
            {
                if(amountValue <= 0)
                    gameObject.SetActive(false);
            });
    }
    internal void ResetHealthBar()
    {
        healthSlider.value = 1f;
        healthSliderV2.value = 1f;
        gameObject.SetActive(false);
    }

    #endregion
}

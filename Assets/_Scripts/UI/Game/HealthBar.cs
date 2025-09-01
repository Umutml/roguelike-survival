using DG.Tweening;
using GameCore.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Game
{
    public class HealthBar : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Transform canvasTransform;
        [SerializeField] private GameObject armorBarObject;

        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider healthSliderV2;
        [SerializeField] private Slider armorSlider;

        [SerializeField] private TMP_Text healthText;
        [SerializeField] private bool isShowHealthText = true;

        #endregion

        #region Fields

        private Transform _mainCameraTransform;

        private PlayerStatusController _playerStatusController;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            healthSlider.value = 1f;
            _mainCameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            canvasTransform.LookAt(_mainCameraTransform);
        }

        private void OnDestroy()
        {
            _playerStatusController.HealthChanged -= OnHealthChanged;
            _playerStatusController.ArmorChanged -= OnArmorChanged;
            _playerStatusController.HealthChanged -= SetHealthText;
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(PlayerStatusController statusController)
        {
            _playerStatusController = statusController;
            _playerStatusController.HealthChanged += OnHealthChanged;
            _playerStatusController.HealthChanged += SetHealthText;
            _playerStatusController.ArmorChanged += OnArmorChanged;
        }

        private void OnHealthChanged(float currentHealth, float maxHealth, bool isIncrease)
        {
            DOTween.To(() =>
                healthSlider.value, x => healthSlider.value = x, currentHealth / maxHealth, 0.25f);
            DOTween.To(() =>
                healthSliderV2.value, x => healthSliderV2.value = x, currentHealth / maxHealth, 0.15f).SetDelay(0.15f);
        }

        private void OnArmorChanged(float currentArmor, float maxArmor, bool isIncrease)
        {
            armorBarObject.SetActive(currentArmor > 0);

            if (armorSlider == null)
            {
                return;
            }
            
            
            DOTween.To(() =>
                armorSlider.value, x => armorSlider.value = x, currentArmor / maxArmor, 0.25f);
        }


        private void SetHealthText(float currentHealth, float maxHealth, bool isIncrease)
        {
            if (healthText is null) return;

            if (isShowHealthText)
            {
                healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
            }
        }

        #endregion
    }
}
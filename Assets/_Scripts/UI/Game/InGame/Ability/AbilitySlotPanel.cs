using System.Threading;
using _Scripts.GameCore.Vibration.Constants;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Game.InGame.Ability
{
    public class AbilitySlotPanel : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private AlertManager alertManager;
        [SerializeField] private VibrationManager vibrationManager;
        [SerializeField] private Image abilityImage;
        [SerializeField] private Image grayScaleAbilityImage;
        [SerializeField] private int unlockLevel;

        #endregion

        #region Fields

        private bool _abilityInstalled;
        private IAbility _attachedAbility;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
        }

        private async void OnAbilityUsed()
        {
            if (!_abilityInstalled) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            grayScaleAbilityImage.fillAmount = 1;

            while (_attachedAbility.IsOnCooldown)
            {
                grayScaleAbilityImage.fillAmount =
                    _attachedAbility.CurrentCooldownTime / _attachedAbility.MaxCooldownTime;
                await UniTask.Delay(50);
            }

            grayScaleAbilityImage.fillAmount = 0;

            transform.DOPunchScale(Vector3.one * 0.3f, 0.3f).SetUpdate(true);
        }

        #endregion

        #region Public Methods

        public void InstallAbility(IAbility ability)
        {
            _attachedAbility = ability;

            abilityImage.sprite = _attachedAbility.Icon;
            grayScaleAbilityImage.sprite = _attachedAbility.Icon;

            _attachedAbility.AbilityUsed += OnAbilityUsed;

            _abilityInstalled = true;
        }

        public void UseAbility()
        {
            vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
            alertManager.CallAlert($"Unlock at level {unlockLevel}");
            //_attachedAbility?.Execute();
        }

        #endregion
    }
}

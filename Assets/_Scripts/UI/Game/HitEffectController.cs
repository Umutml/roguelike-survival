using System.Collections;
using GameCore.Health;
using UI.Game.Architectural;
using UnityEngine;
using VContainer;

namespace UI.Game
{
    public class HitEffectController : Content
    {
        private readonly int HitKey = Animator.StringToHash("Hit");
        private readonly WaitForSeconds AnimationWaitForSeconds = new(0.5f);

        private PlayerStatusController _playerStatusController;
        private Animator _animator;
        private Coroutine _coroutine;
        private bool _subscribed;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        [Inject]
        private void Initialize(PlayerStatusController playerStatusController)
        {
            if (_subscribed) { return; }

            _playerStatusController = playerStatusController;
            _playerStatusController.HealthChanged += OnHealthChanged;
            _subscribed = true;
        }

        private void OnHealthChanged(float value, float maxValue, bool isIncrease)
        {
            if (isIncrease) { return; }

            if (_coroutine != null)
            {
                return;
            }

            _coroutine = StartCoroutine(ShowHitEffect());
        }

        private IEnumerator ShowHitEffect()
        {
            _animator.SetTrigger(HitKey);
            yield return AnimationWaitForSeconds;
            _coroutine = null;
        }
    }
}

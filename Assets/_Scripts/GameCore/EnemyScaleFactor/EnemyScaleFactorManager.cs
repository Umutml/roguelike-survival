using System.Collections.Generic;
using _Scripts.GameCore.NPC;
using GameCore.Level;
using GameCore.Scriptables;
using GameCore.Spawner;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCore.EnemyScaleFactor
{
    public class EnemyScaleFactorManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Scriptables.EnemyScaleFactor enemyScaleFactor;
        [SerializeField] private ManagementNpcController managementNpcController;
        [SerializeField] private MobManager mobManager;
        [SerializeField] private WaveLevelManager waveLevelManager;

        #endregion

        #region Private Fields

        private HashSet<Zombie> _factorizedEnemies = new();
        private List<ScaleCondition> _scaleConditions = new();

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            managementNpcController.OnStartManagement += ResetScaleFactor;
            managementNpcController.OnCompleteManagement += ResetScaleFactor;
            waveLevelManager.WaveLevelChanged += OnWaveWaveLevelChanged;
        }


        private void OnDestroy()
        {
            managementNpcController.OnStartManagement -= ResetScaleFactor;
            managementNpcController.OnCompleteManagement -= ResetScaleFactor;
            waveLevelManager.WaveLevelChanged -= OnWaveWaveLevelChanged;
        }

        #endregion

        #region Private Methods

        private void OnWaveWaveLevelChanged(int level)
        {
            if (!managementNpcController.IsProgress)
            {
                return;
            }

            foreach (var condition in enemyScaleFactor.scaleConditions)
            {
                if (level % condition.perLevel != 0)
                {
                    continue;
                }

                SetScaleFactor(condition);
            }
        }

        private void SetScaleFactor(ScaleCondition condition)
        {
            _scaleConditions.Add(condition);
            mobManager.ActiveMobsSet.ForEach(x => ApplyScaleFactor(x, condition));
        }

        private void ApplyScaleFactor(Zombie zombie, ScaleCondition condition)
        {
            _factorizedEnemies.Add(zombie);
            zombie.SetEnemyScaleFactor(condition);
        }

        private void ResetFactorizedEnemies()
        {
            if (_factorizedEnemies is not {Count: > 0})
            {
                return;
            }

            _factorizedEnemies.ForEach(x => x.Setup());
            _factorizedEnemies.Clear();
        }

        private void ResetScaleFactor()
        {
            ResetFactorizedEnemies();
            _scaleConditions.Clear();
            _factorizedEnemies.Clear();
        }

        #endregion

        #region Public Methods

        public void ApplyScaleFactor(Zombie zombie)
        {
            if (!managementNpcController.IsProgress)
            {
                return;
            }

            if (_scaleConditions is not {Count: > 0})
            {
                return;
            }

            _scaleConditions.ForEach(x => ApplyScaleFactor(zombie, x));
        }

        public float CalculateScaleFactor(float value, ScaleCondition condition)
        {
            return condition.valueModifierType switch
            {
                ValueModifierType.Add => value + condition.value,
                ValueModifierType.Subtract => value - condition.value,
                ValueModifierType.MultiplyIncrease => value + (value * condition.value * 0.01f),
                ValueModifierType.MultiplyDecrease => value + (value * condition.value * -0.01f),
                _ => value
            };
        }

        #endregion
    }
}

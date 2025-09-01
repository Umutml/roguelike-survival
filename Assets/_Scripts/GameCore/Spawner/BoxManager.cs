using System;
using System.Collections.Generic;
using System.Threading;
using _Utilities;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.Box;
using GameCore.Drop;
using GameCore.Health;
using GameCore.Player;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using Managers;
using MyBox;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Spawner
{
    public class BoxManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private WeaponDropChances weaponDropChances;
        [SerializeField] private BoxDropChanceData boxDropChanceData;

        #endregion

        #region Private Fields

        private readonly HashSet<(IDropItem, IDamageable)> _subscribedDamageables = new();
        private readonly HashSet<(IDropItem, IDamageable)> _subscribedTutorialDamageables = new();
        private Camera _camera;
        private WaveManager _waveManager;
        private LootDropManager _lootDropManager;
        private PlayerController _playerController;
        private DropIncrementManager _dropIncrementManager;
        private bool _isBoxDropLocked;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Properties

        public HashSet<(IDropItem, IDamageable)> SubscribedDamageables => _subscribedDamageables;
        public HashSet<(IDropItem, IDamageable)> SubscribedTutorialDamageables => _subscribedTutorialDamageables;

        public bool IsBoxDropLocked
        {
            get => _isBoxDropLocked;
            set
            {
                _isBoxDropLocked = value;

                if (!value)
                {
                    SpawnBoxes();
                    return;
                }

                _subscribedDamageables.ForEach(x =>
                {
                    x.Item1.Reset();
                    RemoveBoxFromSubscribed(x.Item2, x.Item1, DamageSource.Player);
                });

                _subscribedDamageables.Clear();
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _camera = Camera.main;
            SpawnBoxes();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Dispose();
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(LootDropManager lootDropManager, WaveManager waveManager,
            DropIncrementManager dropIncrementManager,
            PlayerController playerController)
        {
            _lootDropManager = lootDropManager;
            _dropIncrementManager = dropIncrementManager;
            _waveManager = waveManager;
            _playerController = playerController;
        }

        private async void SpawnBoxes()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));

            if (IsBoxDropLocked)
            {
                return;
            }

            for (var i = 0; i < boxDropChanceData.boxCount; i++)
            {
                await DropBox();
            }
        }

        private async UniTask<Vector3?> FindValidPositionByTransform(Vector3 playerPosition)
        {
            Vector3? validPosition = null;

            await UniTaskAsyncHelper.WaitWhileOrTimeout(() =>
                {
                    validPosition = AstarPathHelper.GetRandomWalkablePosition(
                        playerPosition,
                        boxDropChanceData.boxSpawnRadius,
                        _camera);

                    return validPosition.HasValue && _subscribedDamageables?.Gen()
                        .All(x => Vector3.Distance(x.Item1.Transform.position, validPosition.Value) >
                                  boxDropChanceData.boxBetweenDistance) == false;
                },
                100,
                false,
                3000,
                _cancellationTokenSource.Token);


            return validPosition;
        }

        private void RemoveBoxFromSubscribed(IDamageable damageable, IDropItem dropItem, DamageSource damageSource)
        {
            if (_subscribedTutorialDamageables.Contains((dropItem, damageable)))
            {
                _subscribedTutorialDamageables.Remove((dropItem, damageable));
            }

            if (_subscribedDamageables.Contains((dropItem, damageable)))
            {
                _subscribedDamageables.Remove((dropItem, damageable));
            }
        }

        private DropPodType DetermineDropPodType()
        {
            var totalProbability = boxDropChanceData.dropChances.Gen()
                .Where(x => _waveManager.IsWaveActive || (!x.isWaveOnly && !_waveManager.IsWaveActive))
                .Sum(dc => dc.probability);

            var randomValue = UnityEngine.Random.Range(0, totalProbability);

            var cumulativeProbability = 0;

            foreach (var dropChance in boxDropChanceData.dropChances.Gen().Where(dc =>
                         _waveManager.IsWaveActive || (!dc.isWaveOnly && !_waveManager.IsWaveActive)))
            {
                cumulativeProbability += dropChance.probability;
                if (randomValue < cumulativeProbability)
                {
                    return dropChance.dropPodType;
                }
            }

            return DropPodType.Xp;
        }

        #endregion

        #region Public Methods

        public async UniTask<IDropItem> GetDropObject(Vector3 dropPosition, BoxController.BoxConfig? config)
        {
            var dropPodType = config?.ForcedDropPodType ?? DetermineDropPodType();

            var targetDrop = boxDropChanceData.dropChances.Gen().Where(x => x.dropPodType == dropPodType)
                .FirstOrDefault();
            var value = targetDrop.hasValue
                ? UnityEngine.Random.Range(targetDrop.minValue, targetDrop.maxValue)
                : 1;

            if (targetDrop.isDelay)
            {
                await UniTask.Delay(targetDrop.delayValue);
            }

            var dropObject = await _lootDropManager.GetDropObject(dropPodType, dropPosition);

            if (dropPodType == DropPodType.Weapon)
            {
                var weaponDrop = dropObject.GetComponent<WeaponDrop>();
                weaponDrop.WeaponKey = weaponDropChances.GetRandomWeaponByChance();
                weaponDrop.PlayerController = _playerController;
            }

            var dropItem = dropObject.GetComponent<IDropItem>();
            dropItem.Initialize(value);

            if (config?.IsDisabledDropIncrement == true) return dropItem;
            if (!targetDrop.canIncrementDrop) return dropItem;
            if (_playerController != null)
            {
                _dropIncrementManager.DropIncrementItem(_playerController.transform.position, value, dropPodType);
            }

            dropItem.Use();

            return dropItem;
        }


        public async UniTask<GameObject> DropBox(Vector3? customPosition = null)
        {
            if (_playerController != null)
                customPosition ??= await FindValidPositionByTransform(_playerController.transform.position);

            if (!customPosition.HasValue)
            {
#if DEBUG_LOGS_ENABLED
                LoggerNS.LogWarning("No valid position found for box drop.");
#endif
                return null;
            }

            customPosition = new Vector3(customPosition.Value.x, customPosition.Value.y + 0.5f, customPosition.Value.z);

            var boxObject = await _lootDropManager.GetDropObject(DropPodType.Box, customPosition.Value);

            if (boxObject == null)
            {
                return null;
            }

            if (!boxObject.TryGetComponent(out IDropItem dropItem) ||
                !boxObject.TryGetComponent(out IDamageable damageable))
            {
                return null;
            }

            _subscribedDamageables.Add((dropItem, damageable));

            damageable.Died += damageSource => RemoveBoxFromSubscribed(damageable, dropItem, damageSource);

            dropItem.Initialize(1);
            return boxObject;
        }


        public IDamageable GetClosestBox(Vector3 position, float? customDistance = null)
        {
            if (_subscribedDamageables is not { Count: > 0 }) return null;

            var boxList = _subscribedDamageables.Gen().Concat(_subscribedTutorialDamageables.Gen()).ToList();

            if (boxList is not { Count: > 0 })
            {
                return null;
            }

            IDamageable closestBox = null;

            var closestDistance = customDistance ?? Mathf.Infinity;

            foreach (var mob in boxList.Gen().Where(dmgbl => dmgbl.Item1 != null && !dmgbl.Item2.IsDead))
            {
                var damageable = mob.Item2;
                var dropItem = mob.Item1;
                if (damageable == null) continue;
                if (dropItem.Transform == null) continue;


                var distance = Vector3.Distance(position, damageable.Position);
                if (!(distance < closestDistance)) continue;
                closestDistance = distance;
                closestBox = damageable;
            }

            if (closestBox != null && _camera.IsInViewport(closestBox.Position))
                return closestBox;

            return null;
        }

        #endregion
    }
}
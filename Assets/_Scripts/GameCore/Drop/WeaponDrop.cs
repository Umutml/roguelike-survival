using System;
using System.Collections;
using System.Collections.Generic;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using GameCore.Spawner;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace GameCore.Drop
{
    public class WeaponDrop : MonoBehaviour, IDropItem
    {
        private const float DestroyDelay = 30;
        private const int PickableDelay = 3;

        #region Serializable Fields

        [SerializeField] private Transform weaponModelParent;
        [SerializeField] private Material transparentMaterial;
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private WeaponProperties weaponProperties;

        #endregion

        #region Fields

        private readonly WaitForSeconds DestroyWaitForSeconds = new(DestroyDelay);
        private Animator _animator;
        private GameObject _modelGo;
        private WeaponProperties.WeaponProperty _weaponProperty;

        #endregion

        #region Properties

        public float PickRadius { get; set; } = 5.7f;
        public PlayerController PlayerController { get; set; }
        public bool WaitForPlayerToMoveout { get; set; }

        public string WeaponKey { get; set; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        #endregion

        #region Private Methods

        private async void SpawnDroppedOldWeapon(string weaponName)
        {
            if (!Resolver.TryResolve(out LootDropManager lootDropManager)) return;
            WeaponKey = weaponName;
            WaitForPlayerToMoveout = true;
            PlayerController = Resolver.Resolve<PlayerController>();
            Initialize(1);
            WaitAndEnablePickable();
        }

        private IEnumerator DestroyDropAfterDelay()
        {
            yield return DestroyWaitForSeconds;
            Reset();
        }

        private async UniTask LoadWeaponModel()
        {
            weaponModelParent.RemoveAllChildren();

            var modelKey = WeaponKey + "Model";
            var weaponModel = await Addressables.LoadAssetAsync<GameObject>(modelKey).BindTo(gameObject);
            _modelGo = Instantiate(weaponModel, weaponModelParent);

            if (_modelGo.TryGetComponent<WeaponModel>(out var model))
            {
                _modelGo.transform.localPosition = model.ModelRotationPivot;
            }
        }

        private async void ChangeWeaponOpacity()
        {
            var originalMaterials = new Dictionary<Renderer, Material>();
            ApplyTransparentMaterial(originalMaterials);
            await UniTask.Delay(TimeSpan.FromSeconds(PickableDelay));
            RevertToOriginalMaterials(originalMaterials);
        }

        private void ApplyTransparentMaterial(Dictionary<Renderer, Material> originalMaterials)
        {
            if (_modelGo == null)
            {
                return;
            }

            foreach (var renderer in _modelGo.GetComponentsInChildren<Renderer>())
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.sharedMaterial;
                    renderer.sharedMaterial = transparentMaterial;
                }
            }
        }

        private void RevertToOriginalMaterials(Dictionary<Renderer, Material> originalMaterials)
        {
            foreach (var kvp in originalMaterials)
            {
                kvp.Key.sharedMaterial = kvp.Value;
            }

            originalMaterials.Clear();
        }

        private async void WaitAndEnablePickable()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(PickableDelay));
            IsPickable = true;
        }

        private async void WaitForPlayerToMoveOut()
        {
            var playerTransform = PlayerController.transform;
            var playerPosition = playerTransform.position;
            await UniTask.WaitWhile(() => Vector3.Distance(playerPosition, playerTransform.position) < PickRadius);
            IsPickable = true;
        }

        private void SetWeaponNameText()
        {
            weaponNameText.text = _weaponProperty.InGameName;
        }

        private void ShowAcquiredWeaponText()
        {
            if (Resolver.TryResolve(out DamageNumberManager damageNumberManager))
            {
                damageNumberManager.UseDamageNumber(PlayerController.transform.position,
                    $"{_weaponProperty.InGameName} acquired", false);
            }
        }

        #endregion

        #region IDropItem Members

        public int Value { get; private set; }
        public IObjectResolver Resolver { get; set; }
        public Transform Transform => transform;
        public float? OptionalDistance { get; private set; }
        public bool IsPickedUp { get; private set; }
        public bool IsPickable { get; set; } = false;

        public async void Initialize(int value, bool isHidden = false)
        {
            Value = value;

            await LoadWeaponModel();

            _animator.enabled = true;
            StartCoroutine(nameof(DestroyDropAfterDelay));

            _weaponProperty = weaponProperties.GetPropertyByKey(WeaponKey);
            SetWeaponNameText();

            if (IsPickable)
            {
                return;
            }

            ChangeWeaponOpacity();

            if (!WaitForPlayerToMoveout)
            {
                WaitAndEnablePickable();
            }
            else
            {
                WaitForPlayerToMoveOut();
            }
        }

        public async void Use()
        {
            PlayerController.PlayOneShotAudio("WeaponCockBack");

            var oldWeaponName =
                await PlayerController.WeaponController.SwitchToWeapon(WeaponKey, WeaponSlot.SlotType.RightHand);

            ShowAcquiredWeaponText();

            if (!string.IsNullOrEmpty(oldWeaponName))
            {
                SpawnDroppedOldWeapon(oldWeaponName);
            }


            IsPickable = false;
        }

        public void Reset()
        {
            IsPickedUp = false;
            IsPickable = false;
            Value = 0;
            if (_animator != null)
            {
                _animator.enabled = false;
            }

            StopCoroutine(nameof(DestroyDropAfterDelay));
            gameObject.SetActive(false);
        }

        #endregion
    }
}
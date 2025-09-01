using System;
using System.Collections.Generic;
using System.Globalization;
using _Utilities;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.Player.Input;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using Michsky.UI.ModernUIPack;
using UI.Game.Architectural;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace UI.Game.InGame.CharacterUpgradePopup
{
    public class CharacterUpgrade : Content
    {
        [SerializeField] private SpriteDatabase spriteDatabase;
        [SerializeField] private List<CharacterUpgradeSegment> characterUpgradeSegments;
        [SerializeField] private List<CharacterUpgradeSegment> allCharacterUpgradeSegments;
        [SerializeField] private CanvasGroup statsArea;
        [SerializeField] private CanvasGroup allStatsArea;

        private const string ContinueButton = "CharacterUpgradeContinueButton";
        private const string CloseButton = "CharacterUpgradeCloseButton";
        private const string LevelSlider = "LevelSlider";
        private const string LevelText = "LevelText";
        private const string CostText = "CostText";
        private const string CostSprite = "CostSprite";
        private const string CharacterImage = "CharacterImage";
        private const string CharacterName = "TitleText";
        private const string InfoArea = "InfoArea";

        private CharacterMetaUpgradeData _characterMetaUpgradeData;
        private CharacterUpgradeResources _characterUpgradeResources;
        private PlayerStatusController _playerStatusController;
        private PlayerMovementController _playerMovementController;
        private PlayerWeaponController _playerWeaponController;
        private ItemPicker _itemPicker;
        private IObjectResolver _resolver;
        private UIGradient _uiGradient;
        private Dictionary<int, StatUpgradeType> _allStats = new();


        private void Awake()
        {
            _uiGradient = GetGameObject(InfoArea).GetComponent<UIGradient>();
        }


        public async void Initialize(IObjectResolver resolver, CharacterResources characterResources,
            List<UpgradeDetail> upgradeDetails, Action onUpgrade, bool isAllStats = false)
        {
            _resolver = resolver;
            _characterMetaUpgradeData = LoadCharacterMetaUpgradeData();
            _characterUpgradeResources = resolver.Resolve<PlayerSkillController>().CharacterUpgradeResources;
            _playerStatusController = _resolver.Resolve<PlayerStatusController>();
            _playerMovementController = _playerStatusController.GetComponent<PlayerMovementController>();
            _itemPicker = _resolver.Resolve<ItemPicker>();
            _playerWeaponController = _resolver.Resolve<PlayerController>().WeaponController;

            var characterImage = await Addressables.LoadAssetAsync<Sprite>(characterResources.CharacterArt)
                .BindTo(gameObject);

            _uiGradient.EffectGradient = characterResources.CharacterGradient;

            SetText(CharacterName, characterResources.CharacterName);
            SetImage(CharacterImage, characterImage);
            ConfigureButtons(onUpgrade);
            ConfigureSlider();
            ConfigureCost();
            ConfigureStats(upgradeDetails);
            InitializeAllStats();
            ConfigureAllStats();
            statsArea.alpha = isAllStats ? 0 : 1;
            allStatsArea.alpha = isAllStats ? 1 : 0;
        }

        private void ConfigureButtons(Action onUpgrade)
        {
            RemoveAllListener();
            OnClickListen(ContinueButton, onUpgrade, _resolver);
            OnClickListen(CloseButton, Close, _resolver);
        }

        private void ConfigureSlider()
        {
            var currentIndex = _characterMetaUpgradeData.UpgradeIndex;
            var maxIndex = _characterUpgradeResources.CharacterUpgradeList.Count - 1;

            SetSlider(LevelSlider, currentIndex, maxIndex);
            SetText(LevelText, $"{currentIndex}/{maxIndex}");
        }

        private async void ConfigureCost()
        {
            var cost = _characterUpgradeResources.CharacterUpgradeList[_characterMetaUpgradeData.UpgradeIndex].Price;
            SetText(CostText, cost.ToString(CultureInfo.InvariantCulture));

            var costSprite = await spriteDatabase.GetSpriteByType(SpriteType.Coin);
            SetImage(CostSprite, costSprite);
        }

        private void ConfigureStats(List<UpgradeDetail> upgradeDetails)
        {
            for (var i = 0; i < characterUpgradeSegments.Count; i++)
            {
                var detail = upgradeDetails[i];
                characterUpgradeSegments[i].InitializeSegment(GetDefaultValue(detail.type), detail);
            }
        }

        private void InitializeAllStats()
        {
            _allStats.Clear();
            _allStats.Add(0, StatUpgradeType.MaxHealth);
            _allStats.Add(1, StatUpgradeType.Speed);
            _allStats.Add(2, StatUpgradeType.Armor);
            _allStats.Add(3, StatUpgradeType.CriticalHitChance);
            _allStats.Add(4, StatUpgradeType.CriticalDamage);
            _allStats.Add(5, StatUpgradeType.AttackSpeed);
        }

        private void ConfigureAllStats()
        {
            for (var i = 0; i < allCharacterUpgradeSegments.Count; i++)
            {
                allCharacterUpgradeSegments[i].InitializeSegment(GetDefaultValue(_allStats[i]),
                    new UpgradeDetail(_allStats[i], 0, ValueModifierType.Add));
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        private float GetDefaultValue(StatUpgradeType type)
        {
            return type switch
            {
                StatUpgradeType.MaxHealth => _playerStatusController.MaxHealth,
                StatUpgradeType.Speed => _playerMovementController.MovementSpeed,
                StatUpgradeType.Armor => _playerStatusController.Armor,
                StatUpgradeType.CriticalHitChance => _playerWeaponController.CalculateCriticalHitChance(),
                StatUpgradeType.CriticalDamage => _playerWeaponController.CalculateCriticalHitDamage(),
                StatUpgradeType.AttackSpeed => _playerWeaponController.CalculateFireInterval(),
                _ => _itemPicker.Radius
            };
        }

        private CharacterMetaUpgradeData LoadCharacterMetaUpgradeData()
        {
            return SaveLoadHelper.TryLoadPersistentData<CharacterMetaUpgradeData>();
        }
    }
}

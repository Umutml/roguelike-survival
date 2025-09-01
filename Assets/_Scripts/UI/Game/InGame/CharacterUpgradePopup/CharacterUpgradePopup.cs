using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.GameCore.NPC;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Interfaces;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Game.InGame.CharacterUpgradePopup
{
    public class CharacterUpgradePopup : Popup
    {
        #region Serialized Fields

        [SerializeField] private AllCharacterResources allCharacterResources;
        [SerializeField] private CharacterUpgrade characterUpgrade;
        [SerializeField] private CharacterUpgradeInfo characterUpgradeInfo;
        [SerializeField] private Button upgradePanelButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button rightArrowButton;
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private CharacterSegment characterSegmentPrefab;
        [SerializeField] private Transform charactersSegmentParent;
        [SerializeField] private TMP_Text alertText;
        [SerializeField] private TMP_Text characterStatusText;
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject tutorialHand;
        [SerializeField] private GameObject infoButton;
        [SerializeField] private Transform selectPoint;
        [SerializeField] private Transform backPoint;
        [SerializeField] private TMP_Text descriptionText;

        #endregion

        #region Private Fields

        private static readonly int Show = Animator.StringToHash("Show");

        private CharacterSegment _characterSegmentInstance;
        private CharacterMetaUpgradeData _characterMetaUpgradeData;
        private IInventoryManager _gameInventoryManager;
        private VibrationManager _vibrationManager;
        private PlayerSkillController _playerSkillController;
        private CharacterUpgradeResources _characterUpgradeResources;
        private ManagementNpcController _managementNpcController;
        private IGeneralOnClickManager _generalOnClickManager;
        private int _currentCharacterIndex;
        private PlayerController _playerController;
        private readonly Dictionary<CharacterResources, CharacterSegment> _characterSegments = new();

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        #endregion


        #region Public Methods

        public override void OnOpenPopup()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _characterMetaUpgradeData = GetCarMetaUpgradeData();
            _gameInventoryManager = Resolver.Resolve<IInventoryManager>();
            _playerSkillController = Resolver.Resolve<PlayerSkillController>();
            _managementNpcController = Resolver.Resolve<ManagementNpcController>();
            _generalOnClickManager = Resolver.Resolve<IGeneralOnClickManager>();
            _playerController = Resolver.Resolve<PlayerController>();
            _vibrationManager = Resolver.Resolve<VibrationManager>();
            _characterUpgradeResources = _playerSkillController.CharacterUpgradeResources;
            characterUpgradeInfo.InitializeCharacter(Resolver,
                GetCurrentCharacterResource(),
                () => SetupCharacterUpgrade(true));
            _generalOnClickManager.RegisterButton(upgradePanelButton, () => SetupCharacterUpgrade());
            _generalOnClickManager.RegisterButton(backButton, ClosePopup);
            _generalOnClickManager.RegisterButton(rightArrowButton, OnClickNextCharacter, "Right");
            _generalOnClickManager.RegisterButton(leftArrowButton, OnClickNextCharacter, "Left");
            _generalOnClickManager.RegisterButton(selectButton, ChooseCurrentCharacter);
            CreateCharacterSegments();
        }

        public override async void InitializeTutorial(object data)
        {
            if (data is not CharacterResources characterResources)
                return;

            try
            {
                await UniTask.WhenAny(UniTask.Delay(2000, true),
                    UniTaskAsyncHelper.WaitUntil(() => _characterSegments.Count > 0,
                        1000,
                        true,
                        _cancellationTokenSource.Token));

                if (_characterSegments.Count <= 0) return;

                var segment = _characterSegments[characterResources];
                var characterIndex = allCharacterResources.CharacterResourcesList.FindIndex(x =>
                    x.Character.Equals(characterResources.Character));

                tutorialHand.SetActive(true);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    backButton.interactable = false;
                    tutorialHand.transform.position = segment.transform.position;

                    await UniTaskAsyncHelper.WaitUntil(() => _currentCharacterIndex == characterIndex,
                        1000,
                        true,
                        _cancellationTokenSource.Token);

                    tutorialHand.transform.position = selectPoint.position;

                    await UniTask.WhenAny(
                        UniTaskAsyncHelper.WaitUntil(() => segment.IsActive,
                            1000,
                            true,
                            _cancellationTokenSource.Token),
                        UniTaskAsyncHelper.WaitUntil(() => _currentCharacterIndex != characterIndex,
                            1000,
                            true,
                            _cancellationTokenSource.Token));

                    if (!segment.IsActive || _currentCharacterIndex != characterIndex) continue;

                    backButton.interactable = true;
                    tutorialHand.transform.position = backPoint.position;

                    await UniTaskAsyncHelper.WaitUntil(() => !segment.IsActive,
                        1500,
                        true,
                        _cancellationTokenSource.Token);

                    if (!segment.IsActive || _currentCharacterIndex != characterIndex) continue;

                    break;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Tutorial cancelled");
            }
        }


        private void OnClickNextCharacter(string buttonType)
        {
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
            _currentCharacterIndex = buttonType.Equals("Right")
                ? (_currentCharacterIndex + 1) % allCharacterResources.CharacterResourcesList.Count
                : (_currentCharacterIndex - 1 + allCharacterResources.CharacterResourcesList.Count) %
                allCharacterResources.CharacterResourcesList.Count;
            characterUpgradeInfo.InitializeCharacter(Resolver,
                allCharacterResources.CharacterResourcesList[_currentCharacterIndex],
                () => SetupCharacterUpgrade());

            UpdateCharacterPageForCurrentCharacter();
        }

        private void UpdateCharacterPageForCurrentCharacter()
        {
            if (IsCurrentCharacterActiveInGame())
                characterStatusText.text = "Selected";
            else if (allCharacterResources.CharacterResourcesList[_currentCharacterIndex].IsLocked)
                characterStatusText.text = "Locked";
            else
                characterStatusText.text = "Select";


            
            

            upgradeButton.SetActive(IsCurrentCharacterActiveInGame());
            infoButton.SetActive(IsCurrentCharacterActiveInGame());
            selectButton.gameObject.SetActive(
                !allCharacterResources.CharacterResourcesList[_currentCharacterIndex].IsLocked &&
                !IsCurrentCharacterActiveInGame());
            
            
            var characterResource = allCharacterResources.CharacterResourcesList[_currentCharacterIndex];
            var isHenry = characterResource.Character.Equals(Character.Henry);
            var isUnlocked = !characterResource.IsLocked && !IsCurrentCharacterActiveInGame();

            if (isHenry && characterResource.WaveIndex > _managementNpcController.LoadManagementStateData().Index)
            {
                descriptionText.text = characterResource.CharacterUnlockDescription;
                descriptionText.gameObject.SetActive(true);
                selectButton.gameObject.SetActive(false);
            }
            else
            {
                descriptionText.gameObject.SetActive(false);
                selectButton.gameObject.SetActive(isUnlocked);
            }

        }

        private bool IsCurrentCharacterActiveInGame()
        {
            return allCharacterResources.CharacterResourcesList[_currentCharacterIndex].CharacterModelAddressableKey ==
                _playerController.CurrentSkinKey;
        }


        private void SelectCharacter(string characterName)
        {
            var characterIndex = allCharacterResources.CharacterResourcesList.FindIndex(x =>
                x.CharacterName.Equals(characterName));

            if (characterIndex == _currentCharacterIndex) return;

            _currentCharacterIndex = characterIndex;
            characterUpgradeInfo.InitializeCharacter(Resolver,
                allCharacterResources.CharacterResourcesList[_currentCharacterIndex],
                () => SetupCharacterUpgrade(true));
            UpdateCharacterPageForCurrentCharacter();
        }

        #endregion


        #region Private Methods

        private void ChooseCurrentCharacter()
        {
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
            var currentCharacterKey = allCharacterResources.CharacterResourcesList[_currentCharacterIndex]
                .CharacterModelAddressableKey;
            SwitchToCharacter(currentCharacterKey);

            var characterSelectionData = new CharacterSelectionData {SelectedCharacterKey = currentCharacterKey};

            SaveLoadHelper.SaveData(characterSelectionData);
            RefreshSegment(currentCharacterKey);
            UpdateCharacterPageForCurrentCharacter();
        }

        private void SetupCharacterUpgrade(bool isAllStats = false)
        {
            if (!IsCurrentCharacterActiveInGame()) return;

            characterUpgrade.gameObject.SetActive(true);
            var characterData = allCharacterResources.CharacterResourcesList[_currentCharacterIndex];
            var upgradeDetails = _characterUpgradeResources.CharacterUpgradeList[_characterMetaUpgradeData.UpgradeIndex]
                .UpgradeDetails;
            characterUpgrade.Initialize(Resolver, characterData, upgradeDetails, ApplySkill, isAllStats);
        }


        private void CreateCharacterSegments()
        {
            foreach (var characterResources in allCharacterResources.CharacterResourcesList)
            {
                _characterSegmentInstance = Instantiate(characterSegmentPrefab, charactersSegmentParent);
                _characterSegmentInstance.InitializeSegment(characterResources,
                    () => SelectCharacter(characterResources.CharacterName),
                    characterResources.CharacterModelAddressableKey == _playerController.CurrentSkinKey);
                _characterSegments.Add(characterResources, _characterSegmentInstance);
            }
        }


        private void ApplySkill()
        {
            var upgrade = _characterUpgradeResources.CharacterUpgradeList[_characterMetaUpgradeData.UpgradeIndex];

            if (!_gameInventoryManager.PurchaseItem(new PurchaseDetails((int) upgrade.Price, PurchaseOptions.Coin)))
            {
                ShowAlert(false);
                return;
            }

            _playerSkillController.ApplyStatUpgrade(upgrade.UpgradeDetails);

            ShowAlert(true);

            _characterMetaUpgradeData.UpgradeIndex++;

            SaveLoadHelper.UpdateData<CharacterMetaUpgradeData>(data =>
            {
                data.UpgradeIndex = _characterMetaUpgradeData.UpgradeIndex;
            });

            Resolver.Resolve<IAnalyticsService>().LogEvent(new EventParameters<string>
            {
                EventName = $"character_upgrade_{_characterMetaUpgradeData.UpgradeIndex}",
                AdjustToken = AdjustNsEventTokens.CharacterUpgrade
            });

            SetupCharacterUpgrade();
        }

        private void RefreshSegment(string currentCharacterKey)
        {
            foreach (var segment in _characterSegments)
                segment.Value.SetState(segment.Key.CharacterModelAddressableKey == currentCharacterKey);
        }

        private void ShowAlert(bool isSuccess)
        {
            var text = isSuccess ? "Upgrade Successful" : "Not enough coins";

            alertText.text = text;
            alertText.color = isSuccess ? Color.green : Color.red;

            if (alertText.TryGetComponent(out Animator animator)) animator.SetTrigger(Show);
        }

        private void SwitchToCharacter(string characterKey)
        {
            if (_playerController)
                _playerController.SetSkin(characterKey);
        }


        private CharacterMetaUpgradeData GetCarMetaUpgradeData()
        {
            return SaveLoadHelper.TryLoadPersistentData<CharacterMetaUpgradeData>();
        }

        private CharacterResources GetCurrentCharacterResource()
        {
            var currentSkinKey = _playerController.CurrentSkinKey;
            _currentCharacterIndex = allCharacterResources.CharacterResourcesList.FindIndex(x =>
                x.CharacterModelAddressableKey.Equals(currentSkinKey));
            return allCharacterResources.CharacterResourcesList.Find(x =>
                x.CharacterModelAddressableKey.Equals(currentSkinKey));
        }

        #endregion
    }
}

#if UNITY_EDITOR

using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using GameCore.Wave;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using System.Collections.Generic;
using GameCore.Health;
using UnityEditor.SceneManagement;
using GameCore.Player.WeaponSystem;
using GameCore.Player.Input;
using MyBox;

public class PerkUpgradeEditor : EditorWindow
{
    private Skill _selectedSkill1;
    private Skill _selectedSkill2;
    private Skill _selectedSkill3;

    private IObjectResolver _resolver;
    private PlayerController _playerController;
    private PlayerStatusController _playerStatusController;
    private PlayerWeaponController _playerWeaponController;
    private PlayerMovementController _playerMovementController;
    private ItemPicker _itemPicker;
    private WaveManager _waveManager;
    private PlayerSkillController _playerSkillController;
    private CarManager _carManager;
    private PopupManager _popupManager;
    private bool _shouldUpdateStats = false;
    private int _carIndex = 0;
    private int _characterIndex = 0;
    private bool _isPlayerRangedWeaponLocked;
    private const string GameScene = "GameScene";
    private readonly Dictionary<string, float> _startPlayerValues = new();
    private readonly Dictionary<string, float> _startCarValues = new();


    [MenuItem("Tools/Perk Upgrade Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<PerkUpgradeEditor>();
        window.titleContent = new GUIContent("Perk Upgrade Editor");
        window.minSize = new Vector2(300, 400);
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.activeSceneChanged += SceneChange;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorSceneManager.activeSceneChanged -= SceneChange;
    }


    private void OnGUI()
    {
        if (_shouldUpdateStats)
        {
            DrawPlayerStats();
            _shouldUpdateStats = false;
        }

        if (!EditorApplication.isPlaying)
        {
            ShowWarning("Please enter Play Mode to use this editor.");
            return;
        }

        if (!IsSceneLoaded(GameScene))
        {
            ShowWarning("Please load the GameScene to use this editor.");
            return;
        }

        InitializeDependencies();


        if (_resolver == null)
        {
            ShowError("LifetimeScope not found.");
            return;
        }

        DrawPlayerStats();
        DrawCarStats();
        DrawSkillSelection();
        DrawPlayerRangedWeaponLockedStatus();
        DrawShowPerkUpgradeButton();
        DrawCarUpgradeButton();
        DrawCharacterUpgradeButton();
        DrawResetSkillsButton();
    }

    public static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            if (scene.name == sceneName && scene.isLoaded)
            {
                return true;
            }
        }

        return false;
    }


    private void InitializeDependencies()
    {
        if (_resolver != null) return;

        var lifetimeScope = Object.FindFirstObjectByType<LifetimeScope>();
        if (lifetimeScope == null) return;

        _resolver = lifetimeScope.Container;
        _popupManager = _resolver.Resolve<PopupManager>();
        _waveManager = _resolver.Resolve<WaveManager>();
        _playerStatusController = _resolver.Resolve<PlayerStatusController>();
        _playerSkillController = _resolver.Resolve<PlayerSkillController>();
        _playerController = _resolver.Resolve<PlayerController>();
        _playerWeaponController = _playerController.WeaponController;
        _playerMovementController = _playerController.GetComponent<PlayerMovementController>();
        _itemPicker = _resolver.Resolve<ItemPicker>();
        _carManager = _resolver.Resolve<CarManager>();
        _carManager.OnCarSpawned += () => SetupCarStartValuesDictionary();
        _playerWeaponController.OnWeaponInitialized += () =>
        {
            _startPlayerValues["Critical Hit Chance"] = _playerWeaponController.CalculateCriticalHitChance();
            _startPlayerValues["Critical Hit Damage"] = _playerWeaponController.CalculateCriticalHitDamage();
            _startPlayerValues["Attack Damage"] = _playerWeaponController.CalculateFireDamage();
            _startPlayerValues["Attack Speed"] = _playerWeaponController.CalculateFireInterval();
        };
        _carIndex = 0;
        _characterIndex = 0;
        SetupPlayerStartValuesDictionary();
        SetupCarStartValuesDictionary();
    }

    private void SetupPlayerStartValuesDictionary()
    {
        _startPlayerValues["Health"] = _playerStatusController.Health;
        _startPlayerValues["Max Health"] = _playerStatusController.MaxHealth;
        _startPlayerValues["Max Armor"] = _playerStatusController.MaxArmor;
        _startPlayerValues["Armor"] = _playerStatusController.Armor;
        _startPlayerValues["Radius"] = _itemPicker.Radius;
        _startPlayerValues["Critical Hit Chance"] = _playerWeaponController.CalculateCriticalHitChance();
        _startPlayerValues["Critical Hit Damage"] = _playerWeaponController.CalculateCriticalHitDamage();
        _startPlayerValues["Attack Damage"] = _playerWeaponController.CalculateFireDamage();
        _startPlayerValues["Attack Speed"] = _playerWeaponController.CalculateFireInterval();
        _startPlayerValues["Melee Attack Speed"] = _playerWeaponController.MeleeWeapon.FireInterval;
        _startPlayerValues["Speed"] = _playerMovementController.MovementSpeed;
        _startPlayerValues["Dodge Chance"] = _playerStatusController.DodgeChance;
    }

    private void SetupCarStartValuesDictionary()
    {
        var car = _carManager.GetAnyCarController();
        if (car == null) return;
        _startCarValues["Health"] = car.CarStatusController.Health;
        _startCarValues["Max Health"] = car.CarStatusController.MaxHealth;
        _startCarValues["Armor"] = car.CarStatusController.CurrentArmor;
        _startCarValues["Max Armor"] = car.CarStatusController.MaxArmor;
        _startCarValues["Speed"] = car.CarMovementController.MoveSpeed;
        _startCarValues["Radius"] = _itemPicker.CarDetectionRadius;
        _startCarValues["Critical Hit Chance"] = car.AutomaticWeapon.CriticalHitChance;
        _startCarValues["Critical Hit Damage"] = car.AutomaticWeapon.CritDamage;
        _startCarValues["Attack Damage"] = car.AutomaticWeapon.Damage;
        _startCarValues["Attack Speed"] = car.AutomaticWeapon.FireInterval;
        _startCarValues["Fuel Cost"] = car.CarStatusController.FuelCost;
        _startCarValues["Collision Damage Multiplier"] = car.CarZombieDetection.CarCollisionDamageMultiplier;
    }

    private void DrawPlayerStats()
    {
        GUILayout.Label("Player Stats", EditorStyles.boldLabel);

        foreach (var stat in _startPlayerValues.Keys)
        {
            float startValue = _startPlayerValues[stat];
            float currentValue = GetCurrentPlayerStatValue(stat);

            string statusEmoji = (currentValue > startValue) ? "🟢" : (currentValue == startValue) ? "🟡" : "🔴";

            GUILayout.Label($"{statusEmoji} {stat}  |  🔹 Start: {startValue}  ➜  🔺 Current: {currentValue}");
        }

        GUILayout.Space(10);
    }

    private void DrawCarStats()
    {
        GUILayout.Label("Car Stats", EditorStyles.boldLabel);

        foreach (var stat in _startCarValues.Keys)
        {
            float startValue = _startCarValues[stat];
            float currentValue = GetCurrentCarStatValue(stat);

            string statusEmoji = (currentValue > startValue) ? "🟢" : (currentValue == startValue) ? "🟡" : "🔴";

            GUILayout.Label($"{statusEmoji} {stat}  |  🔹 Start: {startValue}  ➜  🔺 Current: {currentValue}");
        }

        GUILayout.Space(10);
    }

    private float GetCurrentCarStatValue(string statName)
    {
        var car = _carManager.GetAnyCarController();
        if (car == null) return 0;

        return statName switch
        {
            "Health" => car.CarStatusController.Health,
            "Max Health" => car.CarStatusController.MaxHealth,
            "Max Armor" => car.CarStatusController.MaxArmor,
            "Armor" => car.CarStatusController.CurrentArmor,
            "Speed" => car.CarMovementController.MoveSpeed,
            "Radius" => _itemPicker.CarDetectionRadius,
            "Critical Hit Chance" => car.AutomaticWeapon.CriticalHitChance,
            "Critical Hit Damage" => car.AutomaticWeapon.CritDamage,
            "Attack Damage" => car.AutomaticWeapon.Damage,
            "Attack Speed" => car.AutomaticWeapon.FireInterval,
            "Fuel Cost" => car.CarStatusController.FuelCost,
            "Collision Damage Multiplier" => car.CarZombieDetection.CarCollisionDamageMultiplier,
            _ => 0
        };
    }

    private float GetCurrentPlayerStatValue(string statName)
    {
        return statName switch
        {
            "Health" => _playerStatusController.Health,
            "Max Health" => _playerStatusController.MaxHealth,
            "Max Armor" => _playerStatusController.MaxArmor,
            "Armor" => _playerStatusController.Armor,
            "Speed" => _playerMovementController.MovementSpeed,
            "Critical Hit Chance" => _playerWeaponController.CalculateCriticalHitChance(),
            "Critical Hit Damage" => _playerWeaponController.CalculateCriticalHitDamage(),
            "Attack Damage" => _playerWeaponController.CalculateFireDamage(),
            "Attack Speed" => _playerWeaponController.CalculateFireInterval(),
            "Melee Attack Speed" => _playerWeaponController.MeleeWeapon.FireInterval,
            "Radius" => _itemPicker.Radius,
            "Dodge Chance" => _playerStatusController.DodgeChance,
            _ => 0
        };
    }

    private void DrawSkillSelection()
    {
        GUILayout.Label("Select Skills for Perk Upgrade", EditorStyles.boldLabel);

        _selectedSkill1 = (Skill) EditorGUILayout.ObjectField("Skill 1", _selectedSkill1, typeof(Skill), false);
        _selectedSkill2 = (Skill) EditorGUILayout.ObjectField("Skill 2", _selectedSkill2, typeof(Skill), false);
        _selectedSkill3 = (Skill) EditorGUILayout.ObjectField("Skill 3", _selectedSkill3, typeof(Skill), false);

        GUILayout.Space(10);
    }

    private void DrawShowPerkUpgradeButton()
    {
        if (!GUILayout.Button("Perk Upgrade")) return;

        if (_selectedSkill1 == null || _selectedSkill2 == null || _selectedSkill3 == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select all skills.", "OK");
            return;
        }

        ShowPerkUpgradePopup();
    }

    private async void ShowPerkUpgradePopup()
    {
        if (!IsSceneLoaded(GameScene))
        {
            ShowWarning("Restart the editor after loading the GameScene.");
            return;
        }

        _selectedSkill1.starUpgrades.ForEach(x => x.upgradeDetails.ForEach(y => y.skill = _selectedSkill1));
        _selectedSkill2.starUpgrades.ForEach(x => x.upgradeDetails.ForEach(y => y.skill = _selectedSkill2));
        _selectedSkill3.starUpgrades.ForEach(x => x.upgradeDetails.ForEach(y => y.skill = _selectedSkill3));

        _waveManager.ToggleWave(true);
        await _popupManager.OpenPopup(PopupConstants.PopupType.LevelUp);

        var popup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.LevelUp);
        if (popup == null) return;

        var skillData = new System.Tuple<(Skill skill, int level), (Skill skill, int level), (Skill skill, int level)>(
            (_selectedSkill1, _playerSkillController.GetSkillDetail(_selectedSkill1).StarLevel),
            (_selectedSkill2, _playerSkillController.GetSkillDetail(_selectedSkill2).StarLevel),
            (_selectedSkill3, _playerSkillController.GetSkillDetail(_selectedSkill3).StarLevel));

        popup.Initialize(skillData);
        popup.ClosePopupAction += () =>
        {
            _waveManager.ToggleWave(false);
            _shouldUpdateStats = true;
        };
    }

    private void DrawResetSkillsButton()
    {
        GUILayout.Space(10);

        if (GUILayout.Button("Reset Skills"))
        {
            if (!IsSceneLoaded(GameScene))
            {
                ShowWarning("Restart the editor after loading the GameScene.");
                return;
            }

            _playerSkillController.OnResetSkillInvoke();
            _waveManager.ToggleWave(false);
            _carIndex = 0;
            _characterIndex = 0;
            Debug.Log("Skills reset.");
        }
    }

    private void DrawCarUpgradeButton()
    {
        if (!GUILayout.Button("Car Upgrade")) return;

        var carMetaUpgrade = _playerSkillController.CarMetaUpgradeResources.CarMetaUpgradeList[_carIndex];
        _waveManager.ToggleWave(true);
        _playerSkillController.ApplyStatUpgrade(new List<UpgradeDetail> {carMetaUpgrade.UpgradeDetail});

        _carIndex++;
        _waveManager.ToggleWave(false);
        if (_carIndex >= _playerSkillController.CarMetaUpgradeResources.CarMetaUpgradeList.Count)
        {
            _carIndex = 0;
        }
    }

    private void DrawCharacterUpgradeButton()
    {
        if (!GUILayout.Button("Character Upgrade")) return;
        var characterUpgrade = _playerSkillController.CharacterUpgradeResources.CharacterUpgradeList[_characterIndex];
        _waveManager.ToggleWave(true);

        _playerSkillController.ApplyStatUpgrade(characterUpgrade.UpgradeDetails);

        _characterIndex++;
        _waveManager.ToggleWave(false);
        if (_characterIndex >= _playerSkillController.CharacterUpgradeResources.CharacterUpgradeList.Count)
        {
            _characterIndex = 0;
        }
    }

    private void DrawPlayerRangedWeaponLockedStatus()
    {
        _isPlayerRangedWeaponLocked = GUILayout.Toggle(_isPlayerRangedWeaponLocked, "Player Ranged Weapon Locked");

        UpdateWeaponLockStatus(_isPlayerRangedWeaponLocked);
    }

    private void UpdateWeaponLockStatus(bool isLocked)
    {
        if (_playerWeaponController.Lweapon != null)
            _playerWeaponController.Lweapon.IsLocked = isLocked;

        if (_playerWeaponController.Rweapon != null)
            _playerWeaponController.Rweapon.IsLocked = isLocked;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            _resolver = null;
            _startPlayerValues.Clear();
        }
    }

    private void SceneChange(Scene oldScene, Scene newScene)
    {
        if (newScene.name == GameScene)
        {
            _resolver = null;
            InitializeDependencies();
        }
    }

    private void ShowWarning(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.Warning);
    }

    private void ShowError(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.Error);
    }
}
#endif

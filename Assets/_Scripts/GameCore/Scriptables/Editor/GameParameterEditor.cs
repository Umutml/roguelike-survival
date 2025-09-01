#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.Utilities;
using GameCore.Car;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities;

public class GameParametersEditor : EditorWindow
{
    public GameParameters loadedGameParameters;
    private SerializedObject _serializedGameParameters;
    private GUIStyle _mainContainerStyle;
    private GUIStyle _subContainerStyle;
    private GUIStyle _largeButtonStyle;
    private bool _isPlayerFoldOut;
    private bool _isZombieFoldOut;
    private bool _isCarFoldOut;
    private bool _isSkillFoldOut;
    private bool _isLevelFoldOut;
    private bool _isEnemyTypeFoldOut;
    private bool _isWeaponFoldOut;
    private bool _isWaveFoldOut;
    private bool _isBoxChanceFoldOut;
    private bool _isObjectiveFoldOut;

    [MenuItem("Tools/Game Parameters Editor")]
    public static void ShowWindow()
    {
        GameParametersEditor window = GetWindow<GameParametersEditor>();
        window.minSize = new Vector2(450, 800);
        window.maxSize = new Vector2(450, 800);
    }

    private void OnGUI()
    {
        #region Styles

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 35;
        titleStyle.normal.textColor = Color.red;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniBoldLabel);
        subtitleStyle.fontSize = 20;
        subtitleStyle.normal.textColor = Color.white;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        _mainContainerStyle = new GUIStyle(EditorStyles.foldout);
        _mainContainerStyle.fontSize = 20;
        _mainContainerStyle.fontStyle = FontStyle.Bold;
        _subContainerStyle = new GUIStyle(EditorStyles.foldout);
        _subContainerStyle.fontSize = 15;
        _subContainerStyle.fontStyle = FontStyle.BoldAndItalic;
        _subContainerStyle.margin = new RectOffset(5, 0, 0, 0);

        #endregion

        EditorGUILayout.Space(20);
        GUILayout.Label("Game Parameters Editor", titleStyle);
        EditorGUILayout.Space(5);
        GUILayout.Label("No Surrender", subtitleStyle);
        EditorGUILayout.Space(15);
        if (loadedGameParameters == null)
        {
            EditorGUILayout.HelpBox("No Game Parameters loaded. Please provide a valid hard reference key.",
                MessageType.Info);
            loadedGameParameters = (GameParameters) EditorGUILayout.ObjectField("GameParameters",
                loadedGameParameters,
                typeof(GameParameters),
                false);
            loadedGameParameters =
                AssetDatabase.LoadAssetAtPath<GameParameters>(
                    "Assets/_Scripts/Scriptable_Objects/GameParameters.asset");
            if (loadedGameParameters != null)
            {
                LoggerNS.Log("GameParameters loaded successfully!");
            }
            else
            {
                LoggerNS.LogError("Failed to load GameParameters.");
            }

            return;
        }

        _serializedGameParameters ??= new SerializedObject(loadedGameParameters);
        _serializedGameParameters.Update();
        DrawPlayerParameters();
        EditorGUILayout.Space(5);
        DrawZombieParameters();
        EditorGUILayout.Space(5);
        DrawCarParameters();
        EditorGUILayout.Space(5);
        DrawSkillParameters();
        EditorGUILayout.Space(5);
        DrawLevelParameters();
        EditorGUILayout.Space(5);
        DrawEnemyTypeParameters();
        EditorGUILayout.Space(5);
        DrawWeaponList();
        EditorGUILayout.Space(5);
        DrawBoxChance();
        EditorGUILayout.Space(5);
        DrawWaveList();
        EditorGUILayout.Space(5);
        DrawObjectiveParameters();
        EditorGUILayout.Space(15);
        _largeButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            normal = {textColor = Color.green},
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(20, 20, 10, 10),
            fontStyle = FontStyle.Bold,
            fixedHeight = 40,
            fixedWidth = 220,
            margin = new RectOffset(112, 112, 0, 0)
        };
        if (GUILayout.Button("Save Changes", _largeButtonStyle))
        {
            SaveChanges();
        }

        if (GUILayout.Button("Save Parameter", _largeButtonStyle))
        {
            loadedGameParameters.SaveParameters();
        }
    }

    #region Player

    private void DrawPlayerParameters()
    {
        _isPlayerFoldOut = EditorGUILayout.Foldout(_isPlayerFoldOut, "Player", true, _mainContainerStyle);
        if (!_isPlayerFoldOut) return;
        SerializedObject playerStatusController = new SerializedObject(loadedGameParameters.PlayerStatusController);
        EditorGUILayout.PropertyField(playerStatusController.FindProperty("maxHealth"), new GUIContent("Max Health"));
        EditorGUILayout.PropertyField(playerStatusController.FindProperty("maxArmor"), new GUIContent("Max Armor"));
        SerializedObject playerController = new SerializedObject(loadedGameParameters.PlayerController);
        if (playerController.FindProperty("findTargetCooldown") != null)
            EditorGUILayout.PropertyField(playerController.FindProperty("findTargetCooldown"),
                new GUIContent("Target Cooldown"));
        SerializedObject playerMovementController = new SerializedObject(loadedGameParameters.PlayerMovement);
        EditorGUILayout.PropertyField(playerMovementController.FindProperty("movementSpeed"),
            new GUIContent("Movement Speed"));
        EditorGUILayout.PropertyField(playerMovementController.FindProperty("movementRotationAngle"),
            new GUIContent("Rotation Angle"));

        SerializedObject playerItemPicker = new SerializedObject(loadedGameParameters.PlayerItemPicker);
        EditorGUILayout.PropertyField(playerItemPicker.FindProperty("radius"), new GUIContent("Detection Range"));
        EditorGUILayout.PropertyField(playerItemPicker.FindProperty("carDetectionRadius"),
            new GUIContent("Car Detection Range"));
        playerStatusController.ApplyModifiedProperties();
        playerController.ApplyModifiedProperties();
        playerMovementController.ApplyModifiedProperties();
        playerItemPicker.ApplyModifiedProperties();
        EditorUtility.SetDirty(loadedGameParameters);
    }

    #endregion

    #region zombie

    private void DrawZombieParameters()
    {
        _isZombieFoldOut = EditorGUILayout.Foldout(_isZombieFoldOut, "Zombie", true, _mainContainerStyle);
        if (!_isZombieFoldOut) return;
        loadedGameParameters.FreeRoamZombieCount =
            EditorGUILayout.IntField("Free Roam Zombie Count", loadedGameParameters.FreeRoamZombieCount);
        loadedGameParameters.ZombieAttackRange =
            EditorGUILayout.FloatField("Attack Range", loadedGameParameters.ZombieAttackRange);
        loadedGameParameters.ZombiePatrolDetectionRadius = EditorGUILayout.FloatField("Patrol Detection Range",
            loadedGameParameters.ZombiePatrolDetectionRadius);
        loadedGameParameters.ZombieWaitingDetectionRadius = EditorGUILayout.FloatField("Waiting Detection Range",
            loadedGameParameters.ZombieWaitingDetectionRadius);
        loadedGameParameters.ZombieAttackCooldown =
            EditorGUILayout.FloatField("Attack Cooldown", loadedGameParameters.ZombieAttackCooldown);
        loadedGameParameters.ZombieRagdollDuration =
            EditorGUILayout.IntField("Ragdoll Duration", loadedGameParameters.ZombieRagdollDuration);
        loadedGameParameters.ZombieCrashDeadProbability = EditorGUILayout.FloatField("Crash Dead Probability",
            loadedGameParameters.ZombieCrashDeadProbability);
        EditorUtility.SetDirty(loadedGameParameters);
    }

    #endregion

    #region Car

    private readonly List<bool> _carFoldOutState = new();
    private Vector2 _carScrollPosition;

    private void DrawCarParameters()
    {
        if (loadedGameParameters.CarResources != null)
        {
            _isCarFoldOut = EditorGUILayout.Foldout(_isCarFoldOut, "Cars", true, _mainContainerStyle);
            if (!_isCarFoldOut) return;
            _carScrollPosition = EditorGUILayout.BeginScrollView(_carScrollPosition, GUILayout.Height(0));
            SerializedObject carResourcesSerialized = new SerializedObject(loadedGameParameters.CarResources);
            SerializedProperty carListProperty = carResourcesSerialized.FindProperty("carList");
            if (carListProperty != null)
            {
                if (_carFoldOutState.Count != carListProperty.arraySize)
                {
                    _carFoldOutState.Clear();
                    for (int i = 0; i < carListProperty.arraySize; i++)
                    {
                        _carFoldOutState.Add(false);
                    }
                }

                for (int i = 0; i < carListProperty.arraySize; i++)
                {
                    SerializedProperty carProperty = carListProperty.GetArrayElementAtIndex(i);
                    SerializedProperty carTypeProperty = carProperty.FindPropertyRelative("carType");
                    string carTypeName = carTypeProperty.enumNames[carTypeProperty.enumValueIndex];
                    _carFoldOutState[i] =
                        EditorGUILayout.Foldout(_carFoldOutState[i], carTypeName, true, _subContainerStyle);
                    if (_carFoldOutState[i])
                    {
                        EditorGUILayout.BeginVertical("box");
                        GUILayout.Label(carTypeName, EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("maxHealt"),
                            new GUIContent("Max Health"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("moveSpeed"),
                            new GUIContent("Move Speed"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("maxSpeed"),
                            new GUIContent("Max Speed"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("drag"), new GUIContent("Drag"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("steerAngle"),
                            new GUIContent("Steer Angle"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("traction"),
                            new GUIContent("Traction"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("driftMultiplier"),
                            new GUIContent("Drift Multiplier"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("driftSpeedMultiplier"),
                            new GUIContent("Drift Speed Multiplier"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("driftOffset"),
                            new GUIContent("Drift Offset"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("tiltAmount"),
                            new GUIContent("Tilt Amount"));
                        EditorGUILayout.PropertyField(carProperty.FindPropertyRelative("liftAmount"),
                            new GUIContent("Lift Amount"));

                        if (GUILayout.Button("Remove Car"))
                        {
                            carListProperty.DeleteArrayElementAtIndex(i);
                            break;
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                carResourcesSerialized.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    #endregion

    #region Skill

    private readonly List<bool> _skillFoldOutState = new();
    private Vector2 _skillScrollPosition;

    private void DrawSkillParameters()
    {
        if (loadedGameParameters.SkillData != null)
        {
            _isSkillFoldOut = EditorGUILayout.Foldout(_isSkillFoldOut, "Skills", true, _mainContainerStyle);
            if (!_isSkillFoldOut) return;

            _skillScrollPosition = EditorGUILayout.BeginScrollView(_skillScrollPosition, GUILayout.Height(300));

            if (_skillFoldOutState.Count != loadedGameParameters.SkillData.Skills.Count)
            {
                _skillFoldOutState.Clear();
                for (int i = 0; i < loadedGameParameters.SkillData.Skills.Count; i++)
                {
                    _skillFoldOutState.Add(false);
                }
            }

            for (var i = 0; i < loadedGameParameters.SkillData.Skills.Count; i++)
            {
                var skill = loadedGameParameters.SkillData.Skills[i];
                SerializedObject skillDataSerialized = new SerializedObject(skill);

                _skillFoldOutState[i] =
                    EditorGUILayout.Foldout(_skillFoldOutState[i], skill.name, true, _subContainerStyle);
                if (_skillFoldOutState[i])
                {
                    EditorGUILayout.BeginVertical("box");
                    GUILayout.Label(skill.name, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("name"),
                        new GUIContent("Skill Name"));
                    EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("rarity"), new GUIContent("Rarity"));
                    EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("icon"), new GUIContent("Icon"));
                    EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("upgradeType"),
                        new GUIContent("Upgrade Type"));

                    var triggerTypeProperty = skillDataSerialized.FindProperty("triggerType");
                    EditorGUILayout.PropertyField(triggerTypeProperty, new GUIContent("Trigger Type"));

                    if ((TriggerType) triggerTypeProperty.enumValueIndex == TriggerType.EventBased)
                    {
                        EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("eventTriggerCondition"),
                            new GUIContent("Event Trigger Condition"));
                    }
                    else if ((TriggerType) triggerTypeProperty.enumValueIndex == TriggerType.TimeBased)
                    {
                        EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("timeBasedCondition"),
                            new GUIContent("Time Based Condition"));
                    }

                    SerializedProperty starUpgradesProperty = skillDataSerialized.FindProperty("starUpgrades");
                    EditorGUILayout.PropertyField(starUpgradesProperty, new GUIContent("Star Upgrades"), true);

                    EditorGUILayout.PropertyField(skillDataSerialized.FindProperty("skillEventEffect"),
                        new GUIContent("Skill Event Effect"));

                    if (GUILayout.Button("Remove Skill"))
                    {
                        loadedGameParameters.SkillData.Skills.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndVertical();
                }

                skillDataSerialized.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    #endregion

    #region Level

    private readonly List<bool> _levelFoldOutState = new();
    private Vector2 _levelScrollPosition;

    private void DrawLevelParameters()
    {
        if (loadedGameParameters.waveLevelData != null)
        {
            _isLevelFoldOut = EditorGUILayout.Foldout(_isLevelFoldOut, "Levels", true, _mainContainerStyle);
            if (!_isLevelFoldOut) return;
            _levelScrollPosition = EditorGUILayout.BeginScrollView(_levelScrollPosition, GUILayout.Height(0));

            if (_levelFoldOutState.Count != loadedGameParameters.waveLevelData.levels.Length)
            {
                _levelFoldOutState.Clear();
                for (int i = 0; i < loadedGameParameters.waveLevelData.levels.Length; i++)
                {
                    _levelFoldOutState.Add(false);
                }
            }

            for (int i = 0; i < loadedGameParameters.waveLevelData.levels.Length; i++)
            {
                var level = loadedGameParameters.waveLevelData.levels[i];
                SerializedObject
                    levelDataSerialized =
                        new SerializedObject(loadedGameParameters.waveLevelData); // LevelData nesnesine serialize et
                _levelFoldOutState[i] =
                    EditorGUILayout.Foldout(_levelFoldOutState[i], level.name, true, _subContainerStyle);
                if (_levelFoldOutState[i])
                {
                    EditorGUILayout.BeginVertical("box");
                    GUILayout.Label(level.name, EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(
                        levelDataSerialized.FindProperty("levels").GetArrayElementAtIndex(i)
                            .FindPropertyRelative("name"),
                        new GUIContent("Level Name"));
                    EditorGUILayout.PropertyField(
                        levelDataSerialized.FindProperty("levels").GetArrayElementAtIndex(i)
                            .FindPropertyRelative("level"),
                        new GUIContent("Level"));
                    EditorGUILayout.PropertyField(
                        levelDataSerialized.FindProperty("levels").GetArrayElementAtIndex(i)
                            .FindPropertyRelative("expPodToUnlock"),
                        new GUIContent("Exp to Unlock"));

                    if (GUILayout.Button("Remove Level"))
                    {
                        var levels = loadedGameParameters.waveLevelData.levels;
                        var levelList = new List<LevelDetails>(levels);
                        levelList.RemoveAt(i);
                        loadedGameParameters.waveLevelData.levels = levelList.ToArray();
                        break;
                    }

                    EditorGUILayout.EndVertical();
                }

                levelDataSerialized.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    #endregion

    #region EnemyType

    private readonly List<bool> _enemyFoldOutState = new List<bool>();
    private Vector2 _enemyScrollPosition;

    private void DrawEnemyTypeParameters()
    {
        _isEnemyTypeFoldOut = EditorGUILayout.Foldout(_isEnemyTypeFoldOut, "Enemy Types", true, _mainContainerStyle);
        if (!_isEnemyTypeFoldOut) return;
        _enemyScrollPosition = EditorGUILayout.BeginScrollView(_enemyScrollPosition, GUILayout.Height(0));
        if (_enemyFoldOutState.Count != loadedGameParameters.EnemyTypeData.enemyTypes.Count +
            loadedGameParameters.EnemyTypeData.tutorialEnemyTypes.Count)
        {
            _enemyFoldOutState.Clear();
            for (int i = 0;
                i < loadedGameParameters.EnemyTypeData.enemyTypes.Count +
                loadedGameParameters.EnemyTypeData.tutorialEnemyTypes.Count;
                i++)
            {
                _enemyFoldOutState.Add(false);
            }
        }

        for (int i = 0; i < loadedGameParameters.EnemyTypeData.enemyTypes.Count; i++)
        {
            var enemy = loadedGameParameters.EnemyTypeData.enemyTypes[i];
            SerializedObject enemySerialized = new SerializedObject(enemy.EnemyType);
            _enemyFoldOutState[i] =
                EditorGUILayout.Foldout(_enemyFoldOutState[i], enemy.EnemyType.name, true, _subContainerStyle);
            if (_enemyFoldOutState[i])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("name"), new GUIContent("Enemy Name"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("category"), new GUIContent("Category"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("health"), new GUIContent("Health"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("movementSpeed"),
                    new GUIContent("Movement Speed"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackDamage"),
                    new GUIContent("Attack Damage"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackRange"),
                    new GUIContent("Attack Range"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackSpeed"),
                    new GUIContent("Attack Speed"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("detectionRadius"),
                    new GUIContent("Detection Radius"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("baseXpDropValue"),
                    new GUIContent("Base XP Drop Value"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("xpDropValue"),
                    new GUIContent("XP Drop Value"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("xpDropChance"),
                    new GUIContent("XP Drop Chance"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("softCurrencyChance"),
                    new GUIContent("Soft Currency Chance"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("minSoftCurrencyInWave"),
                    new GUIContent("Min Soft Currency In Wave"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("maxSoftCurrencyInWave"),
                    new GUIContent("Max Soft Currency In Wave"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("minSoftCurrencyInFreeRoam"),
                    new GUIContent("Min Soft Currency In Free Roam"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("maxSoftCurrencyInFreeRoam"),
                    new GUIContent("Max Soft Currency In Free Roam"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("largeHordeCount"),
                    new GUIContent("Large Horde Count"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("mediumHordeCount"),
                    new GUIContent("Medium Horde Count"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("smallHordeCount"),
                    new GUIContent("Small Horde Count"));

                if (GUILayout.Button("Remove Enemy"))
                {
                    loadedGameParameters.EnemyTypeData.enemyTypes.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            enemySerialized.ApplyModifiedProperties();
        }

        EditorGUILayout.LabelField("Tutorial Enemy Types", EditorStyles.boldLabel);
        for (int i = 0; i < loadedGameParameters.EnemyTypeData.tutorialEnemyTypes.Count; i++)
        {
            var enemy = loadedGameParameters.EnemyTypeData.tutorialEnemyTypes[i];
            SerializedObject enemySerialized = new SerializedObject(enemy);

            _enemyFoldOutState[i + loadedGameParameters.EnemyTypeData.enemyTypes.Count] = EditorGUILayout.Foldout(
                _enemyFoldOutState[i + loadedGameParameters.EnemyTypeData.enemyTypes.Count],
                enemy.name,
                true,
                _subContainerStyle);

            if (_enemyFoldOutState[i + loadedGameParameters.EnemyTypeData.enemyTypes.Count])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("name"), new GUIContent("Enemy Name"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("category"), new GUIContent("Category"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("health"), new GUIContent("Health"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("movementSpeed"),
                    new GUIContent("Movement Speed"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackDamage"),
                    new GUIContent("Attack Damage"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackRange"),
                    new GUIContent("Attack Range"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("attackSpeed"),
                    new GUIContent("Attack Speed"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("detectionRadius"),
                    new GUIContent("Detection Radius"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("baseXpDropValue"),
                    new GUIContent("Base XP Drop Value"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("xpDropValue"),
                    new GUIContent("XP Drop Value"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("xpDropChance"),
                    new GUIContent("XP Drop Chance"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("largeHordeCount"),
                    new GUIContent("Large Horde Count"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("mediumHordeCount"),
                    new GUIContent("Medium Horde Count"));
                EditorGUILayout.PropertyField(enemySerialized.FindProperty("smallHordeCount"),
                    new GUIContent("Small Horde Count"));

                if (GUILayout.Button("Remove Tutorial Enemy"))
                {
                    loadedGameParameters.EnemyTypeData.tutorialEnemyTypes.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            enemySerialized.ApplyModifiedProperties();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Weapons

    private readonly List<bool> _weaponsFoldOutState = new();
    private Vector2 _weaponsScrollPosition;

    private void DrawWeaponList()
    {
        _isWeaponFoldOut = EditorGUILayout.Foldout(_isWeaponFoldOut, "Weapons", true, _mainContainerStyle);
        if (!_isWeaponFoldOut) return;
        _weaponsScrollPosition = EditorGUILayout.BeginScrollView(_weaponsScrollPosition, GUILayout.Height(0));
        if (_weaponsFoldOutState.Count != loadedGameParameters.Weapons.Count)
        {
            _weaponsFoldOutState.Clear();
            for (int i = 0; i < loadedGameParameters.Weapons.Count; i++)
            {
                _weaponsFoldOutState.Add(false);
            }
        }

        for (int i = 0; i < loadedGameParameters.Weapons.Count; i++)
        {
            var weapon = loadedGameParameters.Weapons[i];
            SerializedObject weaponSerialized = new SerializedObject(weapon);
            _weaponsFoldOutState[i] =
                EditorGUILayout.Foldout(_weaponsFoldOutState[i], weapon.name, true, _subContainerStyle);

            if (_weaponsFoldOutState[i])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(weaponSerialized.FindProperty("typeOfWeapon"),
                    new GUIContent("Weapon Type"));
                EditorGUILayout.PropertyField(weaponSerialized.FindProperty("damage"), new GUIContent("Damage"));
                EditorGUILayout.PropertyField(weaponSerialized.FindProperty("fireInterval"),
                    new GUIContent("Fire Interval"));
                EditorGUILayout.PropertyField(weaponSerialized.FindProperty("range"), new GUIContent("Range"));
                if (GUILayout.Button("Remove Weapon"))
                {
                    loadedGameParameters.Weapons.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            weaponSerialized.ApplyModifiedProperties();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Wave

    private readonly List<bool> _waveFoldOutState = new();
    private Vector2 _waveScrollPosition;

    private void DrawWaveList()
    {
        _isWaveFoldOut = EditorGUILayout.Foldout(_isWaveFoldOut, "Waves", true, _mainContainerStyle);
        if (!_isWaveFoldOut) return;
        _waveScrollPosition = EditorGUILayout.BeginScrollView(_waveScrollPosition, GUILayout.Height(0));
        if (_waveFoldOutState.Count != loadedGameParameters.WaveData.waves.Length)
        {
            _waveFoldOutState.Clear();
            for (int i = 0; i < loadedGameParameters.WaveData.waves.Length; i++)
            {
                _waveFoldOutState.Add(false);
            }
        }

        for (int i = 0; i < loadedGameParameters.WaveData.waves.Length; i++)
        {
            var wave = loadedGameParameters.WaveData.waves[i];
            _waveFoldOutState[i] = EditorGUILayout.Foldout(_waveFoldOutState[i], wave.name, true, _subContainerStyle);

            if (_waveFoldOutState[i])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.IntField("Wave Level", wave.level);
                EditorGUILayout.IntField("Wave Duration", wave.duration);
                EditorGUILayout.IntField("Wave Large Horde", wave.large);
                EditorGUILayout.IntField("Wave Medium Horde", wave.medium);
                EditorGUILayout.IntField("Wave Small Horde", wave.small);

                if (GUILayout.Button("Remove Wave"))
                {
                    var waves = loadedGameParameters.WaveData.waves.ToList();
                    waves.RemoveAt(i);
                    loadedGameParameters.WaveData.waves = waves.ToArray();
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorUtility.SetDirty(loadedGameParameters.WaveData);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Box Chance

    private readonly List<bool> _boxChanceFoldOutState = new();
    private Vector2 _boxChanceScrollPosition;

    private void DrawBoxChance()
    {
        _isBoxChanceFoldOut = EditorGUILayout.Foldout(_isBoxChanceFoldOut, "Box Chance", true, _mainContainerStyle);
        if (!_isBoxChanceFoldOut) return;
        _boxChanceScrollPosition = EditorGUILayout.BeginScrollView(_boxChanceScrollPosition, GUILayout.Height(0));
        if (_boxChanceFoldOutState.Count != loadedGameParameters.BoxDropChanceData.dropChances.Count)
        {
            _boxChanceFoldOutState.Clear();
            for (var i = 0; i < loadedGameParameters.BoxDropChanceData.dropChances.Count; i++)
            {
                _boxChanceFoldOutState.Add(false);
            }
        }

        EditorGUILayout.IntField("Box Between Distance", loadedGameParameters.BoxDropChanceData.boxBetweenDistance);
        EditorGUILayout.IntField("Box Count", loadedGameParameters.BoxDropChanceData.boxCount);
        EditorGUILayout.FloatField("Box Spawn Radius", loadedGameParameters.BoxDropChanceData.boxSpawnRadius);

        for (var i = 0; i < loadedGameParameters.BoxDropChanceData.dropChances.Count; i++)
        {
            var dropChance = loadedGameParameters.BoxDropChanceData.dropChances[i];
            _boxChanceFoldOutState[i] = EditorGUILayout.Foldout(_boxChanceFoldOutState[i],
                dropChance.dropPodType.ToString(),
                true,
                _subContainerStyle);

            if (_boxChanceFoldOutState[i])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.IntField("Probability", dropChance.probability);
                EditorGUILayout.Toggle("Is Wave Only", dropChance.isWaveOnly);
                EditorGUILayout.Toggle("Can Increment Drop", dropChance.canIncrementDrop);
                EditorGUILayout.Toggle("Has Value", dropChance.hasValue);
                if (dropChance.hasValue)
                {
                    EditorGUILayout.IntField("Min Value", dropChance.minValue);
                    EditorGUILayout.IntField("Max Value", dropChance.maxValue);
                }

                if (GUILayout.Button("Remove Drop Chance"))
                {
                    var chances = loadedGameParameters.BoxDropChanceData.dropChances.ToList();
                    chances.RemoveAt(i);
                    loadedGameParameters.BoxDropChanceData.dropChances = chances.ToList();
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorUtility.SetDirty(loadedGameParameters.BoxDropChanceData);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region Objectives

    private readonly List<bool> _objectiveFoldOutState = new List<bool>();
    private bool _objectiveWavesFoldOutState;
    private bool _objectiveMobBehaviourFoldOutState;
    private bool _objectiveNpcFoldOutState;
    private bool _objectiveDamageableFoldOutState;


    private Vector2 _objectiveScrollPosition;

    private void DrawObjectiveParameters()
    {
        _isObjectiveFoldOut = EditorGUILayout.Foldout(_isObjectiveFoldOut, "Objectives", true, _mainContainerStyle);
        if (!_isObjectiveFoldOut) return;
        _objectiveScrollPosition = EditorGUILayout.BeginScrollView(_objectiveScrollPosition, GUILayout.Height(0));
        if (_objectiveFoldOutState.Count != loadedGameParameters.Objectives.Length)
        {
            _objectiveFoldOutState.Clear();
            for (int i = 0; i < loadedGameParameters.Objectives.Length; i++)
            {
                _objectiveFoldOutState.Add(false);
            }
        }

        for (int i = 0; i < loadedGameParameters.Objectives.Length; i++)
        {
            var objective = loadedGameParameters.Objectives[i];
            _objectiveFoldOutState[i] = EditorGUILayout.Foldout(_objectiveFoldOutState[i],
                objective.gameObject.name,
                true,
                _subContainerStyle);
            if (_objectiveFoldOutState[i])
            {
                SerializedObject objectiveSerialized = new SerializedObject(objective);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(objectiveSerialized.FindProperty("timeCondition"),
                    new GUIContent("Time Condition"));
                EditorGUILayout.PropertyField(objectiveSerialized.FindProperty("mobCountCondition"),
                    new GUIContent("Zombie Count Condition"));
                EditorGUILayout.PropertyField(objectiveSerialized.FindProperty("upgradeType"),
                    new GUIContent("Upgrade Type"));
                SerializedProperty allObjectiveEvents = objectiveSerialized.FindProperty("allObjectiveEvents");
                SerializedProperty objectiveWaves = objectiveSerialized.FindProperty("objectiveWaves");
                SerializedProperty objectiveNpcs = objectiveSerialized.FindProperty("objectiveNpcs");

                _objectiveMobBehaviourFoldOutState = EditorGUILayout.Foldout(_objectiveMobBehaviourFoldOutState,
                    "Mob Behaviours",
                    true,
                    _subContainerStyle);
                if (_objectiveMobBehaviourFoldOutState)
                {
                    SerializedProperty updateEvents = allObjectiveEvents?.FindPropertyRelative("objectiveEvents");
                    if (updateEvents is {isArray: true})
                    {
                        for (int j = 0; j < updateEvents.arraySize; j++)
                        {
                            SerializedProperty eventElement = updateEvents.GetArrayElementAtIndex(j);
                            GUILayout.Label($"Event {j}", EditorStyles.boldLabel);
                            SerializedProperty eventMobBehaviour = eventElement.FindPropertyRelative("mobBehaviours");
                            if (eventMobBehaviour is not {isArray: true}) continue;
                            EditorGUILayout.PropertyField(eventElement.FindPropertyRelative("isRepeatable"),
                                new GUIContent("Is Repeatable"));
                            EditorGUILayout.PropertyField(eventElement.FindPropertyRelative("targetTime"),
                                new GUIContent("Target Time"));
                            EditorGUILayout.PropertyField(eventElement.FindPropertyRelative("delay"),
                                new GUIContent("Delay"));
                            for (int k = 0; k < eventMobBehaviour.arraySize; k++)
                            {
                                SerializedProperty mobBehaviourElement = eventMobBehaviour.GetArrayElementAtIndex(k);
                                EditorGUILayout.PropertyField(mobBehaviourElement.FindPropertyRelative("spawnCount"),
                                    new GUIContent("Spawn Count"));
                                var enemyDifficulty = mobBehaviourElement.FindPropertyRelative("enemyDifficulty");
                                if (enemyDifficulty != null)
                                {
                                    EditorGUILayout.PropertyField(
                                        enemyDifficulty.FindPropertyRelative("healthDifficulty"),
                                        new GUIContent("Health"));
                                    EditorGUILayout.PropertyField(enemyDifficulty.FindPropertyRelative("attackSpeed"),
                                        new GUIContent("Attack Speed"));
                                    EditorGUILayout.PropertyField(enemyDifficulty.FindPropertyRelative("attackDamage"),
                                        new GUIContent("Attack Damage"));
                                }
                            }

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Add Wave", GUILayout.Width(175)))
                            {
                                eventMobBehaviour.arraySize++;
                            }

                            if (eventMobBehaviour.arraySize > 0 &&
                                GUILayout.Button("Remove Wave", GUILayout.Width(175)))
                            {
                                eventMobBehaviour.arraySize--;
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                _objectiveWavesFoldOutState =
                    EditorGUILayout.Foldout(_objectiveWavesFoldOutState, "Waves", true, _subContainerStyle);
                if (_objectiveWavesFoldOutState)
                {
                    if (objectiveWaves is {isArray: true})
                    {
                        for (int j = 0; j < objectiveWaves.arraySize; j++)
                        {
                            SerializedProperty waveElement = objectiveWaves.GetArrayElementAtIndex(j);
                            EditorGUILayout.PropertyField(waveElement.FindPropertyRelative("mobCount"),
                                new GUIContent("Mob Count"));
                            EditorGUILayout.PropertyField(waveElement.FindPropertyRelative("spawnDuration"),
                                new GUIContent("Duration"));
                            var enemyDifficulty = waveElement.FindPropertyRelative("enemyDifficulty");
                            if (enemyDifficulty != null)
                            {
                                EditorGUILayout.PropertyField(enemyDifficulty.FindPropertyRelative("healthDifficulty"),
                                    new GUIContent("Health"));
                                EditorGUILayout.PropertyField(enemyDifficulty.FindPropertyRelative("attackSpeed"),
                                    new GUIContent("Attack Speed"));
                                EditorGUILayout.PropertyField(enemyDifficulty.FindPropertyRelative("attackDamage"),
                                    new GUIContent("Attack Damage"));
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Add Wave", GUILayout.Width(175)))
                        {
                            objectiveWaves.arraySize++;
                        }

                        if (objectiveWaves.arraySize > 1 && GUILayout.Button("Remove Wave", GUILayout.Width(175)))
                        {
                            objectiveWaves.arraySize--;
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }

                _objectiveNpcFoldOutState =
                    EditorGUILayout.Foldout(_objectiveNpcFoldOutState, "Npcs", true, _subContainerStyle);
                if (_objectiveNpcFoldOutState)
                {
                    if (objectiveNpcs is {isArray: true})
                    {
                        for (int j = 0; j < objectiveNpcs.arraySize; j++)
                        {
                            GUILayout.Label($"Npc {j}", EditorStyles.boldLabel);
                            SerializedProperty npcElement = objectiveNpcs.GetArrayElementAtIndex(j);
                            SerializedProperty npcStats = npcElement.FindPropertyRelative("npcStats");
                            if (npcStats == null) continue;
                            EditorGUILayout.PropertyField(npcStats.FindPropertyRelative("health"),
                                new GUIContent("Health"));
                            EditorGUILayout.PropertyField(npcStats.FindPropertyRelative("damage"),
                                new GUIContent("Damage"));
                            EditorGUILayout.PropertyField(npcStats.FindPropertyRelative("attackRate"),
                                new GUIContent("Attack Rate"));
                            EditorGUILayout.PropertyField(npcStats.FindPropertyRelative("attackRange"),
                                new GUIContent("Attack Range"));
                            EditorGUILayout.PropertyField(npcStats.FindPropertyRelative("npcType"),
                                new GUIContent("Npc Type"));
                        }
                    }
                }

                _objectiveDamageableFoldOutState = EditorGUILayout.Foldout(_objectiveDamageableFoldOutState,
                    "Damageables",
                    true,
                    _subContainerStyle);
                if (_objectiveDamageableFoldOutState)
                {
                    SerializedProperty damageables = objectiveSerialized.FindProperty("damageables");
                    SerializedProperty objectiveDamageables = damageables?.FindPropertyRelative("objectiveDamageable");
                    SerializedProperty otherDamageables = damageables?.FindPropertyRelative("damageableChunks");

                    if (objectiveDamageables is {isArray: true})
                    {
                        for (int j = 0; j < objectiveDamageables.arraySize; j++)
                        {
                            SerializedProperty damageableElement = objectiveDamageables.GetArrayElementAtIndex(j);
                            Object damageableObject = damageableElement.objectReferenceValue;
                            if (damageableObject != null)
                            {
                                var damageableComponent = damageableObject as ObjectiveDamageable;
                                if (damageableComponent != null)
                                {
                                    GUILayout.Label(damageableComponent.name, EditorStyles.boldLabel);
                                    damageableComponent.maxHealth =
                                        EditorGUILayout.FloatField("Health", damageableComponent.maxHealth);
                                }
                            }
                        }
                    }

                    if (otherDamageables is {isArray: true})
                    {
                        for (int j = 0; j < otherDamageables.arraySize; j++)
                        {
                            SerializedProperty damageableElement = otherDamageables.GetArrayElementAtIndex(j);
                            Object damageableObject = damageableElement.objectReferenceValue;
                            if (damageableObject != null)
                            {
                                var damageableComponent = damageableObject as ObjectiveDamageable;
                                if (damageableComponent != null)
                                {
                                    GUILayout.Label(damageableComponent.name, EditorStyles.boldLabel);
                                    damageableComponent.maxHealth =
                                        EditorGUILayout.FloatField("Health", damageableComponent.maxHealth);
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical();
                objectiveSerialized.ApplyModifiedProperties();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    private void SaveChanges()
    {
        if (loadedGameParameters == null)
        {
            LoggerNS.LogError("No Game Parameters loaded to save changes.");
            return;
        }

        EditorUtility.SetDirty(loadedGameParameters);
        AssetDatabase.SaveAssets();
        LoggerNS.Log("Changes saved successfully!");
    }
}

#endif

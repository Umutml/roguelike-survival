using System;
using _Scripts.GameCore.NPC;
using _Scripts.GameCore.Player;
using _Scripts.Utilities;
using _Utilities;
using Addler.Runtime.Core.LifetimeBinding;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Player.Input;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using UnityEngine.Rendering.Universal;
using Utilities;
using VContainer.Unity;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;

namespace GameCore.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region  Actions
        public event Action<string> OnSkinChanged;
        public event Action<bool> OnTravelButtonStatusChanged;
        #endregion

        #region Serializable Fields

        [SerializeField] private bool debug;
        [SerializeField] private string defaultSkinKey;
        [SerializeField] private Transform skinParent;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private Transform lHandTarget;
        [SerializeField] private Transform rHandTarget;
        [SerializeField] private Transform lHandBendGoal;
        [SerializeField] private Transform rHandBendGoal;
        [SerializeField] private PlayerStartPoint[] playerStartPoints;
        [SerializeField] private List<string> weaponKeys;
        [SerializeField] private float maxBackAnglePerArm = 90f;
        [SerializeField] private float maxFrontAnglePerArm = 170f;
        [SerializeField] private GameObject rangeIndicator;
        [SerializeField] private GameObject blobShadow;
        [SerializeField] private PlayerBaseDistanceChecker playerBaseDistanceChecker;
        [SerializeField] private BoxManager boxManager;
        [SerializeField] private ManagementNpcController managementNpcController;
        [SerializeField] private DecalProjector mainWeaponDecalProjector;


        public PlayerSpeechBubble playerSpeechBubble;

        #endregion

        #region Fields

        private IAnalyticsService _analyticsService;
        private Transform _centerOfBase = null;
        private Transform _centerOfGarage = null;


        private IDamageableRegisterService _damageableRegisterService;
        private DamageNumberManager _damageNumberManager;


        private float _fireTimerL, _fireTimerR;


        private ItemPicker _itemPicker;


        private MeleeWeapon _meleeWeapon;
        private MobManager _mobManager;
        private PlayerAnimationController _playerAnimationController;
        private PlayerCarController _playerCarController;
        private PlayerMovementController _playerMovementController;

        private PlayerMovementMode _playerMovementMode;
        private PlayerSkillController _playerSkillController;
        private PlayerStatusController _playerStatusController;
        private PlayerTargetingController _playerTargetingController;
        private PlayerWeaponController _playerWeaponController;
        private VibrationManager _vibrationManager;
        private WaveManager _waveManager;
        private AudioManager _audioManager;
        private IObjectResolver _resolver;
        private bool _skinLoaded;
        private TutorialSequenceController _tutorialService;

        #endregion

        #region Properties

        public List<string> StartWeaponKeys => weaponKeys;

        public Transform CenterOfBase
        {
            get => _centerOfBase;
            set => _centerOfBase = value;
        }

        public Transform CenterOfGarage
        {
            get => _centerOfGarage;
            set => _centerOfGarage = value;
        }

        public DamageNumberManager DamageNumberManager => _damageNumberManager;

        public IDamageable GetDamageable => PlayerMovementMode.Equals(PlayerMovementMode.Drive)
            ? _playerCarController.CarController.CarStatusController
            : _playerStatusController;

        public string CurrentSkinKey { get; set; }
        public bool InBase => _playerMovementController.InBase;
        public ItemPicker ItemPicker => _itemPicker;
        public MobManager MobManager => _mobManager;

        public AudioManager AudioManager => _audioManager;

        public PlayerMovementController PlayerMovementController => _playerMovementController;

        public PlayerMovementMode PreviousPlayerMovementMode { get; set; } = PlayerMovementMode.Walk;

        public PlayerMovementMode PlayerMovementMode
        {
            get => _playerMovementMode;
            set
            {
                PreviousPlayerMovementMode = _playerMovementMode;
                _playerMovementMode = value;
                PlayerInCarStateChanged?.Invoke(_playerMovementMode.Equals(PlayerMovementMode.Drive));

                var moveCarToGarage = PlayerPrefs.GetInt("MoveCarToGarage", 0) == 1;
                switch (_playerMovementMode)
                {
                    case PlayerMovementMode.Walk:
                        if (blobShadow)
                            blobShadow.SetActive(true);
                        ExitedCar?.Invoke(moveCarToGarage);
                        break;
                    case PlayerMovementMode.Drive:
                        if (blobShadow)
                            blobShadow.SetActive(false);
                        EnteredCar?.Invoke(moveCarToGarage);
                        break;
                    case PlayerMovementMode.CutScene:
                        if (blobShadow)
                            blobShadow.SetActive(false);
                        CutSceneEntered?.Invoke();
                        break;
                }

                _itemPicker.ConfigureRadius(_playerMovementMode);
                _playerWeaponController.HandleRangeIndicatorCheck(_playerMovementMode.Equals(PlayerMovementMode.Drive));
            }
        }

        public Transform PlayerTransform => modelTransform;
        public VibrationManager VibrationManager => _vibrationManager;
        public WaveManager WaveManager => _waveManager;

        public PlayerWeaponController WeaponController
        {
            get => _playerWeaponController;
            set => _playerWeaponController = value;
        }

        public bool IsCheckPointInit { get; set; }

        public PlayerTargetingController TargetingController
        {
            get => _playerTargetingController;
            set => _playerTargetingController = value;
        }

        #endregion

        #region Events

        public event Action<bool> PlayerInCarStateChanged;
        public event Action<bool> EnteredCar;
        public event Action<bool> ExitedCar;
        public event Action CutSceneEntered;
        public event Action<string> SpeechBubbleShown;
        public event Action<Weapon> WeaponInitialized;
        public event Action<Weapon, Weapon> WeaponSwitched;

        #endregion

        #region Unity Methods

        private async void Awake()
        {
            SetPlayerPosition();
            SendPlayerSpawnAnalytic();
            _itemPicker = GetComponent<ItemPicker>();
            _playerSkillController = GetComponent<PlayerSkillController>();
            _playerMovementController = GetComponent<PlayerMovementController>();
            _playerCarController = GetComponent<PlayerCarController>();
            _playerStatusController = GetComponent<PlayerStatusController>();
            _playerAnimationController = GetComponent<PlayerAnimationController>();
            _playerWeaponController = new PlayerWeaponController(this,

                _playerStatusController,
                _mobManager,
                _playerAnimationController,
                _playerSkillController,
                _resolver);

            await SetSkin(GetInitialSkinKey());

            _playerTargetingController = new PlayerTargetingController(_playerWeaponController,
                lHandTarget,
                rHandTarget,
                modelTransform,
                _mobManager,
                boxManager,
                _playerAnimationController,
                maxFrontAnglePerArm,
                maxBackAnglePerArm,
                _damageableRegisterService);

            _playerAnimationController.ToggleDeadState(false);
            _playerStatusController.Died += (damageSource) => _playerAnimationController.ToggleDeadState(true);
            _playerStatusController.Refill += () => _playerAnimationController.ToggleDeadState(false);
            _playerStatusController.Refill += () => _playerAnimationController.ToggleDeadState(false);

            InitCheckPoint();
        }

        private void Update()
        {
            if (!_skinLoaded) return;
            if (InBase)
            {
                _playerTargetingController.ResetAllHands();
                return;
            }

            if (_playerMovementMode.Equals(PlayerMovementMode.Drive)) return;

            _playerTargetingController.Update();
            _playerWeaponController.Update();
        }


        private void OnDestroy()
        {
            _playerWeaponController?.Dispose();
            _playerTargetingController?.Dispose();
        }

        #endregion

        #region Public Methods

        [Inject]
        public void Init(MobManager mobManager, DamageNumberManager damageNumberManager,
            IDamageableRegisterService damageableRegisterService, VibrationManager vibrationManager,
            WaveManager waveManager, IAnalyticsService analyticsService,
            AudioManager audioManager,
            IObjectResolver resolver, TutorialSequenceController tutorialService)
        {
            _tutorialService = tutorialService;
            _mobManager = mobManager;
            _damageNumberManager = damageNumberManager;
            _damageableRegisterService = damageableRegisterService;
            _vibrationManager = vibrationManager;
            _waveManager = waveManager;
            _analyticsService = analyticsService;
            _audioManager = audioManager;
            _resolver = resolver;
            playerBaseDistanceChecker.SetMobManager(mobManager);
        }

        public async UniTask SetSkin(string skinKey)
        {
            var skinPrefab = await Addressables.LoadAssetAsync<GameObject>(skinKey).BindTo(gameObject);
            skinParent.RemoveAllChildren();
            var skin = _resolver.Instantiate(skinPrefab, skinParent);
            var skinController = skin.GetComponent<PlayerSkinController>();
            skinController.AdjustIK(lHandTarget, lHandBendGoal, rHandTarget, rHandBendGoal);
            OnSkinChanged?.Invoke(skinKey);
            _playerAnimationController.Initialize(skinController.Animator, skinController.LeftArmIk,
                skinController.RightArmIk);
            WeaponController.UpdateWeaponSlots(skinController.WeaponSlots);
            _skinLoaded = true;
            CurrentSkinKey = skinKey;
        }

        public void PlayOneShotAudio(string clipName, float volumeScale = 1)
        {
            _audioManager.PlayOneShot(clipName, volumeScale);
        }

        public void SetBasePopulationNpcsManager(BasePopulationNpcsManager basePopulationNpcsManager)
        {
            playerBaseDistanceChecker.SetBasePopulationNpcsManager(basePopulationNpcsManager);
        }

        /// <summary>
        /// This method is called in sync when a melee attack happens in the animation, so it should deal damage here
        /// </summary>
        public void OnMeleeSwingStarted()
        {
            if (_meleeWeapon)
                _meleeWeapon.ToggleSlashEffect(true);
        }

        public void OnMeleeSwingEnded()
        {
            if (_meleeWeapon)
                _meleeWeapon.ToggleSlashEffect(false);
        }

        public void OnMeleeAttackHappened()
        {
            _playerTargetingController.OnMeleeAttackHappened();
        }

        public void ShowSpeechBubble(string speechText)
        {
            SpeechBubbleShown?.Invoke(speechText);
        }

        public void InvokeWeaponInitialized(Weapon weapon)
        {
            WeaponInitialized?.Invoke(weapon);
        }

        public void InvokeTravelButtonStatusChanged(bool isActive)
        {
            OnTravelButtonStatusChanged?.Invoke(isActive);
        }


        public void InvokeWeaponSwitched(Weapon oldWeapon, Weapon newWeapon)
        {
            WeaponSwitched?.Invoke(oldWeapon, newWeapon);

            float range = newWeapon.Range;
            UpdateRangeIndicatorRange(range);
        }

        public void ToggleRangeIndicator(bool toggle)
        {
            rangeIndicator.SetActive(toggle);
        }

        public void UpdateRangeIndicatorRange(float range)
        {
            float width = range * 2;
            mainWeaponDecalProjector.size = new Vector3(width, width, 16);
        }

        public void TravelToBase()
        {
            if (_playerMovementMode == PlayerMovementMode.Drive)
            {
                _playerCarController.InvokeCarExitedByForce();
            }

            var targetBase = playerStartPoints.Gen().Where(x => x.type == PlayerStartPointType.Default).Select(x => x.position).FirstOrDefault();

            if (targetBase != null)
            {
                transform.position = targetBase;
            }
        }

        #endregion

        #region Private Methods

        private void InitCheckPoint()
        {
            if (_tutorialService.IsTutorialCompleted)
            {
                return;
            }

            var checkPointData = _tutorialService.GetTutorialCheckPointData();
            if (!checkPointData.HasValue)
            {
                return;
            }

            transform.position = checkPointData.Value.Position.ToVector3();
            IsCheckPointInit = true;
        }

        private void SendPlayerSpawnAnalytic()
        {
            _analyticsService.LogEvent(new EventParameters<string>()
            {
                EventName = "blaster_spawn",
                AdjustToken = AdjustNsEventTokens.BlasterSpawn
            });
        }

        private void SetPlayerPosition()
        {
            if (!_tutorialService.IsTutorialCompleted && IsCheckPointInit)
            {
                return;
            }

            transform.position = playerStartPoints.Gen()
                .Where(x => x.type ==
                            (_tutorialService.IsTutorialCompleted ? PlayerStartPointType.Default : PlayerStartPointType.Tutorial))
                .Select(x => x.position).FirstOrDefault();
        }

        public string GetInitialSkinKey()
        {
            var data = SaveLoadHelper.TryLoadPersistentData<CharacterSelectionData>();
            return data?.SelectedCharacterKey ?? defaultSkinKey;
        }

        #endregion

#if UNITY_EDITOR
        /*private void OnDrawGizmos()
        {
            if (!debug) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(modelTransform.position, 7f);
            //draw wire spheres on l and r hand target positions
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lHandTarget.position, 0.5f);
            Gizmos.DrawWireSphere(rHandTarget.position, 0.5f);

            //draw line from l and r hand target to current target
            if (_currentTargetL != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(lHandTarget.position, _currentTargetL.Transform.position);
            }

            if (_currentTargetR != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rHandTarget.position, _currentTargetR.Transform.position);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(modelTransform.position, modelTransform.position + modelTransform.right * 3);
            Gizmos.DrawLine(modelTransform.position, modelTransform.position + modelTransform.right * -3);
        }*/
#endif
    }


    [Serializable]
    public struct PlayerStartPoint
    {
        #region Serializable Fields

        public string name;
        public PlayerStartPointType type;
        public Vector3 position;

        #endregion
    }


    public enum PlayerStartPointType
    {
        Default,
        Tutorial
    }
}


public enum PlayerMovementMode
{
    Walk,
    Drive,
    CutScene
}
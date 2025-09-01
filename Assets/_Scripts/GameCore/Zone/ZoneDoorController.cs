using _Scripts.Utilities;
using DG.Tweening;
using GameCore.Player;
using GameCore.Player.Input;
using GameCore.Tutorial;
using Managers;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Zone
{
    public class ZoneDoorController : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Transform[] doors;
        [SerializeField] private GameObject bridgeWall;
        [SerializeField] private GameObject normalBridge;
        [SerializeField] private GameObject brokenBridge;
        [SerializeField] private GameObject[] outlineDoors;
        [SerializeField] private GameObject[] normalDoors;
        [SerializeField] private bool isDown;

        #endregion


        #region Fields

        private const string PLAYER = "Player";
        private const string CAR = "Car";
        private readonly float _doorOpenDuration = 0.3f;

        private PlayerController _player;
        private PlayerCarController _playerCarController;
        private PlayerMovementController _playerMovementController;
        private AudioManager _audioManager;
        private TutorialSequenceController _tutorialService;

        #endregion

        #region Properties

        public bool LastDoorState { get; set; }
        public bool IsLocked { get; set; }

        #endregion


        #region Unity Methods

        [Inject]
        public void Init(AudioManager audioManager, PlayerController playerController,
            TutorialSequenceController tutorialService)
        {
            _tutorialService = tutorialService;
            _audioManager = audioManager;
            _player = playerController;
        }

        private void Start()
        {
            _playerCarController = _player.GetComponent<PlayerCarController>();
            _playerMovementController = _player.GetComponent<PlayerMovementController>();
            SetActivateOutlineDoors(_tutorialService.IsTutorialCompleted);
            InitializeBridgeStatus();
        }

        private void InitializeBridgeStatus()
        {
            if (_tutorialService.IsTutorialCompleted)
            {
                SetActiveBridge(true);
                return;
            }

            var checkPointData = _tutorialService.GetTutorialCheckPointData();
            if (!checkPointData.HasValue) return;

            SetActiveBridge(true);
        }

        private void PlayDoorSound(string key)
        {
            if (_audioManager == null)
                return;

            _audioManager.PlayOneShot(key);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (IsLocked) return;

            if (other.gameObject.CompareTag(PLAYER) || other.gameObject.CompareTag(CAR))
            {
                if (IsWaveActive(other))
                {
                    LoggerNS.Log("Wave is active, door will not open");
                    return;
                }

                OpenDoors(other.transform);
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag(PLAYER) && !other.gameObject.CompareTag(CAR)) return;
            if (IsWaveActive(other))
            {
                LoggerNS.Log("Wave is active, door will not close");
                return;
            }

            CloseDoors();
        }

        #endregion


        #region Public Methods

        public void CloseDoorOutline(bool value)
        {
            if (isDown) return;

            SetActivateOutlineDoors(value);
        }

        public void OpenDoors(Transform target)
        {
            var doorOffset = isDown ? -5 : 5;
            var outsideYValue = transform.position.z < target.position.z + doorOffset ? -90f : 90f;
            var insideYValue = transform.position.z + doorOffset > target.position.z ? -90f : 90f;
            var endYValue = _playerMovementController.InBase ? insideYValue : -outsideYValue;
            foreach (var door in doors)
                door.DOLocalRotate(new Vector3(0f, endYValue, 0f), _doorOpenDuration).SetEase(Ease.OutBack);

            PlayDoorSound("BaseDoorOpen");
            LastDoorState = true;
        }

        public void CloseDoors()
        {
            doors[0].DOLocalRotate(new Vector3(0f, 180f, 0f), _doorOpenDuration).SetEase(Ease.OutBack);
            doors[1].DOLocalRotate(Vector3.zero, _doorOpenDuration).SetEase(Ease.OutBack);

            PlayDoorSound("BaseDoorClose");
            LastDoorState = false;
        }

        #endregion

        #region Private Methods

        private void SetActivateOutlineDoors(bool isActive)
        {
            foreach (var door in outlineDoors) door.SetActive(isActive);

            foreach (var door in normalDoors) door.SetActive(!isActive);
        }


        public void SetActiveBridge(bool isActive)
        {
            if (normalBridge != null) normalBridge.SetActive(!isActive);
            if (brokenBridge != null) brokenBridge.SetActive(isActive);
            if (bridgeWall != null) bridgeWall.SetActive(isActive);
        }


        private bool IsWaveActive(Collider other)
        {
            return other.tag switch
            {
                PLAYER => other.TryGetComponent(out PlayerController playerController) &&
                    playerController.WaveManager.IsWaveActive,
                CAR => other.TryGetComponent(out CarController carController) && carController.WaveManager.IsWaveActive,
                _ => true
            };
        }

        #endregion
    }
}

using System;
using _Scripts.GameCore.Zone;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Spawner;
using Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace GameCore.Misc
{
    public class CarCutsceneController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private GameObject cutsceneCamera;
        [SerializeField] private GameObject[] objectsToEnableAftercinematic;
        [SerializeField] private ZoneDoorController zoneDoorController;

        private bool _cutSceneTriggered;
        private PlayerController _playerController;
        private PlayerCarController _carController;
        private CinemachineBrain _brain;
        private MobManager _mobManager;
        private IAnalyticsService _analyticsService;

        private void OnTriggerEnter(Collider other)
        {
            if (!_cutSceneTriggered && other.CompareTag("Player"))
            {
                _cutSceneTriggered = true;
                OnCutSceneEnter();
            }
        }

        [Inject]
        public void Construct(PlayerController controller, PlayerCarController carController, MobManager mobManager,
            IAnalyticsService analyticsService)
        {
            _mobManager = mobManager;
            _carController = carController;
            _playerController = controller;
            _analyticsService = analyticsService;

            var camera = Camera.main;
            if (camera)
                _brain = camera.GetComponent<CinemachineBrain>();
        }

        public void OnCutSceneEnter()
        {
            _mobManager.IsLocked = true;
            _playerController.PlayerMovementMode = PlayerMovementMode.CutScene;

            cutsceneCamera.SetActive(true);
            _carController.MoveCar();
            if (_playerController.TryGetComponent(out CharacterController characterController))
                characterController.transform.position = new Vector3(-46.74f, 1.08f, -558.21f);
            director.Play();

            _analyticsService.LogEvent(new EventParameters<string>
            {
                EventName = "cinematic_begin",
                AdjustToken = AdjustNsEventTokens.CinematicBegin
            });
        }

        public void OnCutSceneExit()
        {
            foreach (var enableObjects in objectsToEnableAftercinematic)
            {
                enableObjects.SetActive(true);
            }

            zoneDoorController.SetActiveBridge(true);

            _playerController.PlayerMovementMode = PlayerMovementMode.Walk;
            _playerController.transform.position = playerSpawnPoint.position;
            _playerController.transform.rotation = playerSpawnPoint.rotation;
        }
    }
}

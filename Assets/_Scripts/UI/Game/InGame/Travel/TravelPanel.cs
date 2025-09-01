using GameCore.Player;
using GameCore.Player.Input;
using GameCore.PopupSystem;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace UI.Game.InGame.Travel
{
    public class TravelPanel : MonoBehaviour
    {
        [SerializeField] private Button travelButton;

        private PopupManager _popupManager;
        private PlayerController _playerController;
        private PlayerMovementController _playerMovementController;
        private ITutorialService _tutorialService;

        private void Awake()
        {
            travelButton.onClick.RemoveAllListeners();
            travelButton.onClick.AddListener(OpenTravelPopup);
        }

        private void OnEnable()
        {
            _playerController.OnTravelButtonStatusChanged += SetActivity;
            _playerMovementController.InBaseChanged += SetActivityByBase;
            _tutorialService.TutorialCompleted += TutorialCompleted;
        }

        private void OnDestroy()
        {
            _playerController.OnTravelButtonStatusChanged -= SetActivity;
            _playerMovementController.InBaseChanged -= SetActivityByBase;
            _tutorialService.TutorialCompleted -= TutorialCompleted;
        }

        [Inject]
        private void Initialize(PopupManager popupManager, PlayerController playerController, ITutorialService tutorialService)
        {
            _popupManager = popupManager;
            _playerController = playerController;
            _tutorialService = tutorialService;
            _playerMovementController = _playerController.GetComponent<PlayerMovementController>();
        }

        private async void OpenTravelPopup()
        {
            await _popupManager.OpenPopup(PopupConstants.PopupType.Travel);
        }

        private void SetActivity(bool isActive)
        {
            travelButton.gameObject.SetActive(isActive);
        }
        
        
        private void TutorialCompleted()
        {
            SetActivityByBase(true);
        }
        

        private void SetActivityByBase(bool isActive)
        {
            if (!_tutorialService.IsTutorialCompleted) return;
            
            travelButton.gameObject.SetActive(!isActive);
        }
    }
}


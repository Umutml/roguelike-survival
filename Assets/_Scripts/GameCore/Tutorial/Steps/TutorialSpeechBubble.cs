using System;
using _Scripts.GameCore.Player;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using Interfaces;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/Tutorial Speech Bubble")]
    public class TutorialSpeechBubble : TutorialStep
    {
        [SerializeField] private string speechText;
        [SerializeField] private float timeToDisplay = 3f;
        [SerializeField] private bool isCloseWall;

        private PlayerController _playerController;
        private PlayerSpeechBubble _playerSpeechBubble;
        private ITutorialService _tutorialService;

        public override UniTask ProcessStep()
        {
            _playerController = Resolver.Resolve<PlayerController>();
            _tutorialService = Resolver.Resolve<ITutorialService>();
            _playerSpeechBubble = _playerController.playerSpeechBubble;
            DisplaySpeechBubble();
            if (isCloseWall)
                _tutorialService.CloseTutorialWall(false);
            return UniTask.CompletedTask;
        }

        private void DisplaySpeechBubble()
        {
            _playerController.ShowSpeechBubble(speechText);
            //_playerSpeechBubble.ShowSpeechBubble(speechText, timeToDisplay);
        }
    }
}

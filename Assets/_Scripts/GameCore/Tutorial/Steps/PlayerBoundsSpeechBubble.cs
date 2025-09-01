using _Scripts.GameCore.Player;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

[CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/PlayerBoundsSpeechBubble")]
public class PlayerBoundsSpeechBubble : TutorialStep
{
    [SerializeField] private string speechBubbleText;


    private PlayerController _playerController;

    public override UniTask ProcessStep()
    {
        _playerController = Resolver.Resolve<PlayerController>();
        _playerController.playerSpeechBubble.ShowSpeechBubble(speechBubbleText);
        return UniTask.CompletedTask;
    }
}

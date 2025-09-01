using DG.Tweening;
using GameCore.Player;
using UI.Game.Architectural;
using UnityEngine;
using VContainer;

public class SpeechBubbleController : Content
{
    #region Consts

    private const string SPEECH_TEXT = "SpeechText";
    private static readonly int Enter = Animator.StringToHash("Enter");
    private static readonly int Exit = Animator.StringToHash("Exit");

    #endregion


    #region Fields

    private PlayerController _playerController;
    private Animator _animator;
    
    #endregion


    #region Unity Methods

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    #endregion


    #region Public Methods

    public void ShowSpeechBubbleShown(string bubbleText)
    {
        //SetText(SPEECH_TEXT, bubbleText);
        //_animator.SetTrigger(Enter);
    }


    public void CloseSpeechBubble()
    {
        _animator.SetTrigger(Exit);
    }

    #endregion


    #region Private Methods
    
    [Inject]
    private void Init(PlayerController playerController)
    {
        _playerController = playerController;
        
        _playerController.SpeechBubbleShown += ShowSpeechBubbleShown;
    }
    
    #endregion
}

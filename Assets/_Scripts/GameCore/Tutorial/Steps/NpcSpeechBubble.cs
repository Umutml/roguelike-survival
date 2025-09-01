using System;
using _Scripts.GameCore.Player;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

[CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New NpcSpeechBubble")]
public class NpcSpeechBuble : TutorialStep
{
    [SerializeField] private string speechText;
    private PlayerSpeechBubble _playerSpeechBubble;
    private PlayerController _player;

    public override UniTask ProcessStep()
    {
        var npc = GameObject.Find("NpcSpeechBubble");
        _playerSpeechBubble = npc.GetComponent<PlayerSpeechBubble>();
        _playerSpeechBubble.ShowSpeechBubble(speechText);

        _player = Resolver.Resolve<PlayerController>();
        _player.EnteredCar += OnEnteredCar;
        return UniTask.CompletedTask;
    }

    private async void OnEnteredCar(bool b)
    {
        OpenBridgeHiddenWall();
        await UniTask.Delay(2000);
        _playerSpeechBubble.HideSpeechBubble();
    }

    private async void OpenBridgeHiddenWall()
    {
        var bridgeStartPoint = await TutorialService.GetTutorialObject("BridgeStartPointHiddenWall");
        if (bridgeStartPoint == null)
        {
            LoggerNS.LogError("Couldn't find the BridgeStartPointHiddenWall object can't disable check string name");
            return;
        }

        bridgeStartPoint.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_player)
            _player.EnteredCar -= OnEnteredCar;
    }
}
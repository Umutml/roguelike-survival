using System;
using _Scripts.GameCore.Tutorial;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;

namespace GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/Tutorial Gate")]
    public class TutorialGateStep : TutorialStep
    {
        public override async UniTask ProcessStep()
        {
            var gateObject = await TutorialService.GetTutorialObject("TutorialGate");
            var gate = gateObject.GetComponent<TutorialGate>();
            if (gate != null)
            {
                gate.OpenDoor();
            }
            else
            {
                LoggerNS.LogError("TutorialGateStep: No TutorialGate found in scene");
            }
        }
    }
}
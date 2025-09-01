using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New Tutorial Delay")]
    public class WaitDelay : TutorialStep
    {
        public float WaitTimeSeconds;

        public override UniTask ProcessStep()
        {
            return UniTask.Delay((int)(WaitTimeSeconds * 1000), ignoreTimeScale: true);
        }
    }
}

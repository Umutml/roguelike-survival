using System;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;


namespace _Scripts.GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/Tutorial Barricade")]
    public class TutorialBarricadeStep : TutorialStep
    {
        private TutorialBarricade _tutorialBarricade;

        public override async UniTask ProcessStep()
        {
            _tutorialBarricade = Resolver.Resolve<TutorialBarricade>();
            _tutorialBarricade.RegisterDamageable();
        }
    }
}

using System;
using Interfaces;
using UnityEngine;
using VContainer;

namespace UI.Game.InGame.IngameTutorial
{
    public class IngameTutorialUIController : MonoBehaviour
    {
        [SerializeField] private TutorialUIStep[] tutorialUISteps;

        private ITutorialService _tutorialService;

        [Inject]
        private void Initialize(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;
            _tutorialService.TutorialStepChanged += OnTutorialStepChanged;
            _tutorialService.TutorialStepCompleted += OnTutorialStepCompleted;
        }

        private void OnDestroy()
        {
            _tutorialService.TutorialStepChanged -= OnTutorialStepChanged;
            _tutorialService.TutorialStepCompleted -= OnTutorialStepCompleted;
        }

        private void OnTutorialStepCompleted(string tutorialStepName)
        {
            foreach (var tutorialUIStep in tutorialUISteps)
            {
                tutorialUIStep.UIObject.SetActive(false);
            }
        }

        private void OnTutorialStepChanged(string tutorialStepName)
        {
            foreach (var tutorialUIStep in tutorialUISteps)
            {
                tutorialUIStep.UIObject.SetActive(tutorialUIStep.Name == tutorialStepName);
            }
        }


        [Serializable]
        private class TutorialUIStep
        {
            public string Name;
            public GameObject UIObject;
        }
    }
}
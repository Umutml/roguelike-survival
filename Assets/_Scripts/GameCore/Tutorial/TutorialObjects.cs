using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial
{
    public class TutorialObjects : MonoBehaviour
    {
        [SerializeField] private ITutorialService.TutorialObject[] objects;
        [SerializeField] private bool isCleanCityScene;

        [Inject]
        private void Construct(ITutorialService tutorialService)
        {
            tutorialService.SetObjects(objects, isCleanCityScene);
        }
    }
}
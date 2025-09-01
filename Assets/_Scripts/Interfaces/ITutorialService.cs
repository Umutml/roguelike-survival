using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Interfaces
{
    public interface ITutorialService
    {
        public string CurrentTutorialStepName { get; set; }

        public event Action<string> TutorialStepChanged;
        public event Action<string> TutorialStepCompleted;
        public event Action TutorialCompleted;
        
        public bool IsTutorialCompleted { get; set; }
        
        public void CloseTutorialWall(bool isBase);

        public void SetObjects(TutorialObject[] objects, bool isCleanCityScene);
        
        public UniTask<GameObject> GetTutorialObject(string name);

        [Serializable]
        public struct TutorialObject
        {
            public string Name;
            public GameObject Object;
        }
        
    }
}
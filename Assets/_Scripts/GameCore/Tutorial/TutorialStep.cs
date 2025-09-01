using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial
{

    public class TutorialStep : ScriptableObject
    {
        public bool PauseGame;
        
        public bool Skip;
        
        public ITutorialService TutorialService;

        public IObjectResolver Resolver;

        public virtual async UniTask ProcessStep()
        {
            if(PauseGame)
                Time.timeScale = 0;
        }

        protected virtual void Unpause()
        {
            Time.timeScale = 1;
        }
    }
}

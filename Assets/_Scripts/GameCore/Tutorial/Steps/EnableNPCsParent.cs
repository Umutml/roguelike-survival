using _Scripts.GameCore.NPC;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New EnableNPCsParent")]
    public class EnableNPCsParent : TutorialStep
    {
        [SerializeField] private bool toggle;
        private BasePopulationNpcsManager _basePopulationNpcsManager;
        public override async UniTask ProcessStep()
        {
            await base.ProcessStep();
            
            _basePopulationNpcsManager = FindFirstObjectByType<BasePopulationNpcsManager>();

            if (_basePopulationNpcsManager != null)
            {
                _basePopulationNpcsManager.SetEnableAllChildren(toggle);
            }
        }
    }
}

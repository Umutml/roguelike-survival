using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleNpcStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Npc Status Step")]
    public class ToggleNpcStatusStep : TutorialStep
    {
        [SerializeField] private string npcName;
        [SerializeField] private bool isLocked;

        public override async UniTask ProcessStep()
        {
            var npcObject = await TutorialService.GetTutorialObject(npcName);

            if (npcObject == null)
            {
                LoggerNS.LogError("ToggleNpcStatusStep: No npc found in scene");
                return;
            }

            if (!npcObject.TryGetComponent(out AreaBaseNpc areaBaseNpc))
            {
                LoggerNS.LogError("ToggleNpcStatusStep: No UpgradeNpc found in npc");
                return;
            }

            areaBaseNpc.SetActivateNpcObjects(!isLocked);
        }
    }
}
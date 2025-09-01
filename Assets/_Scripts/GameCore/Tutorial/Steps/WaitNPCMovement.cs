using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using MyBox;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitNPCMovement",
        menuName = "ScriptableObjects/Tutorial/Steps/Wait NPC Movement",
        order = 0)]
    public class WaitNpcMovement : TutorialStep
    {
        [SerializeField] private bool followTutorialObject;
        [ConditionalField(nameof(followTutorialObject), false)][SerializeField] private string targetTransformName;
        [SerializeField] private string npcName;
        [SerializeField] private bool isAwaiting = true;
        [SerializeField] private float moveSpeed = 4f;

        public override async UniTask ProcessStep()
        {
            Transform targetTransform;
            var npcObject = await TutorialService.GetTutorialObject(npcName);
            if (followTutorialObject)
            {
                var targetObject = await TutorialService.GetTutorialObject(targetTransformName);
                targetTransform = targetObject.transform;
            }
            else
            {
                var playerController = Resolver.Resolve<PlayerController>();
                targetTransform = playerController.transform;
            }

            if (!npcObject.TryGetComponent(out BasePopulationNpcAIPathController npcAIPathController))
            {
                return;
            }

            npcAIPathController.TargetTransform = targetTransform;
            npcAIPathController.MoveSpeed = moveSpeed;
            npcAIPathController.IsPending = false;
            npcAIPathController.IsCompleted = false;
            npcAIPathController.IsIdle = false;

            if (isAwaiting)
            {
                await UniTaskAsyncHelper.WaitWhile(() => npcAIPathController.IsCompleted == false, 400);
            }
        }
    }

}
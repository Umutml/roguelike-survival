using _Scripts.GameCore.Zone;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "DoorActionStep", menuName = "ScriptableObjects/Tutorial/Steps/Door Action Step")]
    public class DoorActionStep : TutorialStep
    {
        [SerializeField] private string doorName;
        [SerializeField] private bool isOpen;

        public override async UniTask ProcessStep()
        {
            var door = await TutorialService.GetTutorialObject(doorName);

            if (door == null)
            {
                LoggerNS.LogError("DoorActionStep: No door found in scene");
                return;
            }

            if (!door.TryGetComponent(out ZoneDoorController zoneDoorController))
            {
                LoggerNS.LogError("DoorActionStep: No ZoneDoorController found in door");
                return;
            }

            if (isOpen)
            {
                var player = Resolver.Resolve<PlayerController>().transform;
                zoneDoorController.OpenDoors(player);
            }
            else
            {
                zoneDoorController.CloseDoors();
            }
        }
    }
}

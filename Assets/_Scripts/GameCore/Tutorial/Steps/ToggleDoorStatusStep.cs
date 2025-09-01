using _Scripts.GameCore.Zone;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleDoorStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Door Status Step")]
    public class ToggleDoorStatusStep : TutorialStep
    {
        [SerializeField] private string doorName;
        [SerializeField] private bool isLocked;

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

            zoneDoorController.IsLocked = isLocked;
        }
    }
}

using _Scripts.GameCore.Zone;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player.Input;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitForDoorState", menuName = "ScriptableObjects/Tutorial/Steps/Wait For Door State")]
    public class WaitForDoorState : TutorialStep
    {
        [SerializeField] private bool upDoorLocked;
        [SerializeField] private bool downDoorLocked;

        public override async UniTask ProcessStep()
        {
            var doorUp = await TutorialService.GetTutorialObject("ZoneUpDoor");
            var doorDown = await TutorialService.GetTutorialObject("ZoneDownDoor");

            if (doorUp == null || doorDown == null)
            {
                LoggerNS.LogError("WaitForDoorState: No ZoneUpDoor or ZoneDownDoor found in scene");
                return;
            }

            var doorUpController = doorUp.GetComponent<ZoneDoorController>();
            var doorDownController = doorDown.GetComponent<ZoneDoorController>();

            if (doorUpController == null || doorDownController == null)
            {
                LoggerNS.LogError("WaitForDoorState: No ZoneDoorController found in ZoneUpDoor");
                return;
            }

            doorUpController.IsLocked = upDoorLocked;
            doorDownController.IsLocked = downDoorLocked;

            await UniTask.WaitUntil(() =>
                doorUpController.LastDoorState == true || doorDownController.LastDoorState == true);

            doorUpController.IsLocked = false;
            doorDownController.IsLocked = false;
        }
    }
}

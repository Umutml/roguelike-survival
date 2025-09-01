using Cysharp.Threading.Tasks;
using GameCore.Player;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "EnterVehicleStateStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Enter Vehicle State Step",
        order = 0)]
    public class EnterVehicleStateStep : TutorialStep
    {
        [SerializeField] private bool isLock;

        private ItemPicker _itemPicker;
        public override UniTask ProcessStep()
        {
            _itemPicker = Resolver.Resolve<ItemPicker>();
            _itemPicker.LockCarPickup = isLock;
            return UniTask.CompletedTask;
        }
    }
}
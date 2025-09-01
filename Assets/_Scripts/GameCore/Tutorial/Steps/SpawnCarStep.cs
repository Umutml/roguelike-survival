using Cysharp.Threading.Tasks;
using GameCore.Car;
using UnityEngine;
using VContainer;


namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "SpawnCarStep",
     menuName = "ScriptableObjects/Tutorial/Steps/Spawn Car Step",
     order = 0)]
    public class SpawnCarStep : TutorialStep
    {
        [SerializeField] private CarType carType;
        [SerializeField] private CarSpawnType carSpawnType;
        private CarManager _carManager;

        public override async UniTask ProcessStep()
        {
            _carManager = Resolver.Resolve<CarManager>();
            await _carManager.SpawnCar(carType, carSpawnType);
        }
    }
}

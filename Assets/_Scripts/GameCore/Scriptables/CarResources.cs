using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Car;
using MyBox;
using UnityEngine.AddressableAssets;
using Utilities;


namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CarResources", menuName = "Scriptables/CarResources", order = 1)]
    public class CarResources : ScriptableObject
    {
        [SerializeField] private List<Car> carList = new();
        [SerializeField] private List<CarTransform> carSpawnPoints;

        [Header("Garage")]
        [SerializeField] private GarageTransform garageTransform;

        [Header("Tutorial Car Settings")]
        [SerializeField] private CarType tutorialCarType;

        [SerializeField] private CarTransform tutorialCarTransform;
        [SerializeField] private float tutorialCarHealth;


        public List<Car> CarList => carList;
        public List<CarTransform> CarSpawnPoints => carSpawnPoints;
        public CarTransform TutorialCarTransform => tutorialCarTransform;
        public GarageTransform GarageTransform => garageTransform;
        public CarType TutorialCarType => tutorialCarType;
        public Car GetCar(CarType targetCar) => carList.FirstOrDefault(car => car.CarType.Equals(targetCar));
        public float TutorialCarHealth => tutorialCarHealth;


#if UNITY_EDITOR

        [ButtonMethod]
        public void CheckSpawnPointsDistance()
        {
            for (var i = 0; i < carSpawnPoints.Count; i++)
            {
                for (var j = i + 1; j < carSpawnPoints.Count; j++)
                {
                    var distance = Vector3.Distance(carSpawnPoints[i].Position, carSpawnPoints[j].Position);
                    if (distance < 2)
                    {
                        LoggerNS.LogError(
                            $"Spawn Points are too close to each other. Distance between {i} and {j} is {distance}");
                        break;
                    }
                }
            }

            LoggerNS.Log("All Spawn Points are fine.");
        }

#endif
    }


    [Serializable]
    public struct Car
    {
        [SerializeField] private CarType carType;
        [SerializeField] private CarBuyData carBuyData;
        [SerializeField] private AssetReference carModelArt;
        [SerializeField] private int upgradeCount;
        [SerializeField] private string carName;
        [SerializeField] private CarController carPrefab;
        [SerializeField] private GameObject carModel;
        [SerializeField] private int carCount;
        [SerializeField] private UnlockObjectType unlockObjectType;

        [Header("Status")][SerializeField] private float maxHealt;
        [SerializeField] private float maxArmor;

        [Header("Movement")][SerializeField] private float moveSpeed;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float drag;
        [SerializeField] private float steerAngle;
        [SerializeField] private float traction;
        [SerializeField] private float driftMultiplier;
        [SerializeField] private float driftSpeedMultiplier;
        [SerializeField] private float driftOffset;
        [SerializeField] private float tiltAmount;
        [SerializeField] private float liftAmount;

        public CarType CarType => carType;
        public string CarName => carName;
        public CarBuyData CarBuyData => carBuyData;
        public async UniTask<Sprite> GetCarModelArt() => await AssetManager<Sprite>.LoadObject(carModelArt);
        public int UpgradeCount => upgradeCount;
        public CarController CarPrefab => carPrefab;
        public GameObject CarModel => carModel;
        public int CarCount => carCount;
        public UnlockObjectType UnlockObjectType => unlockObjectType;
        public float MaxHealt => maxHealt;
        public float MaxArmor => maxArmor;
        public float MoveSpeed => moveSpeed;

        public float MaxSpeed
        {
            get { return maxSpeed; }
            set { maxSpeed = value; }
        }

        public float Drag => drag;
        public float SteerAngle => steerAngle;
        public float Traction => traction;
        public float DriftMultiplier => driftMultiplier;
        public float DriftSpeedMultiplier => driftSpeedMultiplier;
        public float DriftOffset => driftOffset;
        public float TiltAmount => tiltAmount;
        public float LiftAmount => liftAmount;
    }


    [Serializable]
    public struct CarTransform
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;

        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
    }


    [Serializable]
    public struct CarBuyData
    {
        [SerializeField] private string lockedMessage;
        [SerializeField] private int price;
        [SerializeField] private int waveCount;
        [SerializeField] private int remainingCount;
        [SerializeField] private PurchaseOptions purchaseOptions;
        
        public string LockedMessage => lockedMessage;
        public int Price => price;
        public int WaveCount => waveCount;
        public int RemainingCount => remainingCount;
        public PurchaseOptions PurchaseOptions => purchaseOptions;
    }


    [Serializable]
    public struct GarageTransform
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;

        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
    }
}
using UnityEngine;
using GameCore.Car;

namespace  GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CarAttackResources", menuName = "Scriptables/CarAttackResources", order = 1)]
    public class CarAttackResources : ScriptableObject
    {
        [SerializeField] private CarType carType;
        [SerializeField] private string bulletAssetKey;
        [SerializeField] private float attackRange;
        [SerializeField] private float attackRate;
        
        
        public CarType CarType => carType;
        public string BulletAssetKey => bulletAssetKey;
        public float AttackRange => attackRange;
        public float AttackRate => attackRate;
    }
}

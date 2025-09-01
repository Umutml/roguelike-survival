using UnityEngine;
using UnityEngine.Pool;

namespace GameCore.AI.Behaviour
{
    public static class RigidBodyPooler
    {
        
        public static IObjectPool<Rigidbody> RigidBodyPool;
        
        public static void  GenerateRigidBodyPool()
        {
            RigidBodyPool = new ObjectPool<Rigidbody>(
                createFunc: CreatePooledItem,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPoolObject,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        private static Rigidbody CreatePooledItem()
        {
            var go = new GameObject("Pooled Item");
            return go.AddComponent<Rigidbody>();
        }

        private static void OnTakeFromPool(Rigidbody component)
        {
            component.gameObject.SetActive(true);
        }

        private static void OnReturnToPool(Rigidbody component)
        {
            component.gameObject.SetActive(false);
        }

        private static void OnDestroyPoolObject(Rigidbody component)
        {
            Object.Destroy(component.gameObject);
        }
    }
}

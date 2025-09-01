using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Scripts.Utilities;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

public static class ObjectManager
{
    public static IObjectResolver Resolver;
    
    private static readonly Dictionary<string, LinkedList<GameObject>> ObjectPool = new();
    private static readonly Dictionary<string, Transform> ObjectParent = new();
    private const int MaximumPoolCount = 160; // Constant for maximum pool count for each object pools.

    public static async UniTask CreatePool(object objectReference, int poolSize)
    {
        return; // TODO: Disabled for future usage pool warmup. Currently we are using AddPool method when we need new objects.

        string key = GetKey(objectReference);
        var loadedList = await AssetManager<GameObject>.LoadObjects(objectReference);
        if (!ObjectPool.ContainsKey(key))
            ObjectPool[key] = new LinkedList<GameObject>();
        if (!ObjectParent.ContainsKey(key))
        {
            var parent = new GameObject(objectReference.ToString()).transform;
            ObjectParent[key] = parent;
        }

        foreach (var loadedObject in loadedList)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var createdObject =  (await GetResolver()).Instantiate(loadedObject,
                    Vector3.one * -100,
                    Quaternion.identity,
                    ObjectParent[key]);
                createdObject.SetActive(false);
                ObjectPool[key].AddLast(createdObject);
            }
        }
    }

    public static async UniTask<GameObject> BindTo(this UniTask<GameObject> task, GameObject bindObject,
        object reference)
    {
        var obj = await task;
        if (obj == null)
        {
            LoggerNS.LogError("Cannot bind a null object.");
            return null;
        }

        if (bindObject.TryGetComponent(out DestroyListener destroyListener))
        {
            if (destroyListener.References.Contains(reference))
            {
#if DEBUG_LOGS_ENABLED
                LoggerNS.LogWarning("DestroyListener already exists on bind object.");
#endif
            }
            else
            {
                destroyListener.References.Add(reference);
            }

            return obj;
        }

        var bindObjectDestroyListener = bindObject.AddComponent<DestroyListener>();
        bindObjectDestroyListener.References.Add(reference);

        bindObjectDestroyListener.OnDestroyed += () =>
        {
            AssetManager<GameObject>.ReleaseAsset(bindObjectDestroyListener.References);
        };

        return obj;
    }

    public static void ClearAllPools()
    {
        foreach (var objectPool in ObjectPool)
        {
            foreach (var poolObject in objectPool.Value)
            {
                if (poolObject)
                    Object.Destroy(poolObject.gameObject);
            }
        }

        foreach (var parentPool in ObjectParent)
        {
            if (parentPool.Value)
                Object.Destroy(parentPool.Value.gameObject);
        }

        ObjectPool.Clear();
        ObjectParent.Clear();
    }

    public static void ClearPool(object objectReference)
    {
        string key = GetKey(objectReference);
        if (ObjectPool.TryGetValue(key, out var pool))
        {
            while (pool.Count > 0)
            {
                var poolObject = pool.First.Value;
                if (poolObject != null)
                    Object.Destroy(poolObject);
            }

            ObjectPool.Remove(key);
            if (ObjectParent.TryGetValue(key, out var parent))
                Object.Destroy(parent.gameObject);
        }
    }

    private static async UniTask<GameObject> AddPool(object objectReference, CancellationToken token = default)
    {
        string key = GetKey(objectReference);
        if (!ObjectPool.ContainsKey(key))
            ObjectPool[key] = new LinkedList<GameObject>();
        if (!ObjectParent.ContainsKey(key))
        {
            var parent = new GameObject(objectReference.ToString()).transform;
            ObjectParent[key] = parent;
        }

        var currentPoolSize = ObjectPool[key].Count;
        // if (currentPoolSize >= MaximumPoolCount) // If pool is full, return null. and do not add new objects.
        //     return null;

        GameObject loadedObject = null;

        if (token != default)
        {
            try
            {
                loadedObject = await AssetManager<GameObject>.LoadObject(objectReference)
                    .AttachExternalCancellation(token);
            }
            catch (OperationCanceledException e)
            {
                throw;
            }
        }
        else
        {
            loadedObject = await AssetManager<GameObject>.LoadObject(objectReference);
            if (loadedObject == null)
            {
                LoggerNS.LogError($"Failed to load object for reference: {objectReference} in AddPool method.");
                return null;
            }
        }

        var createdObject =
            (await GetResolver()).Instantiate(loadedObject, Vector3.one * -100, Quaternion.identity, ObjectParent[key]);
        createdObject.SetActive(false);
        ObjectPool[key].AddLast(createdObject);
        return createdObject;
    }

    public static async UniTask<GameObject> GetRandomObject(object objectReference, Vector3? spawnPosition = null,
        Quaternion? spawnRotation = null)
    {
        string key = GetKey(objectReference);
        if (ObjectPool.TryGetValue(key, out var pool) && pool.Count > 0)
        {
            foreach (var poolObject in pool)
            {
                if (!poolObject.activeSelf)
                {
                    if (spawnPosition.HasValue)
                        poolObject.transform.position = spawnPosition.Value;
                    if (spawnRotation.HasValue)
                        poolObject.transform.rotation = spawnRotation.Value;
                    poolObject.SetActive(true);
                    return poolObject;
                }
            }
        }

        LoggerNS.LogWarning($"No inactive objects found in pool for {objectReference}. Adding new objects.");
        var newPoolObject = await AddPool(objectReference);
        newPoolObject.transform.position = spawnPosition ?? Vector3.zero;
        newPoolObject.transform.rotation = spawnRotation ?? Quaternion.identity;
        return newPoolObject;
    }

    public static async UniTask<GameObject> GetObject(object objectReference, Vector3? spawnPosition = null,
        Quaternion? spawnRotation = null, CancellationToken token = default)
    {
        string key = GetKey(objectReference);
        if (ObjectPool.TryGetValue(key, out var pool))
        {
            foreach (var poolObject in pool)
            {
                if (poolObject == null) continue;
                if (poolObject.activeSelf) continue;
                if (spawnPosition.HasValue)
                    poolObject.transform.position = spawnPosition.Value;
                if (spawnRotation.HasValue)
                    poolObject.transform.rotation = spawnRotation.Value;
                poolObject.SetActive(true);
                PoolSortQueue(objectReference, poolObject);
                return poolObject;
            }
        }

#if DEBUG_LOGS_ENABLED
        LoggerNS.LogWarning($"No objects available in pool for {objectReference}. Adding new objects.");
#endif
        var newPoolObject = default(GameObject);
        try
        {
            newPoolObject = await AddPool(objectReference, token);
        }
        catch (OperationCanceledException e)
        {
            //on cancel
            throw;
        }
        newPoolObject.transform.position = spawnPosition ?? Vector3.zero;
        newPoolObject.transform.rotation = spawnRotation ?? Quaternion.identity;
        newPoolObject.SetActive(true);
        return newPoolObject;
    }

    public static async UniTask<GameObject> GetObjectWithoutPool(object objectReference, Vector3? spawnPosition = null,
        Quaternion? spawnRotation = null, Transform parent = null)
    {
        var loadedObject = await AssetManager<GameObject>.LoadObject(objectReference);
        var createdObject =  (await GetResolver()).Instantiate(loadedObject, Vector3.one * -100, Quaternion.identity);
        if (parent != null)
            createdObject.transform.SetParent(parent);
        createdObject.transform.position = spawnPosition ?? Vector3.zero;
        createdObject.transform.rotation = spawnRotation ?? Quaternion.identity;
        createdObject.SetActive(true);
        return createdObject;
    }

    public static bool PoolIsFull(object objectReference)
    {
        string key = GetKey(objectReference);
        if (!ObjectPool.ContainsKey(key))
            return false;
        return ObjectPool.TryGetValue(key, out var pool) &&
            pool.Gen().All(poolObject => poolObject.gameObject.activeSelf);
    }

    private static string GetKey(object objectReference)
    {
        if (objectReference is string str)
        {
            return str;
        }

        if (objectReference is AssetReference assetReference)
        {
            return assetReference.AssetGUID;
        }

        return objectReference.ToString();
    }


    private static void PoolSortQueue(object objectReference, GameObject gameObject)
    {
        string key = GetKey(objectReference);

        if (!ObjectPool.TryGetValue(key, out var value))
            return;
        ObjectPool[key].RemoveFirst();
        ObjectPool[key].AddLast(gameObject);
    }

    private static async UniTask<IObjectResolver> GetResolver()
    {
        if(Resolver != null)
            return Resolver;
        
        await UniTask.WaitUntil(() => Resolver != null);
        return Resolver;
    }

    public static async void DisableObjectAfterTime(GameObject obj, float delayInSeconds)
    {
        if (obj == null)
            return;
        await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds));
        if (obj != null)
            obj.SetActive(false);
    }
}

public class DestroyListener : MonoBehaviour
{
    public event Action OnDestroyed;
    public readonly List<object> References = new();

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}

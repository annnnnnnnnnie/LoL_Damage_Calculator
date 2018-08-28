using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectUtility
{

    private static Dictionary<RecycledGameObject, ObjectPool> GameObjectsPool = new Dictionary<RecycledGameObject, ObjectPool>();


    public static GameObject CustomInstantiate(GameObject prefab, Vector3 position)
    {

        GameObject instance = null;

        var recycleScript = prefab.GetComponent<RecycledGameObject>();
        if (recycleScript != null)
        {
            var pool = GetObjectPool(recycleScript);
            instance = pool.NextGameObject(position).gameObject;
        }
        else
        {
            instance = GameObject.Instantiate(prefab);
            instance.transform.position = position;
        }
        return instance;
    }

    public static GameObject CustomInstantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {

        GameObject instance = null;

        var recycleScript = prefab.GetComponent<RecycledGameObject>();
        if (recycleScript != null)
        {
            var pool = GetObjectPool(recycleScript);
            instance = pool.NextGameObject(position).gameObject;
        }
        else
        {
            instance = GameObject.Instantiate(prefab);
            instance.transform.position = position;
        }
        instance.transform.rotation = rotation;
        return instance;
    }

    public static GameObject CustomInstantiate(GameObject prefab, Transform parentTrasform)
    {

        GameObject instance = null;

        var recycleScript = prefab.GetComponent<RecycledGameObject>();
        if (recycleScript != null)
        {
            var pool = GetObjectPool(recycleScript);
            instance = pool.NextGameObject(parentTrasform).gameObject;
        }
        else
        {
            instance = GameObject.Instantiate(prefab, parentTrasform);
        }
        if(instance.GetComponent<RecycledGameObject>() != null)
        {
            instance.GetComponent<RecycledGameObject>().Initialize();
        }
        return instance;
    }

    public static void CustomDestroy(GameObject gameObject)
    {
        var RecycledGameObject = gameObject.GetComponent<RecycledGameObject>();
        if (RecycledGameObject != null)
        {
            RecycledGameObject.Shutdown();
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

    private static ObjectPool GetObjectPool(RecycledGameObject reference)
    {
        ObjectPool pool = null;

        if (GameObjectsPool.ContainsKey(reference))
        {
            pool = GameObjectsPool[reference];
        }
        else
        {
            var poolContainer = new GameObject(reference.gameObject.name + "ObjectPool");
            pool = poolContainer.AddComponent<ObjectPool>();
            pool.prefab = reference;
            GameObjectsPool.Add(reference, pool);
        }
        return pool;
    }

    public static void ClearObjectPools()
    {
        GameObjectsPool = new Dictionary<RecycledGameObject, ObjectPool>();
    }
}



public class ObjectPool : MonoBehaviour
{

    public RecycledGameObject prefab;
    private List<RecycledGameObject> poolGameObjects = new List<RecycledGameObject>();

    public RecycledGameObject NextGameObject(Vector3 position)
    {
        RecycledGameObject instance = null;

        foreach (var gO in poolGameObjects)
        {
            if (gO.gameObject.activeSelf != true)
            {
                instance = gO;
                instance.transform.position = position;
            }
        }
        if (instance == null)
        {
            instance = CreateGameObject(position);
        }

        instance.Restart();

        return instance;
    }
    public RecycledGameObject NextGameObject(Transform parentTransform)
    {
        RecycledGameObject instance = null;

        foreach (var gO in poolGameObjects)
        {
            if (gO.gameObject.activeSelf != true)
            {
                instance = gO;
                instance.transform.SetParent(parentTransform);
            }
        }
        if (instance == null)
        {
            instance = CreateGameObject(parentTransform);
        }

        instance.Restart();

        return instance;
    }

    private RecycledGameObject CreateGameObject(Vector3 position)
    {
        var cloneGameObject = GameObject.Instantiate(prefab);
        cloneGameObject.transform.position = position;
        cloneGameObject.transform.parent = transform;

        poolGameObjects.Add(cloneGameObject);

        return cloneGameObject;
    }
    private RecycledGameObject CreateGameObject(Transform parentTransform)
    {
        var cloneGameObject = GameObject.Instantiate(prefab, parentTransform);
        poolGameObjects.Add(cloneGameObject);

        return cloneGameObject;
    }



}
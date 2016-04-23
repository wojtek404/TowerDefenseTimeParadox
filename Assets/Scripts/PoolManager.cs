using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour 
{
    public static Dictionary<string, Pool> Pools = new Dictionary<string, Pool>();
    public static Dictionary<string, Properties> Props = new Dictionary<string, Properties>();

    void OnDestroy()
    {
        Pools.Clear();
        Props.Clear();
    }

	public static void Add(Pool pool) 
    {
        if (Pools.ContainsKey(pool.name))
        {
            Debug.LogError("Pool Manager already contains Pool: " + pool.name);
            return;
        }
        Pools.Add(pool.name, pool);
	}

    public static void CreatePool(string poolName, GameObject prefab, int preLoad, bool limit, int maxCount)
    {
        if (Pools.ContainsKey(poolName))
        {
            Debug.LogError("Pool Manager already contains Pool: " + poolName);
            return;
        }
        GameObject newPoolGO = new GameObject(poolName);
        Pool newPool = newPoolGO.AddComponent<Pool>();
        PoolOptions newPoolOptions = new PoolOptions();
        newPoolOptions.prefab = prefab;
        newPoolOptions.preLoad = preLoad;
        newPoolOptions.limit = limit;
        newPoolOptions.maxCount = maxCount;
        newPool._PoolOptions.Add(newPoolOptions);
        newPool.PreLoad();
    }

    public static bool ContainsPool(string poolName)
    {
        if (Pools.ContainsKey(poolName))
            return true;
        else
            return false;
    }

    public static Pool GetPool(string poolName)
    {
        if (!Pools.ContainsKey(poolName))
        {
            Debug.LogError("Pool Manager couldn't find Pool to return: " + poolName);
            return null;
        }
        return Pools[poolName];
    }

    public static void DeactivateAllInstances(string poolName)
    {
        if (!Pools.ContainsKey(poolName))
        {
            Debug.LogError("Pool Manager couldn't find Pool to deactive instances: " + poolName);
            return;
        }
        List<PoolOptions> tempPool = Pools[poolName]._PoolOptions;
        for (int i = 0; i < tempPool.Count; i++)
        {
            for (int j = 0; j < tempPool[i].active.Count; j++)
            {
                PoolManager.Pools[poolName].Despawn(tempPool[i].active[j]);
            }
        }
    }

    public static void DestroyPool(string poolName)
    {
        if (!Pools.ContainsKey(poolName))
        {
            Debug.LogError("Pool Manager couldn't find Pool to destroy: " + poolName);
            return;
        }
        Destroy(Pools[poolName].gameObject);
        Pools.Remove(poolName);
    }

    public static void DestroyAllPools()
    {
        foreach (KeyValuePair<string, Pool> keyValuePair in Pools)
            Destroy(keyValuePair.Value.gameObject);
        Pools.Clear();
        Props.Clear();
    }

    public static void DestroyAllInactive(bool limitToPreLoad)
    {
        foreach (KeyValuePair<string, Pool> keyValuePair in Pools)
            keyValuePair.Value.DestroyUnused(limitToPreLoad);
    }
}

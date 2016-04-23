using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour
{
    [HideInInspector]
    public Transform container;

    public List<PoolOptions> _PoolOptions = new List<PoolOptions>();
    private List<GameObject> _AllActive = new List<GameObject>();

    void Awake()
    {
        PoolManager.Add(this);
        container = transform;
        PreLoad();
    }

    public void PreLoad()
    {
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            for (int i = poolOptions.totalCount; i < poolOptions.preLoad; i++)
            {
                Transform trans = poolOptions.PreLoad(poolOptions.prefab, Vector3.zero, Quaternion.identity).transform;
                trans.SetParent(container);
            }
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            if (poolOptions.prefab == prefab)
            {
                GameObject obj = poolOptions.Activate(position, rotation);
                if (obj == null) return null;
                this._AllActive.Add(obj);
                Transform trans = obj.transform;
                if (trans.parent != container)
                    trans.parent = container;
                return obj;
            }
        }
        Debug.Log("Prefab not found: " + prefab.name + " added to Pool " + this.name);
        PoolOptions newPoolOptions = new PoolOptions();
        newPoolOptions.prefab = prefab;
        _PoolOptions.Add(newPoolOptions);
        GameObject newObj = newPoolOptions.Activate(position, rotation);
        newObj.transform.parent = container;
        this._AllActive.Add(newObj);
        return newObj;
    }

    public void Despawn(GameObject instance)
    {
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            if (poolOptions.active.Contains(instance))
            {
                if (instance.transform.parent != container)
                    instance.transform.parent = container;
                poolOptions.Deactivate(instance);
                this._AllActive.Remove(instance);
                return;
            }
        }
        Debug.LogWarning("Can't despawn - Prefab not found: " + instance.name + " in Pool " + this.name);
    }

    public void Despawn(GameObject instance, float time)
    {
        PoolTimeObject timeObject = new PoolTimeObject();
        timeObject.instance = instance;
        timeObject.time = time;
        StartCoroutine("DespawnInTime", timeObject);
    }

    IEnumerator DespawnInTime(PoolTimeObject timeObject)
    {
        GameObject instance = timeObject.instance;
        yield return new WaitForSeconds(timeObject.time);
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            if (poolOptions.active.Contains(instance))
            {
                if (instance.transform.parent != container)
                    instance.transform.parent = container;
                poolOptions.Deactivate(instance);
                this._AllActive.Remove(instance);
            }
        }
    }

    public void DestroyUnused(bool limitToPreLoad)
    {
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            poolOptions.ClearUpUnused(limitToPreLoad);
        }
    }

    public void DestroyPrefabCount(GameObject prefab, int count)
    {
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions poolOptions = this._PoolOptions[cnt];
            if (poolOptions.prefab == prefab)
            {
                poolOptions.DestroyCount(count);
                return;
            }
        }
        Debug.LogError("Prefab to destroy count of Pool " + this.name + " not found: " + prefab.name);
    }

    public void OnDestroy()
    {
        _AllActive.Clear();
        for (int cnt = 0; cnt < this._PoolOptions.Count; cnt++)
        {
            PoolOptions pool = this._PoolOptions[cnt];
            pool.ClearUp();
        }
    }

    public GameObject this[int index]
    {
        get { return this._AllActive[index]; }
    }

    public int Count
    {
        get { return this._AllActive.Count; }
    }
}

[System.Serializable]
public class PoolTimeObject
{
    public GameObject instance;
    public float time;
}

[System.Serializable]
public class PoolOptions
{
    internal List<GameObject> active = new List<GameObject>();
    internal List<GameObject> inactive = new List<GameObject>();
    public GameObject prefab;
    public int preLoad = 0;
    public bool limit;
    public int maxCount;
    private int index = 0;

    internal GameObject Activate(Vector3 pos, Quaternion rot)
    {
        GameObject obj;
        Transform trans;

        if (inactive.Count != 0)
        {
            obj = inactive[0];
            inactive.RemoveAt(0);
            trans = obj.transform;
        }
        else
        {
            if (limit && active.Count >= maxCount)
                return null;
            obj = (GameObject)Object.Instantiate(prefab, pos, rot);
            trans = obj.transform;
            Rename(trans);
        }
        trans.position = pos;
        trans.rotation = rot;
        active.Add(obj);
        obj.SetActive(true);
        obj.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
        return obj;
    }

    internal void Deactivate(GameObject obj)
    {
        active.Remove(obj);
        inactive.Add(obj);
        obj.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
        obj.SetActive(false);
    }

    internal GameObject PreLoad(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab in pool empty! Please check references.");
            return null;
        }
        GameObject obj = (GameObject)Object.Instantiate(prefab, position, rotation);
        Rename(obj.transform);
        obj.SetActive(false);
        inactive.Add(obj);
        return obj;
    }

    private void Rename(Transform instance)
    {
        instance.name += (index + 1).ToString("#000");
        index++;
    }

    internal int totalCount
    {
        get
        {
            int count = 0;
            count += this.active.Count;
            count += this.inactive.Count;
            return count;
        }
    }

    internal void ClearUp()
    {
        active.Clear();
        inactive.Clear();
    }

    internal void ClearUpUnused(bool limitToPreLoad)
    {
        if (limitToPreLoad)
        {
            for (int i = inactive.Count - 1; i >= preLoad; i--)
            {
                Object.Destroy(inactive[i]);
            }
            if(inactive.Count > preLoad)
                inactive.RemoveRange(preLoad, inactive.Count - preLoad);
        }
        else
        {
            foreach (GameObject obj in inactive)
            {
                Object.Destroy(obj);
            }
            inactive.Clear();
        }
    }

    internal void DestroyCount(int count)
    {
        if (count > inactive.Count)
        {
            Debug.LogWarning("Destroy Count value: " + count + " is greater than inactive Count: " +
                             inactive.Count + ". Destroying all available inactive objects of type: " +
                             prefab.name + ". Use ClearUpUnused(false) instead to achieve the same.");
            ClearUpUnused(false);
            return;
        }
        for (int i = inactive.Count - 1; i >= inactive.Count - count; i--)
        {
            Object.Destroy(inactive[i]);
        }
        inactive.RemoveRange(inactive.Count - count, count);
    }
}

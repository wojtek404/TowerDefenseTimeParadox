using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressMap : MonoBehaviour
{
    public static ProgressMap instance;
    public static Dictionary<int, ProgressMapObject> objDic = new Dictionary<int, ProgressMapObject>();

    void Awake()
    {
        objDic.Clear();
        instance = this;
    }

    public static void AddToMap(GameObject mapObjectPrefab, int objID)
    {
        GameObject newObj = PoolManager.Pools["PM_StartingPoint"].Spawn(mapObjectPrefab,
                            Vector3.zero, Quaternion.identity);
        ProgressMapObject newMapObj = newObj.GetComponent<ProgressMapObject>();
        ProgressMap.objDic.Add(objID, newMapObj);
    }

    public static void CalcInMap(int objID, float curProgress)
    {
        objDic[objID].CalculateProgress(curProgress);
    }

    public static void RemoveFromMap(int objID)
    {
        instance.StartCoroutine("Remove", objID);
    }


    internal IEnumerator Remove(int objID)
    {
        ProgressMapObject pMapObj = objDic[objID];
        objDic.Remove(objID);
        pMapObj.image.sprite = pMapObj.objDeadSprite;
        yield return new WaitForSeconds(2);
        PoolManager.Pools["PM_StartingPoint"].Despawn(pMapObj.gameObject);
    }
}

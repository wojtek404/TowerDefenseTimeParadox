using UnityEngine;
using System.Collections;

public class UseCase : MonoBehaviour
{
    public GameObject prefabToSpawn1;
    public GameObject prefabToSpawn2;
    public GameObject prefabToSpawn3;

    public int instantiateCount;
    public float delay;

    void Start()
    {
        StartCoroutine("Spawn1");
        StartCoroutine("Spawn2");
        StartCoroutine("Spawn3");
    }


    IEnumerator Spawn1()
    {
        for (int i = 0; i < 3; i++)
        {
            PoolManager.Pools["Boxes"].Spawn(prefabToSpawn1, new Vector3(-4, 0, i * 2), Quaternion.identity);
            
            yield return new WaitForSeconds(delay);
        }

        Debug.Log("Spawn finished");

        yield return new WaitForSeconds(1);
        
        for (int i = 0; i < 3; i++)
        {
            GameObject instance = PoolManager.Pools["Boxes"]._PoolOptions[0].active[0];
            PoolManager.Pools["Boxes"].Despawn(instance);
            
            yield return new WaitForSeconds(delay);
        }

        Debug.Log("Despawn finished");
    }

    
    IEnumerator Spawn2()
    {
        for (int i = 0; i < instantiateCount; i++)
        {
            PoolManager.Pools["Boxes"].Spawn(prefabToSpawn2, new Vector3(4, 0, i * 2), Quaternion.identity);
            
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(1);

        for (int i = 0; i < instantiateCount; i++)
        {
            GameObject instance = PoolManager.Pools["Boxes"]._PoolOptions[1].active[0];
            PoolManager.Pools["Boxes"].Despawn(instance);
            
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(1);
        StartCoroutine("Spawn2");
    }

    IEnumerator Spawn3()
    {
        for (int i = 0; i < instantiateCount; i++)
        {
            PoolManager.Pools["Particles"].Spawn(prefabToSpawn3, new Vector3(0, 0, i * 2), Quaternion.identity);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(1);
        StartCoroutine("Spawn3");
    }   
}
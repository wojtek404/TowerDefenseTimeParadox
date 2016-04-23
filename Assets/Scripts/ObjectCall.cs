using UnityEngine;
using System.Collections;


public class ObjectCall : MonoBehaviour
{
    void OnSpawn()
    {
        Debug.Log(gameObject.name + ": I am spawned!");
    }


    void OnDespawn()
    {
        Debug.Log(gameObject.name + ": I am despawned!");
    }
}

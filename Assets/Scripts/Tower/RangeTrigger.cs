using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangeTrigger : MonoBehaviour
{
    private TowerController towerScript;

    void Start()
    {
        towerScript = transform.gameObject.GetComponent<TowerController>();
    }

    void OnTriggerEnter(Collider col)
    {
        GameObject colGO = col.gameObject;

        //skip it the object is not an enemy, or it died already
        if (colGO.layer != SV.enemyLayer || !colGO.activeInHierarchy || PoolManager.Props[colGO.name].health <= 0)
            return;

        towerScript.inRange.Add(colGO);
        colGO.SendMessage("AddTower", towerScript);

        //if an enemy has passed our range and we have an attackable target,
        //start invoking TowerBase's CheckRange() method
        //(StartInvoke() checks if it is running already, so it does not run twice)
        if (towerScript.inRange.Count == 1)
        {
            towerScript.StartInvoke(0f);
        }
    }


    //something has left our area of interest / collider range
    void OnTriggerExit(Collider col)
    {
        //get collided object
        GameObject colGO = col.gameObject;

        //we don't need to check the enemy type again,
        //we look up our inRange list instead and search this gameobject 
        if (towerScript.inRange.Contains(colGO))
        {
            //collided object was added before and recognized as enemy
            //enemy left our radius, remove from in range list
            towerScript.inRange.Remove(colGO);
            //and on the other side, remove our tower from enemy dictionary
            colGO.SendMessage("RemoveTower", towerScript);
        }
    }
}

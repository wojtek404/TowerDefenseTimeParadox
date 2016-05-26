using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangeTrigger : MonoBehaviour
{
    private TowerController towerController;

    void Start()
    {
        towerController = transform.parent.GetComponentInChildren<TowerController>();
    }

    void OnTriggerEnter(Collider col)
    {
        GameObject colGO = col.gameObject;
        if (colGO.layer != SV.enemyLayer || !colGO.activeInHierarchy || PoolManager.Props[colGO.name].health <= 0)
            return;
        towerController.inRange.Add(colGO);
        colGO.SendMessage("AddTower", towerController);
        if (towerController.inRange.Count == 1)
        {
            towerController.StartInvoke(0f);
        }
    }

    void OnTriggerExit(Collider col)
    {
        GameObject colGO = col.gameObject;

        if (towerController.inRange.Contains(colGO))
        {
            towerController.inRange.Remove(colGO);
            colGO.SendMessage("RemoveTower", towerController);
        }
    }
}

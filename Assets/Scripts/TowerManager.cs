using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;
    //[HideInInspector]    
    public List<string> towerNames = new List<string>();
    //[HideInInspector]
    public List<GameObject> towerPrefabs = new List<GameObject>();
    //[HideInInspector]  
    public int sellLoss;
    [HideInInspector] 
    public List<Upgrade> towerUpgrade = new List<Upgrade>();
    [HideInInspector]
    public List<TowerController> towerController = new List<TowerController>();

    void Start()
    {
        instance = this;
        for (int i = 0; i < towerNames.Count; i++)
        {
            if (towerPrefabs[i] == null)
                return;
            GameObject tower = (GameObject)Instantiate(towerPrefabs[i], SV.outOfView, Quaternion.identity);
            tower.name = towerNames[i];
            tower.transform.parent = transform;
            TowerController tController = tower.GetComponentInChildren<TowerController>();
            towerController.Add(tController);
            Upgrade upgrade = tower.GetComponentInChildren<Upgrade>();
            towerUpgrade.Add(upgrade);
            tController.upgrade = upgrade;
            tower.SetActive(false);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;
    [HideInInspector]    
    public List<string> towerNames = new List<string>();
    [HideInInspector]
    public List<GameObject> towerPrefabs = new List<GameObject>();
    [HideInInspector]  
    public int sellLoss;
    [HideInInspector] 
    public List<Upgrade> towerUpgrade = new List<Upgrade>();
    [HideInInspector]
    public List<TowerBase> towerBase = new List<TowerBase>();

    void Start()
    {
        instance = this;
        for (int i = 0; i < towerNames.Count; i++)
        {
            if (towerPrefabs[i] == null)
            {
                Debug.LogWarning("Prefab not set for tower " + (i+1) + " in TowerManager!");
                return;
            }

            GameObject tower = (GameObject)Instantiate(towerPrefabs[i], SV.outOfView, Quaternion.identity);
            tower.name = towerNames[i];
            tower.transform.parent = transform;
            TowerBase tBase = tower.GetComponentInChildren<TowerBase>();
            towerBase.Add(tBase);
            Upgrade upg = tower.GetComponentInChildren<Upgrade>();
            towerUpgrade.Add(upg);
            tBase.upgrade = upg;
            tower.SetActive(false);
        }
    }
}

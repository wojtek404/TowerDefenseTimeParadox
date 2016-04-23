using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Upgrade : MonoBehaviour
{
    [HideInInspector]
    public int curLvl = 0;
    public List<UpgOptions> options = new List<UpgOptions>(3);
    private SphereCollider sphereCol;
    private Transform rangeIndicator;
    private TowerBase towerScript;

    void Start()
    {
        if (options.Count == 0)
        {
            Debug.LogWarning("No upgrade options for tower " + gameObject.name +
                             " assigned! This will cause errors at runtime.");
            return;
        }
        sphereCol = transform.parent.FindChild("RangeTrigger").gameObject.GetComponent<SphereCollider>();
        sphereCol.radius = options[curLvl].radius;
        rangeIndicator = transform.parent.FindChild("RangeIndicator");
        if(rangeIndicator) RangeChange();
        towerScript = gameObject.GetComponent<TowerBase>();
    }

    public void LVLchange()
    {
        curLvl++;
        if (options[curLvl].swapModel != null)
        {
            TowerChange();
            return;
        }
        RangeChange();
        towerScript.CancelInvoke("CheckRange");
        float lastShot = towerScript.lastShot;
        float invokeInSec = options[curLvl].shootDelay + lastShot - Time.time;
        if (invokeInSec < 0) invokeInSec = 0f;
        towerScript.StartInvoke(invokeInSec);
    }

    public void RangeChange()
    {
        if (!sphereCol)
        {
            Start();
            return;
        }
        sphereCol.radius = options[curLvl].radius;
        Vector3 currentSize = rangeIndicator.GetComponent<Renderer>().bounds.size;
        Vector3 scale = rangeIndicator.transform.localScale;
        scale.z = options[curLvl].radius * 2 * scale.z / currentSize.z;
        scale.x = options[curLvl].radius * 2 * scale.x / currentSize.x;
        rangeIndicator.transform.localScale = scale;
    }

    public void TowerChange()
    {
        GameObject newTower = (GameObject)Instantiate(options[curLvl].swapModel, transform.position, transform.rotation);
        newTower.transform.parent = GameObject.Find("Tower Manager").transform;
        TowerBase newTowerBase = newTower.GetComponentInChildren<TowerBase>();
        GameObject subTower = newTowerBase.gameObject;
        newTower.name = newTowerBase.gameObject.name = transform.name;
        Upgrade newUpgrade = subTower.AddComponent<Upgrade>();
        newUpgrade.curLvl = this.curLvl;
        newUpgrade.options = this.options;
        newTowerBase.upgrade = newUpgrade;
        PowerUpManager.ApplyToSingleTower(newTowerBase, newUpgrade);
        newTowerBase.enabled = true;
        newTowerBase.CancelInvoke("CheckRange");
        float lastShot = towerScript.lastShot;
        float invokeInSec = options[curLvl].shootDelay + lastShot - Time.time;
        if (invokeInSec < 0) invokeInSec = 0f;
        newTowerBase.StartInvoke(invokeInSec);
        GUILogic gui = GameObject.Find("GUI").GetComponent<GUILogic>();
        gui.upgrade = newUpgrade;
        for (int i = 0; i < towerScript.inRange.Count; i++)
        {
            PoolManager.Props[towerScript.inRange[i].name].RemoveTower(towerScript);
        }
        if (newTowerBase.turret)
        {
            if (towerScript.turret)
                newTowerBase.turret.rotation = towerScript.turret.rotation;
            else
            {
                Ray ray = new Ray(transform.position + new Vector3(0, 0.5f, 0), -transform.up);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20, SV.gridMask))
                {
                    Transform grid = hit.transform;
                    newTowerBase.turret.rotation = grid.rotation;
                }
            }
            newTowerBase.shotPos.localRotation = Quaternion.identity;
            newTowerBase.turret.GetComponent<TowerRotation>().enabled = true;
        }
        Destroy(transform.parent.gameObject);
    }
}

public enum CostType
{
    intValue,
    floatValue
}

[System.Serializable]
public class UpgOptions
{
    public float[] cost;
    public float radius = 5;
    public float damage = 1;
    public float shootDelay = 3;
    public int targetCount = 1;
    public GameObject swapModel;
}
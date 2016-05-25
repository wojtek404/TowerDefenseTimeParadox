using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Upgrade : MonoBehaviour
{
    [HideInInspector]
    public int curLvl = 0;
    public List<UpgOptions> options = new List<UpgOptions>();
    private SphereCollider sphereCollider;
    private Transform rangeIndicator;
    private TowerController towerController;

    void Start()
    {
        if (options.Count == 0)
        {
            Debug.LogWarning("No upgrade options for tower " + gameObject.name);
            return;
        }
        sphereCollider = transform.parent.FindChild("RangeTrigger").gameObject.GetComponent<SphereCollider>();
        sphereCollider.radius = options[curLvl].radius;
        rangeIndicator = transform.parent.FindChild("RangeIndicator");
        if(rangeIndicator)
            RangeChange();
        towerController = gameObject.GetComponent<TowerController>();
    }

    public void LVLchange()
    {
        curLvl++;
        RangeChange();
        towerController.CancelInvoke("CheckRange");
        float lastShot = towerController.lastShot;
        float invokeInSec = options[curLvl].shootDelay + lastShot - Time.time;
        if (invokeInSec < 0)
            invokeInSec = 0f;
        towerController.StartInvoke(invokeInSec);
    }

    public void RangeChange()
    {
        if (!sphereCollider)
        {
            Start();
            return;
        }
        sphereCollider.radius = options[curLvl].radius;
        Vector3 currentSize = rangeIndicator.GetComponent<Renderer>().bounds.size;
        Vector3 scale = rangeIndicator.transform.localScale;
        scale.z = options[curLvl].radius * 2 * scale.z / currentSize.z;
        scale.x = options[curLvl].radius * 2 * scale.x / currentSize.x;
        rangeIndicator.transform.localScale = scale;
    }
}

[System.Serializable]
public class UpgOptions
{
    public float cost;
    public float radius = 5;
    public float damage = 1;
    public float shootDelay = 3;
    public int targetCount = 1;
}
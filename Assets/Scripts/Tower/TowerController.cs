using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerController : MonoBehaviour {

    public GameObject projectile;
    public GameObject rangeInd;
    public Transform turret;

    [HideInInspector]
    public float lastShot = 0f;

    [HideInInspector]
    public List<GameObject> inRange = new List<GameObject>();

    [HideInInspector]
    public Transform currentTarget;

    [HideInInspector]
    public Upgrade upgrade;

    public Transform projectileSpawnPoint;


    void Awake()
    {
        this.enabled = false;
    }

    void Start () {
        if (turret)
        {
            TowerRotation towerRotation = turret.gameObject.GetComponent<TowerRotation>();
            if (towerRotation)
                towerRotation.towerController = this;
        }
        StartInvoke(0f);
    }

    public void StartInvoke(float delay)
    {
        if (this.enabled == true && !IsInvoking("CheckRange"))
            InvokeRepeating("CheckRange", delay + 0.1f, upgrade.options[upgrade.curLvl].shootDelay);
    }

    void CheckRange()
    {
        if (inRange.Count == 0)
        {
            CancelInvoke("CheckRange");
            currentTarget = null;
            return;
        }
        AutoAttack();
    }

    void AutoAttack()
    {
        lastShot = Time.time;
        List<Transform> targets = GetTargets();
        if (targets.Count == 0)
        {
            CancelInvoke("CheckRange");
            InvokeRepeating("CheckRange", 0.5f, upgrade.options[upgrade.curLvl].shootDelay);
            return;
        }
        for (int i = 0; i < targets.Count; i++)
            InstantiateProjectile(targets[i]);
    }

    public List<Transform> GetTargets()
    {
        int targetCount = upgrade.options[upgrade.curLvl].targetCount;
        List<Transform> targets = new List<Transform>();
        if (inRange.Count <= 0)
            return targets;
        currentTarget = inRange[0].transform;
        for (int i = 0; i < inRange.Count && i < targetCount; i++)
            targets.Add(inRange[i].transform);
        return targets;
    }

    void InstantiateProjectile(Transform target)
    {
        GameObject projectileObj = (GameObject)Object.Instantiate(projectile, projectileSpawnPoint.position, new Quaternion());
        projectileObj.SetActive(true);

        Projectile proj = projectileObj.GetComponent<Projectile>();
        proj.damage = upgrade.options[upgrade.curLvl].damage;
        proj.target = target;
    }
}

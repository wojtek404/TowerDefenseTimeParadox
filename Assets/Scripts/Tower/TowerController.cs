using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerController : MonoBehaviour {

    public GameObject projectile;

    [HideInInspector]
    public List<GameObject> inRange = new List<GameObject>();

    [HideInInspector]
    public Transform currentTarget;

    public float projSpacing = 3;

    public int damage;

    void Start () {
        StartInvoke(0f);
    }

    public void StartInvoke(float delay)
    {
        InvokeRepeating("CheckRange", delay + 0.1f, 1f);
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
        Transform target = GetTarget();
        if (target == null)
        {
            CancelInvoke("CheckRange");
            InvokeRepeating("CheckRange", 0.5f, 1f);
            return;
        }

        InstantiateProjectile(target);
    }

    public Transform GetTarget()
    {
        if (inRange.Count <= 0) return null;
        else return inRange[0].transform;
    }

    void InstantiateProjectile(Transform target)
    {
        Vector3 projectileSpawnPosition = transform.position;
        projectileSpawnPosition.y = 15;
        GameObject projectileObj = (GameObject)Object.Instantiate(projectile, projectileSpawnPosition, new Quaternion());
        projectileObj.SetActive(true);
        Projectile proj = projectileObj.GetComponent<Projectile>();

        proj.damage = damage;

        proj.target = target;

        
    }
}

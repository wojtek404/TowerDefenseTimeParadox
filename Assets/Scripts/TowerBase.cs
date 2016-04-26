using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerBase : MonoBehaviour
{
    public GameObject projectile;
    public Transform shotPos;
    public AudioClip shotSound;
    public GameObject shotFx;
    public AnimationClip idleAnim;
    public AnimationClip shotAnim;
    public GameObject rangeInd;
    public Transform turret;
    public float shotAngle = 0f;
    [HideInInspector]
    public float lastShot = 0f;
    [HideInInspector]
    public List<GameObject> inRange = new List<GameObject>();
    public enum EnemyType
    {
        Ground,
        Air,
        Both,
    }

    public EnemyType myTargets = EnemyType.Both;
    public enum ShootOrder
    {
        FirstIn,
        LastIn,
        Strongest,
        Weakest
    }
    public ShootOrder shootOrder = ShootOrder.FirstIn;
    [HideInInspector]
    public Transform currentTarget;
    public float controlMultiplier = 1;
    public float projSpacing = 3;
    [HideInInspector]
    public Upgrade upgrade;
    private List<UpgOptions> tempOptions;


    void Awake()
    {
        this.enabled = false;
    }


    void Start()
    {
        if (shotPos.localRotation != Quaternion.identity)
            shotPos.localRotation = Quaternion.identity;
        if (turret)
        {
            if(shotAngle == 0)
                Debug.LogWarning("Turret set on '" + gameObject.name + "' but shot angle equals zero!");
            TowerRotation turScript = turret.gameObject.GetComponent<TowerRotation>();
            if (turScript)
                turScript.towerScript = this;
            else
                Debug.LogWarning("Turret set on '" + gameObject.name + "' but no TowerRotation script attached to it!");
        }
        StartInvoke(0f);
    }

    public void StartInvoke(float delay)
    {
        if (this.enabled == true && !IsInvoking("CheckRange") && 
           (!SV.control || SV.control.gameObject != gameObject))
        {
            InvokeRepeating("CheckRange", delay + 0.1f, upgrade.options[upgrade.curLvl].shootDelay);
        }
    }

    void CheckRange()
    {
        if (inRange.Count == 0)
        {
            CancelInvoke("CheckRange");
            currentTarget = null;
            if (idleAnim) GetComponent<Animation>().Play(idleAnim.name);
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
        ShotEffect();
    }

    public List<Transform> GetTargets()
    {
        int targetCount = upgrade.options[upgrade.curLvl].targetCount;
        List<Transform> targets = new List<Transform>();
        if (inRange.Count <= 0) return targets;
        switch (shootOrder)
        {
            case ShootOrder.FirstIn:
                currentTarget = inRange[0].transform;
                for (int i = 0; i < inRange.Count; i++)
                {
                    if (targets.Count == targetCount)
                        break;
                    Transform enemy = inRange[i].transform;
                    if (!turret || (turret && CheckAngle(enemy)))
                        targets.Add(enemy);
                }
                break;
            case ShootOrder.LastIn:
                currentTarget = inRange[inRange.Count - 1].transform;
                for (int i = 0; i < inRange.Count; i++)
                {
                    if (targets.Count == targetCount)
                        break;
                    Transform enemy = inRange[inRange.Count - i - 1].transform;
                    if (!turret || (turret && CheckAngle(enemy)))
                        targets.Add(enemy);
                }
                break;
            case ShootOrder.Strongest:
            case ShootOrder.Weakest:
                List<Properties> allProperties = new List<Properties>();
                for (int i = 0; i < inRange.Count; i++)
                {
                    allProperties.Add(inRange[i].GetComponent<Properties>());
                }
                switch (shootOrder)
                {
                    case ShootOrder.Strongest:
                        for (int i = 0; i < inRange.Count; i++)
                        {
                            float highestHealth = 0;
                            Transform strongest = null;
                            int index = 0;
                            for (int j = 0; j < allProperties.Count; j++)
                            {
                                float enemyHealth = allProperties[j].health;
                                if (enemyHealth > highestHealth)
                                {
                                    strongest = allProperties[j].transform;
                                    highestHealth = enemyHealth;
                                    index = j;
                                }
                            }
                            targets.Add(strongest);
                            allProperties.RemoveAt(index);
                        }
                        break;
                    case ShootOrder.Weakest:
                        for (int i = 0; i < inRange.Count; i++)
                        {
                            float lowestHealth = float.MaxValue;
                            Transform weakest = null;
                            int index = 0;
                            for (int j = 0; j < allProperties.Count; j++)
                            {
                                float enemyHealth = allProperties[j].health;
                                if (enemyHealth < lowestHealth)
                                {
                                    weakest = allProperties[j].transform;
                                    lowestHealth = enemyHealth;
                                    index = j;
                                }
                            }
                            targets.Add(weakest);
                            allProperties.RemoveAt(index);
                        }
                        break;
                }
                currentTarget = targets[0].transform;
                if (turret)
                {
                    Transform[] temp = new Transform[targets.Count];
                    targets.CopyTo(temp);
                    foreach (Transform enemy in temp)
                    {
                        if (!CheckAngle(enemy))
                        {
                            targets.Remove(enemy);
                        }
                    }
                }
                if (targets.Count > targetCount)
                {
                    targets.RemoveRange(targetCount, targets.Count - targetCount);
                }
                break;
        }
        return targets;
    }


    bool CheckAngle(Transform enemy)
    {
        Vector2 targetVec = new Vector2(enemy.position.x, enemy.position.z);
        Vector2 shootVec = new Vector2(shotPos.position.x, shotPos.position.z);
        Vector2 targetDir = targetVec - shootVec;
        Vector2 forward = new Vector2(shotPos.forward.x, shotPos.forward.z);
        float angle = Vector2.Angle(forward, targetDir);
        if (angle <= shotAngle / 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void ShotEffect()
    {
        if (shotAnim)
            GetComponent<Animation>().Play(shotAnim.name);
        if (shotFx)
        {
            Quaternion rot = Quaternion.identity;
            if (turret) rot = turret.rotation;
            GameObject fx = PoolManager.Pools["Particles"].Spawn(shotFx, shotPos.position, rot);
            fx.transform.parent = shotPos;
        }
    }

    public void SelfAttack(Vector3 targetPos)
    {
        int targets = upgrade.options[upgrade.curLvl].targetCount;
        if (targets >= 1 && targets % 2 != 0)
        {
            InstantiateProjectile(targetPos);
        }
        Vector3 direction = (targetPos - transform.position).normalized;
        direction = Quaternion.AngleAxis(-90, Vector3.up) * direction;
        direction.y = 0f;
        if (targets > 1)
        {
            for (int i = 0; i < targets / 2; i++)
            {
                InstantiateProjectile(targetPos + (-direction * projSpacing * (i + 1)));
            }
            for (int i = 0; i < targets / 2; i++)
            {
                InstantiateProjectile(targetPos + (direction * projSpacing * (i + 1)));
            }
        }
        ShotEffect();
    }

    void InstantiateProjectile(Transform target)
    {
        GameObject ProjectileInst = PoolManager.Pools["Projectiles"].Spawn(projectile, shotPos.position, shotPos.rotation);
        Projectile proj = ProjectileInst.GetComponent<Projectile>();
        proj.damage = upgrade.options[upgrade.curLvl].damage;
        proj.target = target;
    }

    void InstantiateProjectile(Vector3 targetPos)
    {
        GameObject ProjectileInst = PoolManager.Pools["Projectiles"].Spawn(projectile, shotPos.position, shotPos.rotation);
        Projectile proj = ProjectileInst.GetComponent<Projectile>();
        proj.damage = upgrade.options[upgrade.curLvl].damage;
        proj.followEnemy = false;
        proj.endPos = targetPos;
        proj.damage = proj.damage * controlMultiplier;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (this.enabled == false)
            return;
        if (inRange.Count > 0)
        {
            foreach (GameObject enemy in inRange)
            {
                Gizmos.DrawLine(shotPos.position, enemy.transform.position);
            }
        }
        if (!turret)
       	return;
		Gizmos.color = Color.green;
        float currentRadius = upgrade.options[upgrade.curLvl].radius;
        float halfShootAngle = shotAngle / 2;
        Vector3 forwarddir = turret.TransformDirection(Vector3.forward) * currentRadius;
        Gizmos.DrawRay(turret.position, forwarddir);
        Quaternion locRight = Quaternion.AngleAxis(halfShootAngle, Vector3.up);
        Vector3 rightDir = turret.TransformDirection(locRight * Vector3.forward) * currentRadius;
        Gizmos.DrawRay(turret.position, rightDir);
        Quaternion locLeft = Quaternion.AngleAxis(halfShootAngle, Vector3.down);
        Vector3 leftDir = turret.TransformDirection(locLeft * Vector3.forward) * currentRadius;
        Gizmos.DrawRay(turret.position, leftDir);
    }

    void OnMouseEnter()
    {
        if (!SV.selection && !SV.showUpgrade && !SV.control && !SV.showExit)
        rangeInd.GetComponent<Renderer>().enabled = true;
    }

    void OnMouseExit()
    {
        if (!SV.selection && !SV.showUpgrade && !SV.control && !SV.showExit)
        rangeInd.GetComponent<Renderer>().enabled = false;
    }    
}

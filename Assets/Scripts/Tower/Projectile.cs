using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    public Transform target; 
    [HideInInspector]
    public float damage = 0f;
    public bool followEnemy = true;   
    private bool cacheFollowEnemy;

    public float speed = 0f;
    private Rigidbody myRigidbody;
    public bool singleDMG = true;
    public float timeToLast = 0;
    public Explosion explosion;

    [HideInInspector]
    public Vector3 startPos = Vector3.zero;
    [HideInInspector]
    public Vector3 endPos = Vector3.zero;
    private bool executed;

    void Start()
    {
        StartCoroutine(OnSpawn());
    }

    void Init()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    IEnumerator OnSpawn()
    {
        if (!myRigidbody) Init();
        executed = false;
        startPos = transform.position;
        cacheFollowEnemy = followEnemy;
        yield return new WaitForEndOfFrame();
        if (target && !followEnemy)
            endPos = target.position;
        transform.LookAt(endPos);

        while (!executed) {
            yield return StartCoroutine("Straight");
        }
                    

    }

    void Straight()
    {
        if (followEnemy && target && target.gameObject.activeInHierarchy)
        {
            endPos = target.position;
            transform.LookAt(endPos);
        }
        transform.Translate(Vector3.forward * (speed * 10) * Time.deltaTime * Time.timeScale);

        if (Vector3.Distance(transform.position, startPos) > Vector3.Distance(startPos, endPos) + 0.5f)
        {
            StartCoroutine("HandleImpact");
        }
    }

    IEnumerator OnTriggerEnter(Collider hitCol)
    {
        if (executed) yield break;
        GameObject colGO = hitCol.gameObject;
        if (colGO.layer == SV.enemyLayer)
        {
            if (!target)
                target = colGO.transform;
            else if (followEnemy && target != colGO.transform)
                yield break;
            if (singleDMG)
            {
                Properties targetProp = PoolManager.Props[colGO.name];
                targetProp.Hit(damage);
            }
        }
        else if (colGO.layer != SV.worldLayer)
            yield break;
        yield return StartCoroutine("HandleImpact");        
    }

    IEnumerator HandleImpact()
    {
        executed = true;
        if (explosion.enabled)
        {
            yield return StartCoroutine("Explosion");
        }
        
        if (timeToLast > 0)
        {
            float timer = Time.time + timeToLast;
            while (Time.time < timer)
            {
               if (target && PoolManager.Props[target.name].health <= 0)
                    break;

               yield return null;
            }
        }
        Destroy(gameObject);
    }

    void Explosion()
    {
        if (explosion.fx)
            PoolManager.Pools["Particles"].Spawn(explosion.fx, transform.position, Quaternion.identity);

        List<Collider> cols = new List<Collider>(Physics.OverlapSphere(transform.position, explosion.radius, SV.enemyMask));
        foreach (Collider col in cols)
        {
            Properties targetProp = PoolManager.Props[col.name];
            targetProp.Hit(damage * explosion.factor);
        }
    }
}

[System.Serializable]
public class Explosion
{
    public bool enabled;
    public GameObject fx;
    public float radius;
    public float factor;  
}
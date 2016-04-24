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

    public enum FireMode
    {
        straight,
        lob,
    }

    public FireMode flyMode = FireMode.straight;
    public float lobHeight = 0f;
    public float speed = 0f;
    public bool continuousCollision = false;
    private float minimumBounds;
    private float sqrMinimumBounds;
    private Vector3 previousPosition;
    private Rigidbody myRigidbody;
    public AudioClip impactSound;
    public bool singleDMG = true;
    public float timeToLast = 0;
    public Explosion explosion;
    public Burn burn;
    public Slow slow;

    [HideInInspector]
    public Vector3 startPos = Vector3.zero;
    private float time = 0f;
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
        minimumBounds = GetComponent<Collider>().bounds.size.z;
        sqrMinimumBounds = minimumBounds * minimumBounds;
    }

    IEnumerator OnSpawn()
    {
        if (!myRigidbody) Init();
        executed = false;
        startPos = transform.position;
        previousPosition = myRigidbody.position;
        time = 0f;
        cacheFollowEnemy = followEnemy;
        yield return new WaitForEndOfFrame();
        if (target && !followEnemy)
            endPos = target.position;
        transform.LookAt(endPos);
        switch (flyMode)
        {
            case FireMode.straight:
                while (!executed)
                    yield return StartCoroutine("Straight");
                break;
            case FireMode.lob:
                while (!executed)
                    yield return StartCoroutine("Lob");
                break;
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
        if (continuousCollision)
            if(ContinuousCollision()) return;
        if (Vector3.Distance(transform.position, startPos) > Vector3.Distance(startPos, endPos) + 0.5f)
        {
            StartCoroutine("HandleImpact");
        }
    }

    void Lob()
    {
        if (followEnemy && target && target.gameObject.activeInHierarchy)
            endPos = target.position;
        else if (target)
            target = null;
        time += 0.2f * Time.deltaTime;
        float cTime = time * speed;     
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, cTime);
        currentPos.y += lobHeight * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);
        if (currentPos != transform.position)
        {
            Quaternion rotation = Quaternion.LookRotation(currentPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 6.0f);
        }
        transform.position = currentPos;
        if (continuousCollision)
            if (ContinuousCollision()) return;

        if (Vector3.Distance(transform.position, endPos) < 0.1f)
        {
            StartCoroutine("HandleImpact");
        }
    }


    bool ContinuousCollision()
    {
        Vector3 movementThisStep = myRigidbody.position - previousPosition;
        float movementSqrMagnitude = movementThisStep.sqrMagnitude;
        if (movementSqrMagnitude > sqrMinimumBounds)
        {
            float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
            RaycastHit[] hitInfos = Physics.RaycastAll(previousPosition, movementThisStep, movementMagnitude, SV.enemyMask);
            for(int i = 0; i < hitInfos.Length; i++)
                StartCoroutine("OnTriggerEnter", hitInfos[i].collider);
            previousPosition = myRigidbody.position;
            if (hitInfos.Length > 0) return true;
        }
        previousPosition = myRigidbody.position;
        return false;
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
        AudioManager.Play(impactSound, transform.position);
        if (explosion.enabled)
        {
            yield return StartCoroutine("Explosion");
        }
        if (burn.enabled)
        {
            yield return StartCoroutine("Burn");
        }
        if (slow.enabled)
        {
            yield return StartCoroutine("Slow");
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
        PoolManager.Pools["Projectiles"].Despawn(gameObject);
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

    void Burn()
    {
        float[] vars = new float[3];
        vars[0] = damage * burn.factor;
        vars[1] = burn.time;
        vars[2] = burn.frequency;
        List<Collider> cols = new List<Collider>();

        if (burn.area)
            cols = new List<Collider>(Physics.OverlapSphere(transform.position, burn.radius, SV.enemyMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());

        foreach (Collider col in cols)
        {
            Transform colTrans = col.transform;
            Properties targetProp = PoolManager.Props[colTrans.name];
            if (!targetProp.IsAlive())
                continue;

            if(!burn.stack)
                targetProp.StopCoroutine("DamageOverTime");
            targetProp.StartCoroutine("DamageOverTime", vars);
            if (!burn.fx) continue;
            Transform dotEffect = null;
            foreach (Transform child in colTrans)
            {
                if (child.name.Contains(burn.fx.name))
                {
                    dotEffect = child;
                }
            }
            if (!dotEffect)
            {
                dotEffect = PoolManager.Pools["Particles"].Spawn(burn.fx, colTrans.position, Quaternion.identity).transform;
                dotEffect.parent = colTrans;
            }
        }
    }

    void Slow()
    {
        if (slow.fx)
            PoolManager.Pools["Particles"].Spawn(slow.fx, transform.position, Quaternion.identity);
        List<Collider> cols = new List<Collider>();

        if (slow.area)
            cols = new List<Collider>(Physics.OverlapSphere(transform.position, slow.radius, SV.enemyMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());
        foreach (Collider col in cols)
        {
            Properties targetProp = PoolManager.Props[col.name];
            targetProp.Slow(slow.time, slow.factor);
        }
    }

    void OnDespawn()
    {
        target = null;
        followEnemy = cacheFollowEnemy;
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

[System.Serializable]
public class Slow
{
    public bool enabled;
    public GameObject fx;
    public bool area;
    public float radius;
    public float time;
    public float factor;   
}


[System.Serializable]
public class Burn
{
    public bool enabled;
    public GameObject fx;
    public bool area;
    public float radius;
    public float time;
    public int frequency;
    public float factor;
    public bool stack;
}

[System.Serializable]
public class Weakening
{
    public bool enabled;
    public GameObject fx;
    public bool area;
    public float radius;
    public enum Type
    {
        healthOnly,
        shieldOnly,
        all
    }
    public Type type = Type.all;
    public float factor;
}
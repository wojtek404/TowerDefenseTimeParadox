using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Properties : MonoBehaviour
{
    public float health = 100;        //punkty zycia
    [HideInInspector]
    public float maxhealth;          //max zycie
    public Slider healthbar;        //pasek zycia
    public Shield shield;       //wartosc tarczy/zbroi
    private RectTransform barParentTrans;

    public GameObject hitEffect;    //efekt czasteczkowy uderzenia
    public GameObject deathEffect;  //efeket czasteczkowy smierci
    public AudioClip hitSound;   //dzwiek uderzenia
    public AudioClip deathSound; //dzwiek smierci
    [HideInInspector]
    public Animation anim;
    public AnimationClip walkAnim;  //animacja ruchu
    public AnimationClip dieAnim;  //animacja smierci
	public AnimationClip successAnim; //animacja w przypadku powodzenia

    public float[] pointsToEarn;  //ile moze ukrasc skarbu
    public int damageToDeal = 1;    //ew. ile punktow zycia naszego zamku moze za razem zabrac
    [HideInInspector]
	public TweenMove myMove; //obiekt ruchu
    [HideInInspector]
    public List<TowerBase> nearTowers = new List<TowerBase>();
    private float time;


    void Start()
    {
        PoolManager.Props.Add(gameObject.name, this);
        myMove = gameObject.GetComponent<TweenMove>();
        anim = gameObject.GetComponentInChildren<Animation>();
        myMove.maxSpeed = myMove.speed;
        myMove.pMapProperties.myID = gameObject.GetInstanceID();
        if (healthbar)
            barParentTrans = healthbar.transform.parent.GetComponent<RectTransform>();
        else if (shield.bar)
            barParentTrans = shield.bar.transform.parent.GetComponent<RectTransform>();
    }

    IEnumerator OnSpawn()
    {
        yield return new WaitForEndOfFrame();
        if (walkAnim)
        {
            anim[walkAnim.name].time = Random.Range(0f, anim[walkAnim.name].length);
            anim.Play(walkAnim.name);
        }
        if (barParentTrans) 
        {
            RectTransform[] rects = GetComponentsInChildren<RectTransform>();
            for (int i = 0; i < rects.Length; i++)
                rects[i].anchoredPosition = Vector2.zero;
        }
        maxhealth = health;
        shield.maxValue = shield.value;
    }

    void LateUpdate()
    {
        Quaternion rot = Camera.main.transform.rotation;
        if (barParentTrans)
            barParentTrans.rotation = rot;
    }


    public void AddTower(TowerBase tower)
    {
        nearTowers.Add(tower);
    }

    public void RemoveTower(TowerBase tower)
    {
        nearTowers.Remove(tower);
    }

    public void Hit(float damage)
    {
        if (!IsAlive()) return;
        if (shield.enabled)
        {
            damage = HitShield(damage);
            StopCoroutine("RegenerateShield");
            StartCoroutine("RegenerateShield");
        }
        health -= damage;
        if (health > 0 && Time.time > time + 2)
            OnHit();
        else if (health <= 0)
        {
            for (int i = 0; i < pointsToEarn.Length; i++)
                GameHandler.SetResources(i, pointsToEarn[i]);
            GameHandler.EnemyWasKilled();
            OnDeath();
        }
        SetUnitFrame();
    }

	
	public void SetUnitFrame()
    {
        if (healthbar)
            healthbar.value = health / maxhealth;
        if (shield.bar)
            shield.bar.value = shield.value / shield.maxValue;
    }

    float HitShield(float damage)
    {
        float currentValue = shield.value;
        shield.value -= damage;
        if (shield.value < 0) shield.value = 0;
        damage -= currentValue;
        if (damage < 0) damage = 0;
        return damage;
    }

    IEnumerator RegenerateShield()
    {
        if (shield.regenValue <= 0) yield break;
        yield return new WaitForSeconds(shield.delay);
        while (shield.value < shield.maxValue)
        {
            if (shield.regenType == TDValue.fix)
                shield.value += shield.regenValue;
            else
                shield.value += Mathf.Round(shield.maxValue * shield.regenValue * 100f) / 100f;
            shield.value = Mathf.Clamp(shield.value, 0f, shield.maxValue);
            shield.bar.value = shield.value / shield.maxValue;
            yield return new WaitForSeconds(shield.interval);
        }
    }
	
    void OnHit()
    {
        time = Time.time;
        if (hitEffect)
            PoolManager.Pools["Particles"].Spawn(hitEffect, transform.position, hitEffect.transform.rotation);
    }

    void OnDeath()
    {
        myMove.StopAllCoroutines();
        myMove.CancelInvoke("Accelerate");
        myMove.tween.Kill();
        StopAllCoroutines();
        StartCoroutine("RemoveEnemy");
    }

    public bool IsAlive()
    {
        if (health <= 0 || !gameObject.activeInHierarchy || (dieAnim && anim.IsPlaying(dieAnim.name))
            || myMove.tween == null || myMove.tween.isComplete)
            return false;
        else
            return true;
    }

	
    public void Slow(float slowTime, float slowFactor)
    {
        if (health <= 0 || gameObject.activeInHierarchy == false
            || (dieAnim && anim.IsPlaying(dieAnim.name)))
            return;

        float newSpeed = myMove.maxSpeed * slowFactor;
        if (myMove.speed >= newSpeed)
        {
            myMove.speed = newSpeed;
            myMove.Slow();
            myMove.CancelInvoke("Accelerate");
            myMove.Invoke("Accelerate", slowTime);
        }
    }

    public IEnumerator DamageOverTime(float[] vars)
    {
        float delay = vars[1] / (vars[2]-1);
        float dotDmg = vars[0] / vars[2];
        Hit(dotDmg);
        for (int i = 0; i < vars[2]-1; i++)
        {
            yield return new WaitForSeconds(delay);
            Hit(dotDmg);
        }
        foreach (Transform child in transform)
        {
            if (child.name.Contains("(Clone)"))
                PoolManager.Pools["Particles"].Despawn(child.gameObject);
        }
    }

    void PathEnd()
    {
        GameHandler.DealDamage(damageToDeal);
        OnDeath();
    }


    IEnumerator RemoveEnemy()
    {
        for (int i = 0; i < nearTowers.Count; i++)
            nearTowers[i].inRange.Remove(gameObject);
        nearTowers.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.Contains("(Clone)"))
                PoolManager.Pools["Particles"].Despawn(child.gameObject);
        }
        if (myMove.pMapProperties.enabled)
        {
            myMove.CancelInvoke("ProgressCalc");
            ProgressMap.RemoveFromMap(myMove.pMapProperties.myID);
        }
        if (health <= 0)
        {
            if (deathEffect)
                PoolManager.Pools["Particles"].Spawn(deathEffect, transform.position, Quaternion.identity);

            if (dieAnim)
            {
                anim.Play(dieAnim.name);
                yield return new WaitForSeconds(dieAnim.length);
            }
        }
        else
        {
            if (successAnim)
            {
                anim.Play(successAnim.name);
                yield return new WaitForSeconds(successAnim.length);
            }
        }

        health = maxhealth;
        shield.value = shield.maxValue;
        if (healthbar)
            healthbar.value = 1;
        if (shield.bar)
            shield.bar.value = 1;
        PoolManager.Pools["Enemies"].Despawn(gameObject);
    }
	
    void OnDestroy()
    {
        PoolManager.Props.Remove(gameObject.name);
    }
}


[System.Serializable]
public class Shield
{
    public bool enabled = false;        //aktywna
    public Slider bar;                //pasek zbroi
    public float value = 10;            //wartosc obrazen
    [HideInInspector]
    public float maxValue;
    public TDValue regenType = TDValue.fix;
    public float regenValue = 0;
    public float interval = 1;
    public float delay = 1;
}
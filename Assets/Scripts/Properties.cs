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

    public GameObject hitEffect;    //efekt czasteczkowy uderzenia
    public GameObject deathEffect;  //efeket czasteczkowy smierci
    public AudioClip hitSound;   //dzwiek uderzenia
    public AudioClip deathSound; //dzwiek smierci
    [HideInInspector]
    public Animation anim;
    public AnimationClip walkAnim;  //animacja ruchu
    public AnimationClip dieAnim;  //animacja smierci
	public AnimationClip successAnim; //animacja w przypadku powodzenia

    private Image fillImage;
    private Color fullHealthColor = Color.green;
    private Color zeroHealthColor = Color.red;

    public float pointsToEarn;  //ile daje resources po smierci
    public int damageToDeal = 1;    //ew. ile punktow zycia naszego zamku moze za razem zabrac
    [HideInInspector]
	public TweenMove myMove; //obiekt ruchu
    [HideInInspector]
    public List<TowerController> nearTowers = new List<TowerController>();
    private float time;

    


    void Start()
    {
        fillImage = healthbar.transform.FindChild("Fill Area").FindChild("Fill").gameObject.GetComponent<Image>();
        float adjustedScale = 1 / healthbar.transform.lossyScale.x;
        healthbar.transform.localScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
        adjustedScale = 1 / fillImage.transform.lossyScale.x;
        fillImage.transform.localScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);

        PoolManager.Props.Add(gameObject.name, this);
        myMove = gameObject.GetComponent<TweenMove>();
        anim = gameObject.GetComponentInChildren<Animation>();
        myMove.maxSpeed = myMove.speed;
        myMove.pMapProperties.myID = gameObject.GetInstanceID();
    }

    IEnumerator OnSpawn()
    {
        yield return new WaitForEndOfFrame();
        if (walkAnim)
        {
            anim[walkAnim.name].time = Random.Range(0f, anim[walkAnim.name].length);
            anim.Play(walkAnim.name);
        }
        maxhealth = health;
        SetHealthUI();
    }


    public void AddTower(TowerController tower)
    {
        nearTowers.Add(tower);
    }

    public void RemoveTower(TowerController tower)
    {
        nearTowers.Remove(tower);
    }

    public void Hit(float damage)
    {
        if (!IsAlive()) return;
        health -= damage;
        if (health > 0 && Time.time > time + 2)
            OnHit();
        else if (health <= 0)
        {
            GameHandler.SetResources(pointsToEarn);
            GameHandler.EnemyWasKilled();
            OnDeath();
        }
        SetHealthUI();
    }

    private void SetHealthUI()
    {
        healthbar.value = health / maxhealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, health / maxhealth);
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


    public IEnumerator RemoveEnemy()
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
        if (healthbar)
            healthbar.value = 1;
        PoolManager.Pools["Enemies"].Despawn(gameObject);
    }
	
    void OnDestroy()
    {
        PoolManager.Props.Remove(gameObject.name);
    }
}
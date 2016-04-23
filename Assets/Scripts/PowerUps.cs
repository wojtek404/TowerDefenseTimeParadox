using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BattlePowerUp
{
    public string name = "";
    public string description = "";
    public bool enabled = true;
    public float startDelay;
    public float cooldown;
    public Text timerText;
    public int targets = 1;
    public AudioClip sound;
    public GameObject fx;
    public GameObject indicator;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Vector3 position;

    public void InstantiateFX()
    {
        if (fx)
            PoolManager.Pools["Particles"].Spawn(fx, position, Quaternion.identity);
        if (sound)
            AudioManager.Play(sound, position);
    }

    public List<Collider> SortByDistance(List<Collider> cols)
    {
        List<Collider> sortedCols = new List<Collider>();
        if (cols.Count == 0) return sortedCols;
        List<Collider> tempCols = new List<Collider>();
        for (int i = 0; i < cols.Count; i++)
            tempCols.Add(cols[i]);
        for (int i = 0; i < cols.Count; i++)
        {
            float nearest = float.MaxValue;
            Collider col = null;
            int index = 0;
            for (int j = 0; j < tempCols.Count; j++)
            {
                if (tempCols[j].gameObject.layer == SV.enemyLayer
                   && !PoolManager.Props[tempCols[j].gameObject.name].IsAlive())
                    continue;
                float distance = (tempCols[j].transform.position - position).sqrMagnitude;
                if (distance < nearest)
                {
                    col = tempCols[j];
                    nearest = distance;
                    index = j;
                }
            }
            if (col == null) break;
            sortedCols.Add(col);
            tempCols.RemoveAt(index);
        }
        if (sortedCols.Count > targets)
            sortedCols.RemoveRange(targets, sortedCols.Count - targets);
        return sortedCols;
    }

    public virtual bool CheckRequirements()
    { return false; }

    public virtual float GetMaxRadius()
    { return 0f; }
}

[System.Serializable]
public class OffensivePowerUp : BattlePowerUp
{
    public TDValue damageType;
    public float damage;
    public bool singleTarget = false;
    public Explosion explosion = new Explosion();
    public Burn burn = new Burn();
    public Slow slow = new Slow();
    public Weakening weaken = new Weakening();

    public override bool CheckRequirements()
    {
        if (!base.enabled || singleTarget && !base.target
            || singleTarget && base.target && !PoolManager.Props[base.target.name].IsAlive())
        return false;
        else return true;
    }

    public IEnumerator Explosion()
    {
        List<Collider> cols = new List<Collider>(Physics.OverlapSphere(position, explosion.radius, SV.enemyMask));
        cols = SortByDistance(cols);
        for (int i = 0; i < cols.Count; i++)
        {
            Properties targetProp = PoolManager.Props[cols[i].name];
            if (damageType == TDValue.fix)
                targetProp.Hit(damage * explosion.factor);
            else
                targetProp.Hit(targetProp.maxhealth * damage * explosion.factor);
            if (explosion.fx)
                PoolManager.Pools["Particles"].Spawn(explosion.fx, cols[i].transform.position, Quaternion.identity);
        }
        yield return null;
    }

    public IEnumerator Burn()
    {
        List<Collider> cols = new List<Collider>();
        if (burn.area)
            cols = new List<Collider>(Physics.OverlapSphere(position, burn.radius, SV.enemyMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());
        cols = SortByDistance(cols);
        for (int i = 0; i < cols.Count; i++)
        {
            Transform colTrans = cols[i].transform;
            Properties targetProp = PoolManager.Props[colTrans.name];
            if (!targetProp.IsAlive())
                continue;
            float[] vars = new float[3];
            if (damageType == TDValue.fix)
                vars[0] = damage * burn.factor;
            else
                targetProp.Hit(targetProp.maxhealth * damage * burn.factor);
            vars[1] = burn.time;
            vars[2] = burn.frequency;
            if (!burn.stack)
                targetProp.StopCoroutine("DamageOverTime");
            targetProp.StartCoroutine("DamageOverTime", vars);
            if (!burn.fx) continue;
            Transform dotEffect = null;
            foreach (Transform child in colTrans)
            {
                if (child.name.Contains(burn.fx.name))
                    dotEffect = child;
            }
            if (!dotEffect)
            {
                dotEffect = PoolManager.Pools["Particles"].Spawn(burn.fx, colTrans.position, Quaternion.identity).transform;
                dotEffect.parent = colTrans;
            }
        }

        yield return null;
    }

    public IEnumerator Slow()
    {
        List<Collider> cols = new List<Collider>();
        if (slow.area)
            cols = new List<Collider>(Physics.OverlapSphere(position, slow.radius, SV.enemyMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());
        cols = SortByDistance(cols);
        for (int i = 0; i < cols.Count; i++)
        {
            Properties targetProp = PoolManager.Props[cols[i].name];
            targetProp.Slow(slow.time, slow.factor);

            if (slow.fx)
                PoolManager.Pools["Particles"].Spawn(slow.fx, cols[i].transform.position, Quaternion.identity);
        }
        yield return null;
    }

    public IEnumerator Weaken()
    {
        List<Collider> cols = new List<Collider>();

        if (weaken.area)
            cols = new List<Collider>(Physics.OverlapSphere(position, weaken.radius, SV.enemyMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());
        cols = SortByDistance(cols);
        for (int i = 0; i < cols.Count; i++)
        {
            if (weaken.fx)
                PoolManager.Pools["Particles"].Spawn(weaken.fx, cols[i].transform.position, Quaternion.identity);
            Properties targetProp = PoolManager.Props[cols[i].name];
            float healthValue = targetProp.health;
            float shieldValue = 0;
            if (targetProp.shield.enabled)
                shieldValue = targetProp.shield.value;
            switch (weaken.type)
            {
                case Weakening.Type.shieldOnly:
                    if (shieldValue > 0)
                        targetProp.Hit(shieldValue * weaken.factor);
                    break;
                case Weakening.Type.healthOnly:
                    targetProp.Hit(shieldValue + healthValue * weaken.factor);
                    targetProp.shield.value = shieldValue;
                    targetProp.SetUnitFrame();
                    break;
                case Weakening.Type.all:
                    targetProp.Hit(shieldValue + healthValue * weaken.factor);
                    targetProp.shield.value = shieldValue * (1f - weaken.factor);
                    targetProp.SetUnitFrame();
                    break;
            }
        }
        yield return null;
    }

    public override float GetMaxRadius()
    {
        if (singleTarget) return 0f;
        
        float value = 0f;
        if (explosion.enabled && explosion.radius > value)
            value = explosion.radius;
        if (burn.enabled && burn.area && burn.radius > value)
            value = burn.radius;
        if (slow.enabled && slow.area && slow.radius > value)
            value = slow.radius;
        if (weaken.enabled && weaken.area && weaken.radius > value)
            value = weaken.radius;
        return value;
    }
}

[System.Serializable]
public class DefensivePowerUp : BattlePowerUp
{
    public GameObject buffFx;
    public float duration = 1;
    public bool area;
    public float radius;
    public Buff buff = new Buff();

    public override bool CheckRequirements()
    {
        if (!base.enabled || !area && !base.target)
            return false;
        else
            return true;
    }

    [System.Serializable]
    public class Buff
    {
        public float shotAngle = 1;
        public float controlMultiplier = 1;
        public float radius = 1;
        public float damage = 1;
        public float shootDelay = 1;
        public int targetCount = 0;
    }

    public IEnumerator BoostTowers()
    {
        List<Collider> cols = new List<Collider>();
        if (area)
            cols = new List<Collider>(Physics.OverlapSphere(position, radius, SV.towerMask));
        else if (target)
            cols.Add(target.GetComponent<Collider>());
        cols = SortByDistance(cols);
        for (int i = 0; i < cols.Count; i++)
        {
            TowerBase towerBase = cols[i].GetComponent<TowerBase>();
            towerBase.ApplyPowerUp(this);
        }
        yield return null;
    }

    public override float GetMaxRadius()
    {
        if (!area) return 0f;
        else return radius;
    }
}

[System.Serializable]
public class PassivePowerUp
{
    public string name = "";
    public string description = "";
    public Image icon = null;
    public Sprite defaultSprite = null;
    public Sprite enabledSprite = null;
    public Image onEnabledIcon = null;
    public bool locked = false;
    public bool enabled = false;
    public float[] cost;
    public List<int> req = new List<int>();
    public PassiveTowerPowerUp towerProperties;
    public PassiveEnemyPowerUp enemyProperties;
    public PassivePlayerPowerUp playerProperties;
    public List<string> selectedTowers = new List<string>();
    public List<string> selectedEnemies = new List<string>();

    public void Unlock()
    {
        if (icon && locked && defaultSprite)
            icon.sprite = defaultSprite;

        locked = false;
    }

    public void Activate()
    {
        if (icon && enabledSprite)
            icon.sprite = enabledSprite;

        if (onEnabledIcon)
            onEnabledIcon.enabled = true;

        ApplyPowerUp();
        enabled = true;
    }

    void ApplyPowerUp()
    {
        TowerBase[] allBases = TowerManager.instance.gameObject.GetComponentsInChildren<TowerBase>(true);
        for (int i = 0; i < allBases.Length; i++)
            ApplyToTower(allBases[i], allBases[i].upgrade);
        Properties[] allProps = new Properties[PoolManager.Props.Count];
        PoolManager.Props.Values.CopyTo(allProps, 0);
        for (int i = 0; i < allProps.Length; i++)
            ApplyToEnemy(allProps[i], allProps[i].myMove);
        ApplyToPlayer();
    }


    public void ApplyToTower(TowerBase tBase, Upgrade upg)
    {
        if (!selectedTowers.Contains(tBase.transform.parent.name))
            return;
        if(towerProperties.targets.enabled)
            tBase.myTargets = towerProperties.targets.type;
        if (towerProperties.shotAngle.enabled)
        {
            if (towerProperties.shotAngle.type == TDValue.fix)
                tBase.shotAngle += towerProperties.shotAngle.value;
            else
                tBase.shotAngle = Round(tBase.shotAngle, towerProperties.shotAngle.value);
        }
        if (towerProperties.controlMultiplier.enabled)
        {
            if (towerProperties.controlMultiplier.type == TDValue.fix)
                tBase.controlMultiplier += towerProperties.controlMultiplier.value;
            else
                tBase.controlMultiplier = Round(tBase.controlMultiplier, towerProperties.controlMultiplier.value);
        }
        for (int i = 0; i < upg.options.Count; i++)
        {
            UpgOptions cur = upg.options[i];
            if (towerProperties.damage.enabled)
            {
                if (towerProperties.damage.type == TDValue.fix)
                    cur.damage += towerProperties.damage.value;
                else
                    cur.damage = Round(cur.damage, towerProperties.damage.value);
            }
            if (towerProperties.radius.enabled)
            {
                if (towerProperties.radius.type == TDValue.fix)
                    cur.radius += towerProperties.radius.value;
                else
                    cur.radius = Round(cur.radius, towerProperties.radius.value);
            }
            if (towerProperties.shotDelay.enabled)
            {
                if (towerProperties.shotDelay.type == TDValue.fix)
                    cur.shootDelay += towerProperties.shotDelay.value;
                else
                    cur.shootDelay = Round(cur.shootDelay, towerProperties.shotDelay.value);
            }
            if (towerProperties.targetCount.enabled)
                cur.targetCount += towerProperties.targetCount.value;
            if (towerProperties.cost.enabled)
            {
                float curValue = towerProperties.cost.initialValue;
                if (i > 0) curValue = towerProperties.cost.value;

                for (int j = 0; j < cur.cost.Length; j++)
                {
                    if (towerProperties.cost.type == TDValue.fix)
                        cur.cost[j] += curValue;
                    else if (towerProperties.cost.costType == CostType.intValue)
                        cur.cost[j] = (int)(cur.cost[j] * curValue);
                    else
                        cur.cost[j] = Round(cur.cost[j], curValue);
                }
            }
        }
        if(towerProperties.radius.enabled)
            upg.RangeChange();
    }


    public void ApplyToEnemy(Properties prop, TweenMove move)
    {
        string myName = prop.name;
        if (!selectedEnemies.Contains(myName.Substring(0, myName.Length - 10)))
            return;
        if (enemyProperties.health.enabled)
        {
            float healthDiff = prop.maxhealth;
            if (prop.maxhealth == 0) prop.maxhealth = prop.health;
            if (enemyProperties.health.type == TDValue.fix)
                prop.maxhealth += enemyProperties.health.value;
            else
                prop.maxhealth = Round(prop.maxhealth, enemyProperties.health.value);
            prop.health -= healthDiff - prop.maxhealth;
            prop.health = Mathf.Clamp(prop.health, 1, prop.maxhealth);
        }
        if (enemyProperties.shield.enabled)
        {
            float shieldDiff = prop.shield.maxValue;
            if (prop.shield.maxValue == 0) prop.shield.maxValue = prop.shield.value;
            if (enemyProperties.shield.type == TDValue.fix)
                prop.shield.maxValue += enemyProperties.shield.value;
            else
                prop.shield.maxValue = Round(prop.shield.maxValue, enemyProperties.shield.value);
            prop.shield.value -= shieldDiff - prop.shield.maxValue;
            prop.shield.value = Mathf.Clamp(prop.shield.value, 0, prop.shield.maxValue);
        }
        if (enemyProperties.health.enabled || enemyProperties.shield.enabled)
            prop.SetUnitFrame();
        if (enemyProperties.pointsToEarn.enabled)
        {
            for (int j = 0; j < prop.pointsToEarn.Length; j++)
            {
                if (enemyProperties.pointsToEarn.type == TDValue.fix)
                    prop.pointsToEarn[j] += enemyProperties.pointsToEarn.value[j];
                else if (towerProperties.cost.costType == CostType.intValue)
                    prop.pointsToEarn[j] = (int)(prop.pointsToEarn[j] * enemyProperties.pointsToEarn.value[j]);
                else
                    prop.pointsToEarn[j] = Round(prop.pointsToEarn[j], enemyProperties.pointsToEarn.value[j]);
            }
        }
        if (enemyProperties.damageToDeal.enabled)
        {
            if (enemyProperties.damageToDeal.type == TDValue.fix)
                prop.damageToDeal += (int)enemyProperties.damageToDeal.value;
            else
                prop.damageToDeal = (int)(prop.damageToDeal * enemyProperties.damageToDeal.value);
        }
        if (enemyProperties.speed.enabled)
        {
            float curValue = move.maxSpeed;
            if (enemyProperties.speed.type == TDValue.fix)
                move.maxSpeed += enemyProperties.speed.value;
            else
                move.maxSpeed = Round(move.maxSpeed, enemyProperties.speed.value);
            move.speed = move.maxSpeed;

            if (move.tween != null)
            {
                move.tScale *= move.maxSpeed / curValue;
                if(move.tween.timeScale > move.tScale)
                    move.tween.timeScale = move.tScale;
            }
        }
    }

    public void ApplyToPlayer()
    {
        if(playerProperties.health.enabled)
        {
            if (playerProperties.health.interval > 0)
            {
                GameHandler.instance.StopCoroutine("AddHealthRoutine");
                GameHandler.instance.StartCoroutine("AddHealthRoutine", playerProperties.health);
            }
            else
            {
                float healthDiff = playerProperties.health.value;
                if (playerProperties.health.type == TDValue.percentual)
                    healthDiff = (int)(GameHandler.maxHealth * playerProperties.health.value);
                GameHandler.AddHealth(healthDiff);
            }
        }
        if(playerProperties.resources.enabled)
        {
            if (playerProperties.resources.interval > 0)
            {
                GameHandler.instance.StopCoroutine("SetResourcesRoutine");
                GameHandler.instance.StartCoroutine("SetResourcesRoutine", playerProperties.resources);
            }
            else
            {
                for (int i = 0; i < playerProperties.resources.value.Length; i++)
                {
                    float resDiff = playerProperties.resources.value[i];
                    if (playerProperties.resources.type == TDValue.percentual)
                        resDiff = (int)(GameHandler.resources[i] * playerProperties.resources.value[i]);
                    GameHandler.SetResources(i, resDiff);
                }
            }
        }
    }

    float Round(float value, float multiply)
    {
        return Mathf.Round(value * multiply * 100f) / 100f;
    }

    [System.Serializable]
    public class PassiveTowerPowerUp
    {
        public Damage damage;
        public Radius radius;
        public ShotDelay shotDelay;
        public TargetCount targetCount;
        public Cost cost;
        public Targets targets;
        public ShotAngle shotAngle;
        public ControlMultiplier controlMultiplier;

        [System.Serializable]
        public class Damage
        {
            public bool enabled;
            public float value;
            public TDValue type;
        }

        [System.Serializable]
        public class Radius
        {
            public bool enabled;
            public float value;
            public TDValue type;
        }

        [System.Serializable]
        public class ShotDelay
        {
            public bool enabled;
            public float value;
            public TDValue type; 
        }

        [System.Serializable]
        public class TargetCount
        {
            public bool enabled;
            public int value;
        }

        [System.Serializable]
        public class Cost
        {
            public bool enabled;
            public float initialValue;
            public float value;
            public TDValue type;
            public CostType costType;
        }

        [System.Serializable]
        public class Targets
        {
            public bool enabled;
            public TowerBase.EnemyType type;
        }

        [System.Serializable]
        public class ShotAngle
        {
            public bool enabled;
            public float value;
            public TDValue type; 
        }

        [System.Serializable]
        public class ControlMultiplier
        {
            public bool enabled;
            public float value;
            public TDValue type;   
        }
    }

    [System.Serializable]
    public class PassiveEnemyPowerUp
    {
        public Speed speed;
        public Health health;
        public Shield shield;
        public PointsToEarn pointsToEarn;
        public DamageToDeal damageToDeal;

        [System.Serializable]
        public class Speed
        {
            public bool enabled;
            public float value;
            public TDValue type; 
        }

        [System.Serializable]
        public class Health
        {
            public bool enabled;
            public float value;
            public TDValue type;
        }

        [System.Serializable]
        public class Shield
        {
            public bool enabled;
            public float value;
            public TDValue type;
        }

        [System.Serializable]
        public class PointsToEarn
        {
            public bool enabled;
            public float[] value;
            public TDValue type;
            public CostType costType;
        }

        [System.Serializable]
        public class DamageToDeal
        {
            public bool enabled;
            public float value;
            public TDValue type;
        }
    }

    [System.Serializable]
    public class PassivePlayerPowerUp
    {
        public Health health;
        public Resources resources;

        [System.Serializable]
        public class Health
        {
            public bool enabled;
            public float value;
            public float interval;
            public TDValue type;
        }

        [System.Serializable]
        public class Resources
        {
            public bool enabled;
            public float[] value;
            public float interval;
            public TDValue type;
            public CostType costType;
        }
    }
}
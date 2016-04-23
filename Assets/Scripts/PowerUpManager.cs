using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager instance;
    [HideInInspector]
    public List<BattlePowerUp> battlePowerUps = new List<BattlePowerUp>();
    public List<PassivePowerUp> passivePowerUps = new List<PassivePowerUp>();
    public List<OffensivePowerUp> battleOffensive = new List<OffensivePowerUp>();
    public List<DefensivePowerUp> battleDefensive = new List<DefensivePowerUp>();
    private BattlePowerUp activePowerUp;
    private PassivePowerUp passivePowerUp;
    public static event Action<BattlePowerUp> battlePowerUpActivated;
    public static event Action<PassivePowerUp> passivePowerUpActivated;


    void Start()
    {
        instance = this;
    }

    public void SelectPowerUp(int index)
    {
        BattlePowerUp powerUp = null;
        if (index <= battleOffensive.Count - 1)
            powerUp = battleOffensive[index];
        else
            powerUp = battleDefensive[index - battleOffensive.Count];
        if (activePowerUp == powerUp)
            Deselect();
        else
            activePowerUp = powerUp;
    }

    public void SelectPassivePowerUp(int index)
    {
        if (passivePowerUps.Count - 1 < index)
            Debug.LogWarning("Button index " + index + " does not match PowerUps list count.");
        passivePowerUp = passivePowerUps[index];
    }

    public bool HasSelection()
    {
        if (activePowerUp != null)
            return true;
        else
            return false;
    }

    public bool HasPassiveSelection()
    {
        if (passivePowerUp != null)
            return true;
        else
            return false;
    }

    public BattlePowerUp GetSelection()
    {
        return activePowerUp;
    }

    public PassivePowerUp GetPassiveSelection()
    {
        return passivePowerUp;
    }

    public void Deselect()
    {
        activePowerUp = null;
    }

    public void Activate()
    {
        StartCoroutine("ActivatePowerUp", activePowerUp);
    }

    IEnumerator ActivatePowerUp(OffensivePowerUp powerUp)
    {
        if(!powerUp.CheckRequirements())
            yield break;
        else
            powerUp.enabled = false;
        if(battlePowerUpActivated != null)
            battlePowerUpActivated(powerUp);
        powerUp.InstantiateFX();
        yield return new WaitForSeconds(powerUp.startDelay);
        if (powerUp.weaken.enabled)
            yield return StartCoroutine(powerUp.Weaken());
        if (powerUp.explosion.enabled)
            yield return StartCoroutine(powerUp.Explosion());
        if (powerUp.burn.enabled)
            yield return StartCoroutine(powerUp.Burn());
        if (powerUp.slow.enabled)
            yield return StartCoroutine(powerUp.Slow());
        powerUp.target = null;
        powerUp.position = Vector3.zero;
        yield return new WaitForSeconds(powerUp.cooldown);
        powerUp.enabled = true;
    }

    IEnumerator ActivatePowerUp(DefensivePowerUp powerUp)
    {
        if(!powerUp.CheckRequirements())
            yield break;
        else
            powerUp.enabled = false;
        if (battlePowerUpActivated != null)
            battlePowerUpActivated(powerUp);
        powerUp.InstantiateFX();
        yield return new WaitForSeconds(powerUp.startDelay);
        yield return StartCoroutine(powerUp.BoostTowers());
        powerUp.target = null;
        powerUp.position = Vector3.zero;
        yield return new WaitForSeconds(powerUp.cooldown);
        powerUp.enabled = true;
    }

    public void ActivatePassive()
    {
        passivePowerUp.Activate();
        if(passivePowerUpActivated != null)
            passivePowerUpActivated(passivePowerUp);
        UnlockPassive();
    }

    public void UnlockPassive()
    {
        for (int i = 0; i < passivePowerUps.Count; i++)
        {
            PassivePowerUp powerup = passivePowerUps[i];
            bool state = true;
            for (int j = 0; j < powerup.req.Count; j++)
            {
                if (!passivePowerUps[powerup.req[j]].enabled)
                {
                    state = false;
                    break;
                }
            }
            if(state) powerup.Unlock();
        }
    }

    public static void ApplyToSingleTower(TowerBase tBase, Upgrade upg)
    {
        for (int i = 0; i < instance.passivePowerUps.Count; i++)
        {
            if (instance.passivePowerUps[i].enabled)
                instance.passivePowerUps[i].ApplyToTower(tBase, upg);
        }
    }

    public static void ApplyToSingleEnemy(Properties prop, TweenMove move)
    {
        for (int i = 0; i < instance.passivePowerUps.Count; i++)
        {
            if (instance.passivePowerUps[i].enabled)
                instance.passivePowerUps[i].ApplyToEnemy(prop, move);
        }
    }
}
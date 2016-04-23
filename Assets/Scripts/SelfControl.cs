using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SelfControl : MonoBehaviour
{
    private Transform cam;
    private CameraControl camDesktop;
    private TowerBase towerScript;
    private TowerRotation towerRotScript;
    private int curLvl;
    private float shotTime;
    private TowerBase.EnemyType enemyType;
    private Transform cross;
    private LineRenderer aimRend;
    private GameObject guiObj;
    private bool centerCrosshair = false;

    public void Initialize(GameObject guiObj, GameObject crosshair, GameObject aimIndicator,
                           float towerHeight, bool mobile)
    {
        cam = Camera.main.transform;
        this.guiObj = guiObj;
        if (!mobile)
        {
            camDesktop = cam.GetComponent<CameraControl>();
            camDesktop.enabled = false;
        }
        towerScript = gameObject.GetComponent<TowerBase>();
        towerScript.rangeInd.GetComponent<Renderer>().enabled = true;
        towerScript.CancelInvoke("CheckRange");
        curLvl = towerScript.upgrade.curLvl;
        shotTime = towerScript.lastShot + towerScript.upgrade.options[curLvl].shootDelay;
        enemyType = towerScript.myTargets;
        if (towerScript.turret)
        {
            towerRotScript = towerScript.turret.GetComponent<TowerRotation>();
            towerRotScript.enabled = false;
        }
        cross = crosshair.transform;
        centerCrosshair = mobile;
        aimRend = aimIndicator.GetComponent<LineRenderer>();
        aimRend.SetPosition(0, towerScript.shotPos.position);
        aimRend.SetPosition(1, towerScript.shotPos.position);
        Holoville.HOTween.HOTween.To(cam, 1f, "position", transform.position + new Vector3(0, towerHeight, 0), false, Holoville.HOTween.EaseType.EaseInOutExpo, 0f);
        Invoke("StartCoroutineUpdate", 1);
    }

    void StartCoroutineUpdate()
    {
        StartCoroutine("CoroutineUpdate");
    }
    
    IEnumerator CoroutineUpdate()
    {
        while (true)
        {
            if (camDesktop)
            {
                camDesktop.Rotate();
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    Attack();
                }
            }
            PositionCrosshair();
            yield return null;
        }
    }


    void PositionCrosshair()
    {
        if (EventSystem.current.IsPointerOverGameObject()
            && !centerCrosshair || SV.showExit)
            return;
        Ray ray;
        RaycastHit rayHit;

        if (centerCrosshair)
        {
            ray = new Ray(cam.position, cam.forward);
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        if (Physics.Raycast(ray, out rayHit, 300, SV.worldMask))
        {
            if (towerRotScript)
            {
                towerScript.turret.LookAt(new Vector3(cross.position.x, towerScript.turret.position.y, cross.position.z));
            }
            float dist = Vector3.Distance(transform.position, rayHit.point);
            float radius = towerScript.upgrade.options[curLvl].radius;
            if (dist <= radius && rayHit.transform.tag == "Ground")
            {
                cross.position = rayHit.point + new Vector3(0, 0.2f, 0);
                cross.localEulerAngles = Vector3.zero;
                aimRend.SetPosition(0, towerScript.shotPos.position);
                aimRend.SetPosition(1, cross.position);
            }
            else if (enemyType == TowerBase.EnemyType.Air ||
                     enemyType == TowerBase.EnemyType.Both)
            {
                if (dist < radius)
                    cross.position = rayHit.point - ray.direction;
                else
                    cross.position = ray.origin + ray.direction * (radius + 2);
                cross.LookAt(cam);
                cross.localEulerAngles += new Vector3(90, 0, 0);
                aimRend.SetPosition(0, towerScript.shotPos.position);
                aimRend.SetPosition(1, cross.position);
            }
        }
    }

    public void Attack()
    {
        float delay = towerScript.upgrade.options[curLvl].shootDelay;
        float newShotTime = towerScript.lastShot + delay;
        if (Time.time < shotTime && newShotTime > shotTime)
            shotTime = newShotTime;
        if (Time.time < shotTime)
            return;
        towerScript.lastShot = Time.time;
        shotTime = towerScript.lastShot + delay;
        towerScript.SelfAttack(cross.position);
        guiObj.SendMessage("DrawReload", SendMessageOptions.DontRequireReceiver);
    }

    public void Terminate()
    {
        this.StopAllCoroutines();
        if(camDesktop)
        camDesktop.enabled = true;
        towerScript.rangeInd.GetComponent<Renderer>().enabled = false;
        if (towerRotScript)
        {
            towerRotScript.enabled = true;
        }
        SV.control = null;
        towerScript.GetTargets();
        float lastShot = towerScript.lastShot;
        float invokeInSec = towerScript.upgrade.options[curLvl].shootDelay + lastShot - Time.time;
        if (invokeInSec < 0) invokeInSec = 0f;
        towerScript.StartInvoke(invokeInSec);
        cross.position = new Vector3(0, -100, 0);
        aimRend.SetPosition(0, new Vector3(0, -100, 0));
        aimRend.SetPosition(1, new Vector3(0, -100, 0));
        Destroy(this);
    }
}

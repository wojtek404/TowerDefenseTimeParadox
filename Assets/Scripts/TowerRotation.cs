using UnityEngine;
using System.Collections;

public class TowerRotation : MonoBehaviour
{
    public float damping = 0f;
    [HideInInspector]
	public TowerBase towerScript;

    void Awake()
    {
        this.enabled = false;
    }

    void Update()
    {
        Transform rotateTo = towerScript.currentTarget;
        if (rotateTo && !towerScript.inRange.Contains(rotateTo.gameObject))
        {
            if (towerScript.inRange.Count > 0)
                rotateTo = towerScript.inRange[0].transform;
            else
                rotateTo = null;
        }
        if(!rotateTo)
            return;
        Vector3 dir = rotateTo.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime / (damping / 10));
		transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
}
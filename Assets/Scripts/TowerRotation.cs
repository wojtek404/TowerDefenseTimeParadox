using UnityEngine;
using System.Collections;

public class TowerRotation : MonoBehaviour
{
    public float damping = 0f;
    [HideInInspector]
	public TowerController towerController;

    public float initialRotationY = 0f;

    void Awake()
    {
        this.enabled = false;
    }

    void Update()
    {
        Transform rotateTo = towerController.currentTarget;
        if (rotateTo && !towerController.inRange.Contains(rotateTo.gameObject))
        {
            if (towerController.inRange.Count > 0)
                rotateTo = towerController.inRange[0].transform;
            else
                rotateTo = null;
        }
        if(!rotateTo)
            return;
        Vector3 dir = rotateTo.position - transform.position;
        dir.y += initialRotationY;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime / (damping / 10));
		transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y + initialRotationY, 0f);
    }
}
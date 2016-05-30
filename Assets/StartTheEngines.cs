using UnityEngine;
using System.Collections;

public class StartTheEngines : MonoBehaviour {
    public float rotationSpeed;
    private Transform leftBladeTransform, rightBladeTransform;
    private float lastUpdate, delta;
	void Start () {
        leftBladeTransform = transform.FindChild("PA_Drone").FindChild("PA_DroneWingLeft").FindChild("BladeAxisLeft");
        rightBladeTransform = transform.FindChild("PA_Drone").FindChild("PA_DroneWingRight").FindChild("BladeAxisRight");
    }
	
	void Update () {
        delta = Time.time - lastUpdate;
        lastUpdate = Time.time;
        leftBladeTransform.Rotate(0, rotationSpeed * delta, 0);
        rightBladeTransform.Rotate(0, -rotationSpeed * delta, 0);
    }
}

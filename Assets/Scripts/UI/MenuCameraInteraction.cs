using UnityEngine;
using System.Collections;

public class MenuCameraInteraction : MonoBehaviour {

    public Camera menuCamera;
    public int movementRange;
    private Vector3 cameraInitialPosition;
    private float factor;

	void Start () {
        cameraInitialPosition = menuCamera.transform.position;
        factor = movementRange / 1000f;
    }
	
	void Update () {
        menuCamera.transform.position = cameraInitialPosition + Input.mousePosition * factor;
    }
}

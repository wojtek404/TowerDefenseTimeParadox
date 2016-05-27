using UnityEngine;
using System.Collections;

public class StayInPlace : MonoBehaviour {
    private Vector3 differencePosition;
    private Vector3 differenceRotation;

	void Start ()
    {
        Transform rootTransform = transform.FindChild("ROOT");
        Transform originTransform = transform.FindChild("Origin");
        differencePosition = new Vector3(rootTransform.position.x - originTransform.position.x, rootTransform.position.y - originTransform.position.y, rootTransform.position.z - originTransform.position.z);
        differenceRotation = new Vector3(rootTransform.rotation.x - originTransform.rotation.x, rootTransform.rotation.y - originTransform.rotation.y, rootTransform.rotation.z - originTransform.rotation.z);
    }
	
	void Update ()
    {
        Transform rootTransform = transform.FindChild("ROOT");
        Transform originTransform = transform.FindChild("Origin");
        rootTransform.position = new Vector3(originTransform.position.x + differencePosition.x, originTransform.position.y + differencePosition.y, originTransform.position.z + differencePosition.z);
        rootTransform.eulerAngles = new Vector3(originTransform.eulerAngles.x + differenceRotation.x + 90, originTransform.eulerAngles.y + differenceRotation.y, originTransform.eulerAngles.z + differenceRotation.z);
    }
}

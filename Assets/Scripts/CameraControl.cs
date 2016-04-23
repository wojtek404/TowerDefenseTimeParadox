using UnityEngine;
using System.Collections;


public class CameraControl : MonoBehaviour
{
    public float scrollSpeed = 15f;
    public bool edgeScroll;
    public float scrollEdge = 0.01f;
    public float panSpeed = 10f;
    public Vector2 zoomRange = new Vector2(-5,5);
    public float zoomSpeed = 1f;
    private float currentZoom = 0;
    private Vector3 initPos;
    public float xRotSpeed = 200f;
    public float yRotSpeed = 200f;
    public float rotDamp = 5f;
    public float yMinRotLimit = -10f;
    public float yMaxRotLimit = 60f;
    private float xDeg;
    private float yDeg;
    private Quaternion desiredRotation;
    private Quaternion currentRotation;
    private Quaternion rotation;

    public void Start()
    {
        initPos = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;
        xDeg = ClampAngle(transform.eulerAngles.y, 0f, 360f);
        yDeg = ClampAngle(transform.eulerAngles.x, 0f, 360f);
    }

    void LateUpdate()
    {
        if (Input.GetKey("mouse 2") && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            if ( Input.mousePosition.x <= Screen.width * 0.5f && CheckBounds(-transform.right)
                 || Input.mousePosition.x >= Screen.width * 0.5f && CheckBounds(transform.right))
            {
                transform.Translate(transform.right * Time.deltaTime * panSpeed * 
                (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f), Space.World);
            }
            if ( Input.mousePosition.y <= Screen.height * 0.5f && CheckBounds(-transform.forward)
                 || Input.mousePosition.y >= Screen.height * 0.5f && CheckBounds(transform.forward))
            {
                transform.Translate(transform.forward * Time.deltaTime * panSpeed *
                (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), Space.World);
            }
        }
        else
        {
            if (edgeScroll)
            {
                if (Input.mousePosition.x >= Screen.width * (1 - scrollEdge) && CheckBounds(transform.right))
                {
                    transform.Translate(transform.right * Time.deltaTime * scrollSpeed, Space.World);
                }
                else if (Input.mousePosition.x <= Screen.width * scrollEdge && CheckBounds(-transform.right))
                {
                    transform.Translate(transform.right * Time.deltaTime * -scrollSpeed, Space.World);
                }

                if (Input.mousePosition.y >= Screen.height * (1 - scrollEdge) && CheckBounds(transform.forward))
                {
                    transform.Translate(transform.forward * Time.deltaTime * scrollSpeed, Space.World);
                }
                else if (Input.mousePosition.y <= Screen.height * scrollEdge && CheckBounds(-transform.forward))
                {
                    transform.Translate(transform.forward * Time.deltaTime * -scrollSpeed, Space.World);
                }
            }
            else
            {
                if (Input.GetKey("right") && CheckBounds(transform.right))
                {
                    transform.Translate(transform.right * Time.deltaTime * scrollSpeed, Space.World);
                }
                else if (Input.GetKey("left") && CheckBounds(-transform.right))
                {
                    transform.Translate(transform.right * Time.deltaTime * -scrollSpeed, Space.World);
                }

                if (Input.GetKey("up") && CheckBounds(transform.forward))
                {
                    transform.Translate(transform.forward * Time.deltaTime * scrollSpeed, Space.World);
                }
                else if (Input.GetKey("down") && CheckBounds(-transform.forward))
                {
                    transform.Translate(transform.forward * Time.deltaTime * -scrollSpeed, Space.World);
                }
            }
        }

        if (!Input.GetMouseButton(0))
        {
            currentZoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000 * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, zoomRange.x, zoomRange.y);
            Vector3 pos = transform.position;
            pos.y -= (transform.position.y - (initPos.y + currentZoom)) * 0.1f;
            transform.position = pos;
        }
        Rotate();
    }


    public void Rotate()
    {
        if (Input.GetMouseButton(1))
        {            
            xDeg += Input.GetAxis("Mouse X") * xRotSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * yRotSpeed * 0.02f;
        }
        if (SV.control)
        {
            if (Input.GetKey("right"))
            {
                xDeg += Time.deltaTime * scrollSpeed * xRotSpeed * 0.02f;
            }
            else if (Input.GetKey("left"))
            {
                xDeg += Time.deltaTime * -scrollSpeed * xRotSpeed * 0.02f;
            }
            if (Input.GetKey("down"))
            {
                yDeg += Time.deltaTime * scrollSpeed * yRotSpeed * 0.02f;
            }
            else if (Input.GetKey("up"))
            {
                yDeg += Time.deltaTime * -scrollSpeed * yRotSpeed * 0.02f;
            }
        }
        if (Input.GetMouseButton(1) || SV.control)
        {
            yDeg = ClampAngle(yDeg, yMinRotLimit, yMaxRotLimit);
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * rotDamp);
            transform.rotation = rotation;
        }
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private bool CheckBounds(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, 5, SV.worldMask))
        {
            return false;
        }
        else
            return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.right * 5);
        Gizmos.DrawRay(transform.position, -transform.right * 5);
        Gizmos.DrawRay(transform.position, transform.forward * 5);
        Gizmos.DrawRay(transform.position, -transform.forward * 5);
    }
}

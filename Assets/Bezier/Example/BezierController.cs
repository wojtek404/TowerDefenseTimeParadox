/**
 * Author: Sander Homan
 * Copyright 2012
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BezierController : MonoBehaviour
{
    public BezierPath path = null;
    public float speed = 1;
    public bool byDist = false;
    public bool looped = false;
    private Vector3 startingDirection = Vector3.left;
    public float yRotation;
    public bool rotatePathOriented = false;
    private Vector3 prevPosition;

    private float t = 0;

    void Start()
    {
        prevPosition = transform.position;
    }

    void Update()
    {
        t += speed*Time.deltaTime;

        if (t > path.points.Count && looped)
            t = 0;

        if (!byDist)
            transform.position = path.GetPositionByT(t);
        else
            transform.position = path.GetPositionByDistance(t);

        RotateWithPath();
        /* if(t > path.points.Count)
             transform.rotation = Quaternion.Euler(transform.rotation.x, yRotation, transform.rotation.z);*/
        if (rotatePathOriented)
            RotateBasedOnPositionDifference();
    }

    void RotateWithPath()
    {
        float addition = 0.2f;
        Vector3 direction;
        if(t + addition > path.points.Count )
            direction = path.GetPositionByT(t) - transform.position;
        else
            direction = path.GetPositionByT((t + addition)) - transform.position;

        float angleOnX = Mathf.Asin(Vector3.Cross(direction.normalized, startingDirection).x) * Mathf.Rad2Deg;
        float angleOnY = Mathf.Asin(Vector3.Cross(direction.normalized, startingDirection).y) * Mathf.Rad2Deg;
        float angleOnZ = Mathf.Asin(Vector3.Cross(direction.normalized, startingDirection).z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(angleOnX, yRotation, 0);
    }

    void RotateBasedOnPositionDifference()
    {
        Vector3 differenceVector = transform.position - prevPosition;
        prevPosition = transform.position;
        differenceVector.y = 0;
        if(differenceVector.magnitude != 0)
            transform.rotation = Quaternion.LookRotation(differenceVector);
    }
}


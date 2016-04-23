using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public Transform[] waypoints;
    public bool drawStraight = true;
    public bool drawCurved = true;

    public Color color1 = new Color(1, 0, 1, 0.5f);
    public Color color2 = new Color(1, 235 / 255f, 4 / 255f, 0.5f);

    private float radius = .4f;
    private Vector3 size = new Vector3(.7f, .7f, .7f);


    void OnDrawGizmos()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Waypoint")
            {
                Gizmos.color = color2;
                Gizmos.DrawWireSphere(child.position, radius);
            }
            else
            {
                Gizmos.color = color1;
                Gizmos.DrawWireCube(child.position, size);
            }
        }

        if (drawStraight)
            DrawStraight();

        if (drawCurved)
            DrawCurved();
    }


    void DrawStraight()
    {
        Gizmos.color = color2;
        for (int i = 0; i < waypoints.Length - 1; i++)
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
    }

    Vector3[] points;

    void DrawCurved()
    {
        if (waypoints.Length < 2) return;

        points = new Vector3[waypoints.Length + 2];

        for (int i = 0; i < waypoints.Length; i++)
        {
            points[i + 1] = waypoints[i].position;
        }

        points[0] = points[1];
        points[points.Length - 1] = points[points.Length - 2];

        Gizmos.color = color2;
        Vector3[] drawPs;
        Vector3 currPt;

        int subdivisions = points.Length * 10;
        drawPs = new Vector3[subdivisions + 1];
        for (int i = 0; i <= subdivisions; ++i)
        {
            float pm = i / (float)subdivisions;
            currPt = GetPoint(pm);
            drawPs[i] = currPt;
        }
        Vector3 prevPt = drawPs[0];
        for (int i = 1; i < drawPs.Length; ++i)
        {
            currPt = drawPs[i];
            Gizmos.DrawLine(currPt, prevPt);
            prevPt = currPt;
        }
    }

    private Vector3 GetPoint(float t)
    {
        int numSections = points.Length - 3;
        int tSec = (int)Math.Floor(t * numSections);
        int currPt = numSections - 1;
        if (currPt > tSec)
        {
            currPt = tSec;
        }
        float u = t * numSections - currPt;

        Vector3 a = points[currPt];
        Vector3 b = points[currPt + 1];
        Vector3 c = points[currPt + 2];
        Vector3 d = points[currPt + 3];

        return .5f * (
                       (-a + 3f * b - 3f * c + d) * (u * u * u)
                       + (2f * a - 5f * b + 4f * c - d) * (u * u)
                       + (-a + c) * u
                       + 2f * b
                   );
    }
}

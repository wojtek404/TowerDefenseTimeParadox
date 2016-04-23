using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PathManager))]
public class PathEditor : Editor
{
    private SerializedObject m_Object;
    private SerializedProperty m_Waypoint;
    private SerializedProperty m_WaypointsCount;
    private SerializedProperty m_Check1;
    private SerializedProperty m_Check2;
    private SerializedProperty m_Color1;
    private SerializedProperty m_Color2;

    private static string wpArraySize = "waypoints.Array.size";
    private static string wpArrayData = "waypoints.Array.data[{0}]";

    public void OnEnable()
    {
        m_Object = new SerializedObject(target);
        m_Check1 = m_Object.FindProperty("drawStraight");
        m_Check2 = m_Object.FindProperty("drawCurved");
        m_Color1 = m_Object.FindProperty("color1");
        m_Color2 = m_Object.FindProperty("color2");
        m_WaypointsCount = m_Object.FindProperty(wpArraySize);
    }


    private Transform[] GetWaypointArray()
    {
        var arrayCount = m_Object.FindProperty(wpArraySize).intValue;
        var transformArray = new Transform[arrayCount];
        for (var i = 0; i < arrayCount; i++)
        {
            transformArray[i] = m_Object.FindProperty(string.Format(wpArrayData, i)).objectReferenceValue as Transform;
        }
        return transformArray;
    }


    private void SetWaypoint(int index, Transform waypoint)
    {
        m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue = waypoint;
    }

    private Transform GetWaypointAtIndex(int index)
    {
        return m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue as Transform;
    }

    private void RemoveWaypointAtIndex(int index)
    {
        Undo.RegisterFullObjectHierarchyUndo(GetWaypointAtIndex(index).parent, "RemoveWaypoint");
        Undo.DestroyObjectImmediate(GetWaypointAtIndex(index).gameObject);
        for (int i = index; i < m_WaypointsCount.intValue - 1; i++)
            SetWaypoint(i, GetWaypointAtIndex(i + 1));
        m_WaypointsCount.intValue--;
    }

    private void AddWaypointAtIndex(int index)
    {
        m_WaypointsCount.intValue++;
        for (int i = m_WaypointsCount.intValue - 1; i > index; i--)
        {
            SetWaypoint(i, GetWaypointAtIndex(i - 1));
        }
        GameObject wp = new GameObject("Waypoint");
        Undo.RegisterCreatedObjectUndo(wp, "CreatedWaypoint");
        wp.transform.position = GetWaypointAtIndex(index).position;
        wp.transform.parent = GetWaypointAtIndex(index).parent;
        SetWaypoint(index + 1, wp.transform);
        Selection.activeGameObject = wp;
    }

    public override void OnInspectorGUI()
    {
        m_Object.Update();
        m_Check1.boolValue = EditorGUILayout.Toggle("Draw straight Lines", m_Check1.boolValue);
        m_Check2.boolValue = EditorGUILayout.Toggle("Draw curved Lines", m_Check2.boolValue);
        EditorGUILayout.PropertyField(m_Color1);
        EditorGUILayout.PropertyField(m_Color2);
        var waypoints = GetWaypointArray();
        float pathLength = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
            pathLength += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        GUILayout.Label("Path Length: " + pathLength);
        GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);
        for (int i = 0; i < waypoints.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label((i + 1) + ".", GUILayout.Width(20));
            var result = EditorGUILayout.ObjectField(waypoints[i], typeof(Transform), true) as Transform;
            if (GUI.changed)
                SetWaypoint(i, result);
            if (i < waypoints.Length - 1 && GUILayout.Button("+", GUILayout.Width(30f)))
			{
                AddWaypointAtIndex(i);
				break;
			}
            if (i > 0 && i < waypoints.Length - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
			{
                RemoveWaypointAtIndex(i);
				break;
			}

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Place to Ground"))
        {
            foreach (Transform trans in waypoints)
            {
                Undo.RecordObject(trans, "PlaceGround");
                Ray ray = new Ray(trans.position + new Vector3(0, 2f, 0), -trans.up);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    trans.position = new Vector3(trans.position.x, hit.point.y, trans.position.z);
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Invert Direction"))
        {
            Undo.RecordObjects(waypoints, "InvertDirection");
            Vector3[] waypointCopy = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypointCopy[i] = waypoints[i].position;
            }
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i].position = waypointCopy[waypointCopy.Length - 1 - i];
            }
        }
        m_Object.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        var waypoints = GetWaypointArray();
        if (waypoints.Length == 0) return;
        Handles.BeginGUI();
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            var guiPoint = HandleUtility.WorldToGUIPoint(waypoints[i].transform.position);
            var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
            GUI.Box(rect, "Waypoint: " + (i + 1));
        }
        Handles.EndGUI();
    }
}

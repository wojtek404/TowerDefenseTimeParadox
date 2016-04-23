using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static readonly Dictionary<string, PathManager> Paths = new Dictionary<string, PathManager>();

    void Awake()
    {
        Paths.Clear();
        foreach (Transform path in transform)
        {
            AddPath(path.gameObject);
        }
    }

    public static void AddPath(GameObject path)
    {
        if (path.name.Contains("Clone"))
        {
            path.name = path.name.Replace("(Clone)", "");
        }
        if (Paths.ContainsKey(path.name))
        {
            Debug.LogWarning("Called AddPath() but Scene already contains Path " + path.name + ".");
            return;
        }
        PathManager pathMan = path.GetComponent<PathManager>();
        if (pathMan == null)
        {
            Debug.LogWarning("Called AddPath() but Transform " + path.name + " has no PathManager attached.");
            return;
        }
        Paths.Add(path.name, pathMan);
    }

}


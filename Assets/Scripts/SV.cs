using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SV : MonoBehaviour
{
    public static int gridLayer;
    public static int gridMask;
    public static int towerLayer;
    public static int towerMask;
    public static int worldLayer;
    public static int worldMask;
    public static int enemyMask;
    public static int enemyLayer;
    public static Vector3 outOfView = new Vector3(0, -300, 0);
    public static GameObject selection;
    public static GameObject gridSelection;
    public static GameObject powerUpIndicator;
    public static bool showUpgrade;
    public static bool showExit;

    void Awake()
    {
        Reset();
        SetMasks();
    }

    public static void SetMasks()
    {
        enemyLayer = LayerMask.NameToLayer("Enemies");
        gridLayer = LayerMask.NameToLayer("Grid");
        towerLayer = LayerMask.NameToLayer("Tower");
        worldLayer = LayerMask.NameToLayer("WorldLimit");

        gridMask = 1 << LayerMask.NameToLayer("Grid");
        towerMask = 1 << LayerMask.NameToLayer("Tower");
        worldMask = 1 << LayerMask.NameToLayer("WorldLimit");
        enemyMask = 1 << LayerMask.NameToLayer("Enemies");
    }

    public static void Reset()
    {    
        selection = null;
		gridSelection = null;
        showUpgrade = false;
        showExit = false;
    }
}
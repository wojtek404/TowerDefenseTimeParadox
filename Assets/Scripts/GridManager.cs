using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject gridPrefab;
    public Material gridFreeMat;
    public Material gridFullMat;
    private bool gridVisible = true;
    public List<string> GridList = new List<string>();

    void Start()
    {
        ToggleVisibility(false);
    }

    public void ToggleVisibility(bool visible)
    {
        if (gridVisible == visible) return;
        gridVisible = visible;
        foreach (Transform trans in transform)
        {
            trans.GetComponent<Renderer>().enabled = gridVisible;
        }
    }
}

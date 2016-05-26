using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GUILogic : MonoBehaviour
{
    public TowerManager towerManager;
    public GridManager gridScript;
    public Camera raycastCam;

    private Ray ray;
    private RaycastHit gridHit;
    private RaycastHit towHit;

    private Transform towerContainer;
    [HideInInspector]
    public TowerController towerController;
    [HideInInspector]
    public Upgrade upgrade;
    [HideInInspector]
    public GameObject currentGrid;
    [HideInInspector]
    public GameObject currentTower;

    int index = 0;

    public GameObject[] invisibleWidgets;

    void Start()
    {
        if (towerManager == null)
            Debug.LogWarning("GUI TowerManager not set");

        if (gridScript == null)
            Debug.LogWarning("GUI GridManager not set");

        towerContainer = GameObject.Find("Tower Manager").transform;
        foreach (GameObject gui_Obj in invisibleWidgets)
        {
            gui_Obj.SetActive(false);
        }
    }

    void Update()
    {
        ray = raycastCam.ScreenPointToRay(Input.mousePosition);
        RaycastGrid();
        RaycastTower();
    }

    void RaycastGrid()
    {
        if (Physics.Raycast(ray, out gridHit, 300, SV.gridMask | SV.towerMask))
        {
            Debug.DrawLine(ray.origin, gridHit.point, Color.yellow);
            GameObject hit = gridHit.transform.gameObject;
            if(hit.layer == SV.gridLayer)
                currentGrid = gridHit.transform.gameObject;
        }
        else
            currentGrid = null;
    }

    void RaycastTower()
    {
        if (Physics.Raycast(ray, out towHit, 300, SV.towerMask))
        {
            Debug.DrawLine(ray.origin, towHit.point, Color.red);
            currentTower = towHit.transform.gameObject;
        }
        else
            currentTower = null;
    }

    public void CancelSelection(bool destroySelection)
    {
        if (destroySelection && SV.selection)
            Destroy(SV.selection);

        currentGrid = null;
        currentTower = null;
        SV.selection = null;
        SV.gridSelection = null;
        gridScript.ToggleVisibility(false);
    }

    public bool CheckIfGridIsFree()
    {
        if (currentGrid == null || gridScript.GridList.Contains(currentGrid.name))
            return false;
        else
            return true;
    }

    public void SetTowerComponents(GameObject tower)
    {
        upgrade = tower.GetComponent<Upgrade>();
        towerController = tower.GetComponent<TowerController>();
    }

    public bool AvailableUpgrade()
    {
        if (upgrade == null)
            return false;
        bool available = true;
        int curLvl = upgrade.curLvl;
        if (curLvl >= upgrade.options.Count - 1)
            available = false;

        return available;
    }
    public bool AffordableUpgrade()
    {
        if (upgrade == null)
            return false;
        bool affordable = true;
        int curLvl = upgrade.curLvl;
        if (AvailableUpgrade())
        {
            if (GameHandler.resources < upgrade.options[curLvl + 1].cost)
                affordable = false;
        }
        else
            affordable = false;

        return affordable;
    }

    public float GetUpgradePrice()
    {
        float upgradePrice = 0;
        int curLvl = upgrade.curLvl;
        if (AvailableUpgrade())
            upgradePrice = upgrade.options[curLvl + 1].cost;
        return upgradePrice;
    }

    public float GetSellPrice()
    {
        float sellPrice = 0;
        int curLvl = upgrade.curLvl;
        for (int j = 0; j < curLvl + 1; j++)
            sellPrice += upgrade.options[j].cost * (1f - (towerManager.sellLoss / 100f));
        return sellPrice;
    }

    public void InstantiateTower(int clickedButton)
    {
        index = clickedButton;
        if (SV.selection)
        {
            currentGrid = null;
            currentTower = null;
            Destroy(SV.selection);
        }
        if (gridScript.GridList.Count == gridScript.transform.childCount)
        {
            StartCoroutine("DisplayError", "No free grids left for placing a new tower!");
            Debug.Log("No free grids left for placing a new tower!");
            return;
        }
        UpgOptions opt = towerManager.towerUpgrade[index].options[0];
        float price = opt.cost;
        if (GameHandler.resources < price)
        {
            StartCoroutine("DisplayError", "Not enough resources for buying this tower!");
            Debug.Log("Not enough resources for buying this tower!");
            if (SV.gridSelection)
            {
                GameObject grid = SV.gridSelection;
                CancelSelection(true);
                SV.gridSelection = grid;
                grid.GetComponent<Renderer>().enabled = true;
            }
            else
                CancelSelection(true);
            return;
        }
        SV.selection = (GameObject)Instantiate(towerManager.towerPrefabs[index], SV.outOfView, Quaternion.identity);
        SV.selection.name = towerManager.towerNames[index];
        towerController = SV.selection.GetComponentInChildren<TowerController>();
        towerController.gameObject.name = towerManager.towerNames[index];
        SV.selection.transform.parent = towerContainer;
        upgrade = SV.selection.GetComponentInChildren<Upgrade>();
        towerController.upgrade = upgrade;
        towerController.enabled = false;
        if (!SV.gridSelection)
            gridScript.ToggleVisibility(true);
    }

    public void BuyTower()
    {
        GameHandler.resources -= towerManager.towerUpgrade[index].options[0].cost;
        if (SV.gridSelection)
        {
            currentGrid = SV.gridSelection;
            SV.gridSelection.GetComponent<Renderer>().enabled = false;
        }
        gridScript.GridList.Add(currentGrid.name);
        currentGrid.transform.GetComponent<Renderer>().material = gridScript.gridFullMat;
        towerController.enabled = true;
        towerController.rangeInd.GetComponent<Renderer>().enabled = false;
        if (towerController.turret)
            towerController.turret.gameObject.GetComponent<TowerRotation>().enabled = true;
    }

    public void UpgradeTower()
    {
        upgrade.LVLchange();
        GameHandler.resources -= upgrade.options[upgrade.curLvl].cost;
    }

    public void SellTower(GameObject tower)
    {
        float sellPrice = GetSellPrice();
        GameHandler.resources += sellPrice;    
        Ray ray = new Ray(tower.transform.position + new Vector3(0, 0.5f, 0), -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, SV.gridMask))
        {
            Transform grid = hit.transform;
            gridScript.GridList.Remove(grid.name);
            grid.GetComponent<Renderer>().material = gridScript.gridFreeMat;
        }
    }

    public void OnVolumeChange(float value)
    {
        AudioListener.volume = value;
    }

    public IEnumerator FadeIn(GameObject gObj)
    {
        float duration = 0.2f;
        if (!gObj.activeInHierarchy)
            gObj.SetActive(true);
        else
            yield break;
        float alpha = 1f;

        Graphic[] graphics = gObj.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].canvasRenderer.SetAlpha(0f);
            graphics[i].CrossFadeAlpha(alpha, duration, true);
        }
    }

    public IEnumerator FadeOut(GameObject gObj)
    {
        float duration = 0.2f;
        if (!gObj.activeInHierarchy)
            yield break;
        float alpha = 0f;
        Graphic[] graphics = gObj.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].CrossFadeAlpha(alpha, duration, true);
        }
        yield return new WaitForSeconds(duration);
        gObj.SetActive(false);
    }
}

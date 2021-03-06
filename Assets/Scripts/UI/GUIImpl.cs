﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GUIImpl : MonoBehaviour
{
    private GUILogic gui;
    public Panels panels = new Panels();
    public Buttons buttons = new Buttons();
    public Labels labels = new Labels();
    private float delay;

    void Awake()
    {
        gui = GetComponent<GUILogic>();
    }

    void Start()
    {
        DisableMenus();
    }

    public void DisableMenus()
    {
        SV.showUpgrade = false;
        CancelInvoke("UpdateUpgradeMenu");
        gui.StartCoroutine("FadeOut", panels.tooltip);
        gui.StartCoroutine("FadeOut", panels.upgradeMenu);
        Toggle[] allToggles = buttons.towerButtons.GetComponentsInChildren<Toggle>(true);
        for (int i = 0; i < allToggles.Length; i++)
            allToggles[i].isOn = false;
        if (gui.towerController)
            gui.towerController.rangeInd.GetComponent<Renderer>().enabled = false;
        if (SV.gridSelection)
            SV.gridSelection.GetComponent<Renderer>().enabled = false;
        gui.CancelSelection(true);
    }

    void Update()
    {
        CheckESC();
        if (EventSystem.current.IsPointerOverGameObject() && !SV.gridSelection || SV.showExit)
            return;
        else if (Input.GetMouseButtonUp(0)
            && (!gui.currentTower || SV.selection && gui.currentTower.transform.parent.gameObject != SV.selection)
            && (!gui.currentGrid || SV.showUpgrade && gui.currentGrid))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            DisableMenus();
        }
        ProcessGrid();
        ProcessTower();
    }

    void CheckESC()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gui.StartCoroutine("FadeIn", panels.main);
            gui.StartCoroutine("FadeOut", buttons.towerButtons);
            DisableMenus();
            SV.showExit = true;
            Time.timeScale = 0.0001f; //pause
        }
    }


    void ProcessGrid()
    {
        if (!SV.selection)
            return;
        else if (!gui.CheckIfGridIsFree() || gui.currentTower && gui.currentTower.transform.parent.gameObject != SV.selection)
        {
            SV.selection.transform.position = SV.outOfView;
        }
        else
        {
            SV.selection.transform.position = gui.currentGrid.transform.position;
            if (gui.towerController.turret)
                gui.towerController.turret.localRotation = gui.currentGrid.transform.rotation;
            if (Input.GetMouseButtonUp(0))
            {
                BuyTower();
            }
        }
    }


    void ProcessTower()
    {
        GameObject tower = gui.currentTower;
        if (SV.selection || tower == null) return;
        if (Input.GetMouseButtonUp(0) && delay + 0.5f < Time.time)
        {
            ShowUpgradeMenu(tower);
        }
    }

    public void CreateTower(int index)
    {
        Transform button = buttons.towerButtons.transform.GetChild(index);
        Toggle checkbox = button.GetComponent<Toggle>();
        if (SV.showUpgrade && gui.towerController)
        {
            gui.towerController.rangeInd.GetComponent<Renderer>().enabled = false;
            SV.showUpgrade = false;
            CancelInvoke("UpdateUpgradeMenu");
        }
        if (!checkbox || checkbox.isOn)
            gui.InstantiateTower(index);
        ShowTooltipMenu(index);
        if (SV.selection)
            gui.towerController.rangeInd.GetComponent<Renderer>().enabled = true;
    }

    void BuyTower()
    {
        gui.BuyTower();
        gui.CancelSelection(false);
        if (gui.towerController)
            gui.towerController.rangeInd.GetComponent<Renderer>().enabled = false;
        gui.StartCoroutine("FadeOut", panels.tooltip);
        delay = Time.time;
    }


    public void SellTower()
    {
        GameObject tower = gui.upgrade.transform.parent.gameObject;
        gui.SellTower(tower);
        SV.selection = tower;
        DisableMenus();
    }

    public void ShowUpgradeMenu(GameObject tower)
    {
        gui.StartCoroutine("FadeIn", panels.tooltip);
        gui.StartCoroutine("FadeIn", panels.upgradeMenu);
        SV.showUpgrade = true;
        if (gui.towerController)
            gui.towerController.rangeInd.GetComponent<Renderer>().enabled = false;
        if (SV.gridSelection)
        {
            SV.gridSelection.GetComponent<Renderer>().enabled = false;
            gui.StartCoroutine("FadeOut", buttons.towerButtons);
        }
        SV.gridSelection = null;
        gui.SetTowerComponents(tower);
        gui.towerController.rangeInd.GetComponent<Renderer>().enabled = true;
        UpdateUpgradeMenu();
        if(!IsInvoking("UpdateUpgradeMenu"))
            InvokeRepeating("UpdateUpgradeMenu", .5f, 1f);
    }


    void UpdateUpgradeMenu()
    {
        int curLvl = gui.upgrade.curLvl;
        UpgOptions upgOptions = gui.upgrade.options[curLvl];
        labels.headerName.text = gui.upgrade.gameObject.name;
        labels.properties.text = "Level:" + "\n" +
                                "Radius:" + "\n" +
                                "Damage:" + "\n" +
                                "Delay:" + "\n" +
                                "Targets:";
        labels.stats.text = +curLvl + "\n" +
                                (Mathf.Round(upgOptions.radius * 100f) / 100f) + "\n" +
                                (Mathf.Round(upgOptions.damage * 100f) / 100f) + "\n" +
                                (Mathf.Round(upgOptions.shootDelay * 100f) / 100f) + "\n" +
                                upgOptions.targetCount;
        if (labels.upgradeInfo)
        {
            if (curLvl < gui.upgrade.options.Count - 1)
            {
                UpgOptions nextUpg = gui.upgrade.options[curLvl + 1];
                labels.upgradeInfo.text = "= " + (curLvl + 1) + "\n" +
                                          "= " + (Mathf.Round(nextUpg.radius * 100f) / 100f) + "\n" +
                                          "= " + (Mathf.Round(nextUpg.damage * 100f) / 100f) + "\n" +
                                          "= " + (Mathf.Round(nextUpg.shootDelay * 100f) / 100f) + "\n" +
                                          "= " + nextUpg.targetCount;
            }
            else
                labels.upgradeInfo.text = "";
        }
        float sellPrice = gui.GetSellPrice();
        float upgradePrice = gui.GetUpgradePrice();
        bool affordable = true;
        labels.sellPrice.text = "" + sellPrice;
        if (!gui.AvailableUpgrade())
        {
            affordable = false;
            labels.price.text = "Cost: ";
        }
        else
            labels.price.text = "Cost: " + upgradePrice;
        if (affordable)
            affordable = gui.AffordableUpgrade();
        if (affordable)
            buttons.button_upgrade.SetActive(true);
        else
            buttons.button_upgrade.SetActive(false);
    }

    public void Upgrade()
    {
        GameObject tower = gui.upgrade.gameObject;
        gui.UpgradeTower();
        UpdateUpgradeMenu();
    }

    public void ShowTooltipMenu(int index)
    {
        gui.StartCoroutine("FadeIn", panels.tooltip);
        gui.StartCoroutine("FadeOut", panels.upgradeMenu);
        SV.showUpgrade = false;
        CancelInvoke("UpdateUpgradeMenu");
        TowerController baseOptions = null;
        UpgOptions upgOptions = null;
        if (SV.selection)
        {
            baseOptions = gui.towerController;
            upgOptions = gui.upgrade.options[0];
        }
        else
        {
            baseOptions = gui.towerManager.towerController[index];
            upgOptions = gui.towerManager.towerUpgrade[index].options[0];
        }
        labels.headerName.text = gui.towerManager.towerNames[index];
        labels.properties.text = "Projectile:" + "\n" +
                                "Radius:" + "\n" +
                                "Damage:" + "\n" +
                                "Delay:" + "\n";
        labels.stats.text = baseOptions.projectile.name + "\n" +
                                upgOptions.radius + "\n" +
                                upgOptions.damage + "\n" +
                                upgOptions.shootDelay + "\n";
        labels.price.text = "Cost: " + upgOptions.cost;
    }

    public void ExitMenu(int index)
    {
        SV.showExit = false;
        Time.timeScale = 1;
        gui.StartCoroutine("FadeIn", buttons.towerButtons);
        gui.StartCoroutine("FadeOut", panels.main);
        if (index == 1)
        {
            Application.LoadLevel(0);
        }
    }

    [System.Serializable]
    public class Panels
    {
        public GameObject main;         
        public GameObject upgradeMenu; 
        public GameObject tooltip;          
    }


    [System.Serializable]
    public class Buttons
    {   
        public GameObject towerButtons;    
        public GameObject button_sell;      
        public GameObject button_upgrade;  
        public GameObject button_abort;           
    }


    [System.Serializable]
    public class Labels
    {
        public Text headerName;      
        public Text properties;      
        public Text stats;           
        public Text upgradeInfo;   
        public Text price;         
        public Text sellPrice;   
    }
}
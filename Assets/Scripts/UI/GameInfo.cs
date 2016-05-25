using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameInfo : MonoBehaviour
{
    public WaveManager waveScript;
    public GameObject btn_startWave;
    public Text lbl_wave;
    public Text lbl_alive;
    public Text lbl_killed;
    public Text lbl_gold;
    public Text lbl_waveTimer;
    public Text lbl_resources;

    void Start()
    {

    }


    void Update()
    {
        bool nextWave = CheckWave();
        if (btn_startWave.activeInHierarchy != nextWave)
            btn_startWave.SetActive(nextWave);
        lbl_wave.text = GameHandler.wave + " / " + GameHandler.waveCount;
        lbl_alive.text = GameHandler.enemiesAlive.ToString();
        lbl_killed.text = GameHandler.enemiesKilled.ToString();
        lbl_gold.text = GameHandler.gameHealth.ToString();
        if (waveScript.secTillWave == 0)
            lbl_waveTimer.text = "";
        else
            lbl_waveTimer.text = waveScript.secTillWave.ToString();
            lbl_resources.text = GameHandler.resources.ToString();
    }

    private bool CheckWave()
    {
        if (SV.showExit || GameHandler.gameHealth <= 0)
            return false;
        if (waveScript.waveMode == WaveManager.WaveMode.normal
            && GameHandler.wave >= int.Parse(GameHandler.waveCount))
            return false;
        switch (waveScript.waveStartOption)
        { 
            case WaveManager.WaveStartOption.userInput:
                if (!waveScript.userInput)
                    return false;
                break;
            case WaveManager.WaveStartOption.waveCleared:
            case WaveManager.WaveStartOption.interval:
                if (GameHandler.wave > 0)
                    return false;
                break;
            case WaveManager.WaveStartOption.roundBased:
                if (waveScript.IsInvoking("CheckStatus"))
                    return false;
                break;
        }
        return true;
    }

    public void NextWave()
    {
        waveScript.StartWaves();
    }

}

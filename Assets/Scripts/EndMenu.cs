using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndMenu : MonoBehaviour
{
    public AudioClip endMusic;
    public Text result;
    public Text stats;
    public string mainSceneName;

    IEnumerator Start()
    {
        if (!GameObject.Find("Game Manager"))
        {
            Debug.LogWarning("EndMenu.cs can't find any game stats. Cancelling. Have you played the game?");
            yield break;
        }
        ConstructScene();
        if(endMusic)
            AudioManager.Play(endMusic, 100); //ustawic domyslna
    }

    void ConstructScene()
    {
        if (GameHandler.gameOver)
        {
            result.text = "GAME LOST!";
        }
        else
        {
            result.text = "GAME WON!\n\n";
        }
        stats.text = "Wave: " + GameHandler.wave + " / " + GameHandler.waveCount + "\n"
                + "Enemies alive: " + GameHandler.enemiesAlive + "\n"
                + "Enemies killed: " + GameHandler.enemiesKilled + "\n"
                + "Gold left: " + GameHandler.gameHealth + "\n"
                + "Resources: " + GameHandler.resources;
    }


    public void BackToMain()
    {
        Destroy(GameObject.Find("Game Manager"));
        Application.LoadLevel(mainSceneName);
    }
}
using UnityEngine;
using System.Collections;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;    //singleton
    public static float gameHealth = 1f;   //zycie zamku
    public static float resources;  //zasoby do kupowania wiez
    public static int enemiesAlive;  //przeciwnicy zyjacy
    public static int enemiesKilled;    //przeciwnicy zabici
    public static bool gameOver = false;    //flaga game over
    public static int wave = 0; //licznik fal przeciwnikow
    public static string waveCount = "oo"; //ile max fal na gre
    public static float maxHealth = 1f;
    public float maxGameHealth = 1f;
    public float startResources;

    public string nextScene;
    public string gameOverScene;


    void Awake()
    {
        instance = this;
        gameHealth = maxHealth = maxGameHealth;
        resources = startResources;
        enemiesAlive = 0;
        enemiesKilled = 0;
        wave = 0;
        waveCount = "oo";
        gameOver = false;
        InvokeRepeating("CheckGameState", 1f, 1f);
    }

   
    void CheckGameState()
    {
        if (gameHealth <= 0 && !gameOver)
        {
            Debug.Log(" YOU've LOST the Game! ");

            gameHealth = 0;
            gameOver = true;
            StartCoroutine("LoadGameOverScene");
        }
        else if (gameHealth > 0 && gameOver)
        {
            Debug.Log(" YOU've WON the Game! ");
            gameOver = false;
            StartCoroutine("LoadGameOverScene");
        }
    }

    public static void SetResources(float points)
    {
        resources += points;
    }

    public static void EnemyWasKilled()
    {
        enemiesAlive--;
        enemiesKilled++;
    }


    public static void AddHealth(float points)
    {
        gameHealth += points;
        if (gameHealth > maxHealth)
            gameHealth = maxHealth;
    }

    public static void DealDamage(float dmg)
    {
        if (gameOver) return;
        gameHealth -= dmg;
        enemiesAlive--;
    }

    IEnumerator LoadGameOverScene()
    {
        CancelInvoke("CheckGameState");
        if(gameOver || nextScene == "")
            nextScene = gameOverScene;

        if (nextScene == gameOverScene)
        {
            DontDestroyOnLoad(gameObject);
            yield return new WaitForSeconds(5);
            transform.parent = null;
        }
        Application.LoadLevel(nextScene);
    }
}

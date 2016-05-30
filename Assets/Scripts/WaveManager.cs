using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveManager : MonoBehaviour
{
    [HideInInspector]
    public List<WaveOptions> options = new List<WaveOptions>();

    public enum WaveMode
    {
        normal,         //fala zwykla
        endless,        //bez konca
        randomEndless   //randomowe fale bez konca
    }

    public WaveMode waveMode = WaveMode.normal;

    public enum WaveStartOption
    {
        waveCleared, 	//czekaj do zakonczenia poprzedniej
        interval,		//po czasie
        userInput,		//czekaj na usera az nacisnie
        roundBased		//kompilacja 2 poprzednich
    }

    public WaveStartOption waveStartOption = WaveStartOption.waveCleared;

    private int waveIndex = 0;
    public int secBetweenWaves;
    public int secIncrement;
    public bool autoStart;
    public bool autoCleanup;

    public EndlessOptions endlessOptions = new EndlessOptions();
    public WaveAnimations anims = new WaveAnimations();
    public WaveSounds sounds = new WaveSounds();

    [HideInInspector]
    public float secTillWave = 0;

    [HideInInspector]
    public bool userInput = true;


    void Start()
    {
        if (waveMode == WaveMode.normal)
            GameHandler.waveCount = options.Count.ToString();
        AudioManager.Play(sounds.backgroundMusic, 1.0f);
        if (autoStart)
            StartWaves();
    }

    public void StartWaves()
    {
        StartCoroutine(LaunchWave());
    }

    void CheckStatus()
    {
        if (GameHandler.gameHealth <= 0)
        {
            CancelInvoke("CheckStatus");
            return;
        }
        if (waveIndex < options.Count)
        {
            if (GameHandler.enemiesAlive > 0)
                return;
            switch (waveStartOption)
            {
                case WaveStartOption.waveCleared:
                    if (GameHandler.wave > 0)
                        secBetweenWaves += secIncrement;
                    StartCoroutine("WaveTimer", secBetweenWaves);
                    Invoke("StartWaves", secBetweenWaves);
                    break;
            }

            Debug.Log("Wave Defeated");
            AudioManager.Play(sounds.backgroundMusic, 1.0f);
            AudioManager.Play2D(sounds.waveEndSound, 1.0f);

        }
        else
        {
            if (GameHandler.enemiesAlive > 0)
                return;
            AudioManager.Play(sounds.backgroundMusic, 1.0f);

            AudioManager.Play2D(sounds.waveEndSound, 1.0f);
            GameHandler.gameOver = true;
        }
        CancelInvoke("CheckStatus");
    }

    IEnumerator LaunchWave()
    {
        Debug.Log("Wave " + waveIndex + " launched! StartUp GameTime: " + Time.time);
        if (autoCleanup)
            PoolManager.DestroyAllInactive(true);
        if(GameHandler.wave > 0)
            IncreaseSettings();
        AudioManager.Play(sounds.battleMusic, 0.05f);
        AudioManager.Play2D(sounds.waveStartSound, 1.0f);

        if (anims.spawnStart)
        {
            anims.objectToAnimate.GetComponent<Animation>().Play(anims.spawnStart.name);
            yield return new WaitForSeconds(anims.spawnStart.length);
        }
        for (int i = 0; i < options[waveIndex].enemyPrefab.Count; i++)
        {
            StartCoroutine(SpawnEnemyWave(i));
        }
        float lastSpawn = GetLastSpawnTime(waveIndex);
        if (IsInvoking("CheckStatus"))
            CancelInvoke("CheckStatus");
        switch (waveStartOption)
        {
            case WaveStartOption.interval:
                if (GameHandler.wave > 0)
                    secBetweenWaves += secIncrement;
                if (waveMode != WaveMode.normal || waveIndex + 1 < options.Count)
                {
                    StartCoroutine("WaveTimer", secBetweenWaves);
                    Invoke("StartWaves", secBetweenWaves);
                }
                break;
                
            case WaveStartOption.userInput:
                userInput = false;
                Invoke("ToggleInput", lastSpawn);
                break;
        }
        InvokeRepeating("CheckStatus", lastSpawn, 2f);
        Invoke("PlaySpawnEndAnimation", lastSpawn);
        GameHandler.wave++;
        switch (waveMode)
        {
            case WaveMode.normal:
                waveIndex++;
                break;
            case WaveMode.endless:
                waveIndex++;
                if (waveIndex == options.Count)
                    waveIndex = 0;
                break;
            case WaveMode.randomEndless:
                int rnd = waveIndex;
                while(rnd == waveIndex)
                    waveIndex = Random.Range(0, options.Count);
                break;
        }
    }

    void PlaySpawnEndAnimation()
    {
        if (anims.spawnEnd)
        {
            anims.objectToAnimate.GetComponent<Animation>().Play(anims.spawnEnd.name);
        }
    }

    void ToggleInput()
    {
        userInput = true;
    }

    IEnumerator WaveTimer(float seconds)
    {
        float timer = Time.time + seconds;
        while (Time.time < timer)
        {
            secTillWave = Mathf.Round((timer - Time.time) * 100f) / 100f;
            yield return true;
        }
        secTillWave = 0f;
    }
    IEnumerator SpawnEnemyWave(int index)
    {
        int waveNo = waveIndex;
        yield return new WaitForSeconds(Random.Range(options[waveNo].startDelayMin[index], options[waveNo].startDelayMax[index]));
        for (int j = 0; j < options[waveNo].enemyCount[index]; j++)
        {
            if (options[waveNo].enemyPrefab[index] == null)
            {
                Debug.LogWarning("Enemy Prefab not set in Wave Editor!");
                yield return null;
            }
            if (options[waveNo].path[index] == null)
            {
                Debug.LogWarning(options[waveNo].enemyPrefab[index].name + " has no path! Please set Path Container.");
                break;
            }
            SpawnEnemy(waveNo, index);
            yield return new WaitForSeconds(Random.Range(options[waveNo].delayBetweenMin[index],options[waveNo].delayBetweenMax[index]));
        }
    }


    void SpawnEnemy(int waveNo, int index)
    {
        GameObject prefab = options[waveNo].enemyPrefab[index];
        Vector3 position = options[waveNo].path[index].waypoints[0].position;
        GameObject enemy = PoolManager.Pools["Enemies"].Spawn(prefab, position, Quaternion.identity);
        if (!enemy) return;
        enemy.GetComponentInChildren<TweenMove>().pathContainer = options[waveNo].path[index];
        Properties prop = enemy.GetComponentInChildren<Properties>();
        if (options[waveNo].enemyHP[index] > 0)
            prop.health = options[waveNo].enemyHP[index];
        GameHandler.enemiesAlive++;
    }

    float GetLastSpawnTime(int wave)
    {
        float lastSpawn = 1;
        for (int i = 0; i < options[wave].enemyCount.Count; i++)
        {
            float result = options[wave].startDelayMax[i] + (options[wave].enemyCount[i] - 1) * options[wave].delayBetweenMax[i];
            if (result > lastSpawn)
            {
                lastSpawn = result + 0.25f;
            }
        }
        return lastSpawn;
    }

    void IncreaseSettings()
    {
        switch (waveMode)
        {
            case WaveMode.normal:
                return;
            case WaveMode.endless:
                if (GameHandler.wave % options.Count != 0)
                    return;
                break;
        }
        for (int i = 0; i < options.Count; i++)
        {
            WaveOptions option = options[i];
            for (int j = 0; j < option.enemyPrefab.Count; j++)
            {
                if (endlessOptions.increaseAmount.enabled)
                {
                    if (endlessOptions.increaseAmount.type == TDValue.fix)
                        option.enemyCount[j] += (int)endlessOptions.increaseAmount.value;
                    else
                        option.enemyCount[j] = Mathf.CeilToInt(option.enemyCount[j] * endlessOptions.increaseAmount.value);
                }
                if (endlessOptions.increaseHP.enabled)
                {
                    if (option.enemyHP[j] == 0)
                        option.enemyHP[j] = option.enemyPrefab[j].GetComponent<Properties>().health;
                    if (endlessOptions.increaseHP.type == TDValue.fix)
                        option.enemyHP[j] += endlessOptions.increaseHP.value;
                    else
                        option.enemyHP[j] = Mathf.Round(option.enemyHP[j] * endlessOptions.increaseHP.value
                                            * 100f) / 100f;
                }
            }
        }
    }
}


public enum TDValue
{
    fix,
    percentual
}

[System.Serializable]
public class EndlessOptions
{
    public Setting increaseHP = new Setting();
    public Setting increaseAmount = new Setting();
    [System.Serializable]
    public class Setting
    {
        public bool enabled;
        public TDValue type;
        public float value;
    }
}

[System.Serializable]
public class WaveAnimations
{
    public GameObject objectToAnimate;
    public AnimationClip spawnStart;
    public AnimationClip spawnEnd;
}


[System.Serializable]
public class WaveSounds
{
    public AudioClip waveStartSound;
    public AudioClip battleMusic;
    public AudioClip waveEndSound;
    public AudioClip backgroundMusic;
}

[System.Serializable]
public class WaveOptions
{
    public List<GameObject> enemyPrefab = new List<GameObject>();
    public List<float> enemyHP = new List<float>();
    public List<int> enemyCount = new List<int>();
    public List<float> startDelayMin = new List<float>();
    public List<float> startDelayMax = new List<float>();
    public List<float> delayBetweenMin = new List<float>();
    public List<float> delayBetweenMax = new List<float>();
    public List<PathManager> path = new List<PathManager>();
}
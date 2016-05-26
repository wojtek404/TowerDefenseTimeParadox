using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//custom wave editor window
public class WaveEditor : EditorWindow
{
    [SerializeField]
    WaveManager waveScript;
    Vector2 scrollPos;

    private static WaveEditor waveEditor;

    [MenuItem("Window/Tower Defense: Time Paradox/Wave Settings")]
    static void Init()
    {
        waveEditor = (WaveEditor)EditorWindow.GetWindowWithRect(typeof(WaveEditor), new Rect(0,0,800,400), false, "Wave Settings");
        waveEditor.autoRepaintOnSceneChange = true;
    }


    void OnGUI()
    {
        GameObject wavesGO = GameObject.Find("Wave Manager");
        if (wavesGO == null)
        {
            Debug.LogError("Current Scene contains no Wave Manager.");
            waveEditor.Close();
            return;
        }
        waveScript = wavesGO.GetComponent<WaveManager>();
        if (waveScript == null)
        {
            Debug.LogWarning("No Wave Manager Component found!");
            waveEditor.Close();
            return;
        }
        bool waveChange = false;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+++ Add Wave +++"))
        {
            Undo.RecordObject(waveScript, "AddWave");
            WaveOptions newWave = new WaveOptions();
            newWave.enemyPrefab.Add(null);
            newWave.enemyHP.Add(0);
            newWave.enemyCount.Add(1);
            newWave.startDelayMin.Add(0);
            newWave.startDelayMax.Add(0);
            newWave.delayBetweenMin.Add(0);
            newWave.delayBetweenMax.Add(0);
            newWave.path.Add(null);
            waveScript.options.Add(newWave);
            waveChange = true;
        }

        if (GUILayout.Button("Delete all Waves"))
        {
            Undo.RecordObject(waveScript, "DeleteWaves");
            waveScript.options.Clear();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(370));

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("No. |", EditorStyles.boldLabel, GUILayout.Width(40));
        GUILayout.Label("      Prefab      ", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("| Overwrite | Overwrite", EditorStyles.boldLabel, GUILayout.Width(155));
        GUILayout.Label("|  Count  |", EditorStyles.boldLabel, GUILayout.Width(70));
        GUILayout.Label(" Spawn-Delay  |", EditorStyles.boldLabel, GUILayout.Width(110));
        GUILayout.Label("Delay-Between |", EditorStyles.boldLabel, GUILayout.Width(115));
        GUILayout.Label("      Path      ", EditorStyles.boldLabel, GUILayout.Width(90));
        GUILayout.Label("| DELETE", EditorStyles.boldLabel, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(170);
        GUILayout.Label("Health", EditorStyles.boldLabel, GUILayout.Width(75));
        GUILayout.Label("Shield", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Space(70);
        GUILayout.Label("From-To", EditorStyles.boldLabel, GUILayout.Width(110));
        GUILayout.Label("From-To", EditorStyles.boldLabel, GUILayout.Width(125));
        EditorGUILayout.EndHorizontal();

        if (waveScript.options.Count == 0)
        {
            EditorGUILayout.EndScrollView();
            return;
        } 

        EditorGUILayout.Space();

        for (int i = 0; i < waveScript.options.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label((i+1) + "", GUILayout.Width(90));
            GUILayout.Space(580);

            if (GUILayout.Button("X"))
            {
                Undo.RecordObject(waveScript, "DeleteWave");
                waveScript.options.RemoveAt(i);
                TrackChange(true);
                return;
            }
            
            EditorGUILayout.EndHorizontal();

            for (int j = 0; j < waveScript.options[i].enemyPrefab.Count; j++)
            {
                if (waveScript.waveStartOption == WaveManager.WaveStartOption.interval)
                {
                    float finalDelay = waveScript.options[i].startDelayMax[j] + 
                                       (waveScript.options[i].enemyCount[j] - 1) * waveScript.options[i].delayBetweenMax[j];
                    float allowedDelay = waveScript.secBetweenWaves + i * waveScript.secIncrement;

                    if (finalDelay > allowedDelay && waveScript.options[i].enemyPrefab[j] != null)
                    {
                        Debug.LogWarning("Delay of Enemy " + waveScript.options[i].enemyPrefab[j].name + ", Wave " +
                                         (i + 1) + " exceeds Delay between Waves");
                    }
                }

                for (int h = waveScript.options[i].enemyHP.Count; h < waveScript.options[i].enemyPrefab.Count; h++)
                    waveScript.options[i].enemyHP.Add(0);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);
                waveScript.options[i].enemyPrefab[j] = (GameObject)EditorGUILayout.ObjectField(waveScript.options[i].enemyPrefab[j], typeof(GameObject), false, GUILayout.Width(115));
                GUILayout.Space(15);
                waveScript.options[i].enemyHP[j] = EditorGUILayout.FloatField(waveScript.options[i].enemyHP[j], GUILayout.Width(50));
                GUILayout.Space(25);
                waveScript.options[i].enemyCount[j] = EditorGUILayout.IntField(waveScript.options[i].enemyCount[j], GUILayout.Width(40));
                GUILayout.Space(35);
                waveScript.options[i].startDelayMin[j] = EditorGUILayout.FloatField(waveScript.options[i].startDelayMin[j], GUILayout.Width(25));
                waveScript.options[i].startDelayMax[j] = EditorGUILayout.FloatField(waveScript.options[i].startDelayMax[j], GUILayout.Width(25));
                GUILayout.Space(55);
                waveScript.options[i].delayBetweenMin[j] = EditorGUILayout.FloatField(waveScript.options[i].delayBetweenMin[j], GUILayout.Width(25));
                waveScript.options[i].delayBetweenMax[j] = EditorGUILayout.FloatField(waveScript.options[i].delayBetweenMax[j], GUILayout.Width(25));
                GUILayout.Space(35);
                waveScript.options[i].path[j] = (PathManager)EditorGUILayout.ObjectField(waveScript.options[i].path[j], typeof(PathManager), true, GUILayout.Width(105));
                if (GUILayout.Button("X"))
                {
                    Undo.RecordObject(waveScript, "DeleteRow");
                    waveScript.options[i].enemyPrefab.RemoveAt(j);
                    waveScript.options[i].enemyHP.RemoveAt(j);
                    waveScript.options[i].enemyCount.RemoveAt(j);
                    waveScript.options[i].startDelayMin.RemoveAt(j);
                    waveScript.options[i].startDelayMax.RemoveAt(j);
                    waveScript.options[i].delayBetweenMin.RemoveAt(j);
                    waveScript.options[i].delayBetweenMax.RemoveAt(j);
                    waveScript.options[i].path.RemoveAt(j);

                    if (waveScript.options[i].enemyPrefab.Count == 0)
                        waveScript.options.RemoveAt(i);
                    TrackChange(true);
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Enemy"))
            {
                Undo.RecordObject(waveScript, "AddRow");
                waveScript.options[i].enemyPrefab.Add(null);
                waveScript.options[i].enemyHP.Add(0);
                waveScript.options[i].enemyCount.Add(1);
                waveScript.options[i].startDelayMin.Add(0);
                waveScript.options[i].startDelayMax.Add(0);
                waveScript.options[i].delayBetweenMin.Add(0);
                waveScript.options[i].delayBetweenMax.Add(0);
                waveScript.options[i].path.Add(null);
                waveChange = true;
            }
            if (i < (waveScript.options.Count-1) && GUILayout.Button("Insert Wave"))
            {
                Undo.RecordObject(waveScript, "AddWave");
                WaveOptions newWave = new WaveOptions();
                newWave.enemyPrefab.Add(null);
                newWave.enemyHP.Add(0);
                newWave.enemyCount.Add(1);
                newWave.startDelayMin.Add(0);
                newWave.startDelayMax.Add(0);
                newWave.delayBetweenMin.Add(0);
                newWave.delayBetweenMax.Add(0);
                newWave.path.Add(null);
                waveScript.options.Insert(i+1, newWave);
                waveChange = true;
            }

            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        TrackChange(waveChange);
    }


    void TrackChange(bool waveChange)
    {
        if (GUI.changed || waveChange)
        {
            EditorUtility.SetDirty(waveScript);
            Repaint();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelSceneSelection;
    public GameObject panelLoading;

    public Text progressText;
    public Slider progressSlider;
    public AudioClip introMusic;
    private string sceneName;

    void Start()
    {
        if(introMusic)
            AudioManager.Play(introMusic, 100); //moze zmniejszyc
        panelMenu.SetActive(false);
        panelLoading.SetActive(false);
        panelSceneSelection.SetActive(false);
        StartCoroutine("FadeIn", panelMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ActivateMenu()
    {
        StartCoroutine("FadeOut", panelSceneSelection);
        StartCoroutine("FadeIn", panelMenu);
    }

    public void ActivateSceneSelection()
    {
        StartCoroutine("FadeOut", panelMenu);
        StartCoroutine("FadeIn", panelSceneSelection);
    }

    public void LoadButton(string sceneName)
    {
        this.sceneName = sceneName;
        if (!IsInvoking("LoadGame"))
        {
            InvokeRepeating("LoadGame", 0f, 0.2f);
            //StartCoroutine("FadeOut", panelSceneSelection);
            //StartCoroutine("FadeIn", panelLoading);
            panelSceneSelection.SetActive(false);
            panelLoading.SetActive(true);
        }
    }

    void LoadGame()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        if (asyncOperation.isDone)
        {
            progressSlider.value = 1f;
            progressText.text = 100 + "%";
        }
        else
        {
            float progressValue = asyncOperation.progress;
            if (progressValue > 0.01f)
            {
                progressSlider.transform.Find("Fill Area").gameObject.SetActive(true);
                progressSlider.value = progressValue;
                progressText.text = ((int)(progressValue * 100)) + "%";
            }
            else
            {
                progressSlider.transform.Find("Fill Area").gameObject.SetActive(false);
                progressText.text = "Loading...";
            }
        }
    }

    IEnumerator FadeIn(GameObject gObj)
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

    IEnumerator FadeOut(GameObject gObj)
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
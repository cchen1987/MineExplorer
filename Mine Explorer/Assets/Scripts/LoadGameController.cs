using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGameController : MonoBehaviour {

    public Slider progressBar;
    public Text progressText;

    public Sprite dayBackground;
    public Sprite sunsetBackground;
    public Sprite nightBackground;

    public Image background;

    // Use this for initialization
    void Start()
    {
        if (System.DateTime.Now.Hour >= 6 && System.DateTime.Now.Hour < 18)
        {
            background.overrideSprite = dayBackground;
        }
        else if (System.DateTime.Now.Hour >= 18 && System.DateTime.Now.Hour < 18)
        {
            background.overrideSprite = sunsetBackground;
        }
        else
        {
            background.overrideSprite = nightBackground;
        }
        StartCoroutine(LoadNewScene());
    }

    IEnumerator LoadNewScene()
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(1);

        while (!loadScene.isDone)
        {
            progressBar.value = loadScene.progress;
            progressText.text = (progressBar.value * 100).ToString("0") + " %";
            //Debug.Log("Progreso " + progressBar.value);
            //Debug.Log("Progreso2 " + loadScene.progress);
            yield return null;
        }
        progressBar.value = 100;
        progressText.text = "100 %";
    }
}

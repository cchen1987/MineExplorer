using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour {
    
    public Slider progressBar;
    public Text progressText;
    
    // Use this for initialization
    void Start ()
    {
        StartCoroutine(LoadNewScene());
    }

    IEnumerator LoadNewScene()
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(3);

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

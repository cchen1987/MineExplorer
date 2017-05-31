using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {

    public GameObject rowSlider;
    public GameObject colSlider;
    public GameObject mineSlider;
    public GameObject timeEffect;
    public GameObject decoration;
    public GameObject shadow;

    private void Start()
    {
        timeEffect.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("light") == 1;
        decoration.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("environment") == 1;
        shadow.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("shadow") == 1;
    }

    public void StartGameBtn(string scene)
    {
        PlayerPrefs.SetInt("light", timeEffect.GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("environment", decoration.GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("shadow", shadow.GetComponent<Toggle>().isOn ? 1 : 0);

        SceneManager.LoadScene(scene);
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Game modes, 0 -> classic mode, 1 -> survival mode
    /// </summary>
    /// <param name="mode"></param>
    public void SetGameMode(int mode)
    {
        PlayerPrefs.SetInt("mode", mode);
    }
    
    public void SetRows(int rows)
    {
        PlayerPrefs.SetInt("rows", rows);
    }

    public void SetCols(int cols)
    {
        PlayerPrefs.SetInt("columns", cols);
    }

    public void SetBombs(int bombs)
    {
        PlayerPrefs.SetInt("bombs", bombs);
    }

    public void SetTimeLimit(int time)
    {
        PlayerPrefs.SetInt("time", time);
    }

    public void SetCustomTable()
    {
        PlayerPrefs.SetInt("rows", (int)rowSlider.GetComponent<Slider>().value);
        PlayerPrefs.SetInt("columns", (int)colSlider.GetComponent<Slider>().value);
        PlayerPrefs.SetInt("bombs", (int)mineSlider.GetComponent<Slider>().value);
    }

    public void SetDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("difficulty", difficulty);
    }
}

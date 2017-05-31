using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IntroTouchPad : MonoBehaviour, IPointerDownHandler,
    IPointerUpHandler
{
    private RaycastHit hit;
    private Ray ray;
    public GameObject controlBlock;
    public GameObject blocksContainer;

    public GameObject gameModeBlock;
    public GameObject difficultyBlocks;
    public GameObject classicBlock;
    public GameObject survivalBlock;

    public GameObject titles;
    public GameObject difficultyTextContainer;
    
    public GameObject exitPanel;
    public GameObject customSettingsPanel;
    public GameObject instructionPanel;
    public ButtonManager buttonManager;

    public GameObject explosionPlay;
    public GameObject explosionClassic;
    public GameObject explosionSurvival;

    public AudioSource explosionSound;
    public AudioSource daySound;
    public AudioSource nightSound;

    public GameObject[] titleLights;
    public GameObject gameModeLight;
    private GameStatus.Mode mode;

    public void OnPointerDown(PointerEventData eventData)
    {
        ray = Camera.main.ScreenPointToRay(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!customSettingsPanel.activeInHierarchy && !exitPanel.activeInHierarchy &&
            Physics.Raycast(ray, out hit, 4000, LayerMask.GetMask("Buttons")))
        {
            string objectTag = hit.transform.tag;
            Debug.Log(objectTag);
            switch(objectTag)
            {
                case "Play":
                    GameObject.Find("PlayBlock").AddComponent<BoxCollider>();
                    GameObject.Find("PlayBlock").AddComponent<Rigidbody>();
                    GameObject.Find("ScoreBlock").AddComponent<BoxCollider>();
                    GameObject.Find("ScoreBlock").AddComponent<Rigidbody>();
                    GameObject.Find("HelpBlock").AddComponent<BoxCollider>();
                    GameObject.Find("HelpBlock").AddComponent<Rigidbody>();
                    GameObject.Find("ExitBlock").AddComponent<BoxCollider>();
                    GameObject.Find("ExitBlock").AddComponent<Rigidbody>();

                    GameObject blocks = GameObject.Find("Blocks");
                    int count = blocks.transform.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        if (blocks.transform.GetChild(i).gameObject.GetComponent<BoxCollider>() == null)
                        {
                            blocks.transform.GetChild(i).gameObject.AddComponent<BoxCollider>();
                            blocks.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
                            blocks.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().mass = 10;
                        }
                    }
                    titles.SetActive(false);
                    foreach (GameObject light in titleLights)
                        if (light.activeInHierarchy)
                            light.SetActive(false);

                    if (daySound.isPlaying)
                        daySound.Stop();
                    if (nightSound.isPlaying)
                        nightSound.Stop();
                    explosionPlay.SetActive(true);
                    explosionSound.Play();
                    GameObject.Find("GameModeBlocks").GetComponent<Animation>().StartMoveAnimation();
                    
                    break;
                case "Score":
                    buttonManager.LoadScene("PointsScreen");
                    break;
                case "Help":
                    instructionPanel.SetActive(true);
                    break;
                case "Exit":
                    exitPanel.SetActive(true);
                    break;
                case "Classic":
                    mode = GameStatus.Mode.CLASSIC;
                    AddRigidBody();
                    if (gameModeLight.activeInHierarchy)
                        gameModeLight.SetActive(false);
                    
                    explosionClassic.SetActive(true);
                    explosionSound.Play();
                    difficultyTextContainer.SetActive(true);
                    difficultyBlocks.SetActive(true);
                    difficultyBlocks.GetComponent<Animation>().StartDifficultyFadeAnimation();
                    difficultyTextContainer.GetComponent<Animation>().StartDifficultyFadeAnimation();
                    break;
                case "Survival":
                    mode = GameStatus.Mode.SURVIVAL;
                    AddRigidBody();
                    if (gameModeLight.activeInHierarchy)
                        gameModeLight.SetActive(false);
                    
                    explosionSurvival.SetActive(true);
                    explosionSound.Play();
                    difficultyTextContainer.SetActive(true);
                    difficultyBlocks.SetActive(true);
                    difficultyBlocks.GetComponent<Animation>().StartSurvivalFadeAnimation();
                    difficultyTextContainer.GetComponent<Animation>().StartSurvivalFadeAnimation();
                    break;
                case "Beginner":
                    buttonManager.SetGameMode(mode == GameStatus.Mode.CLASSIC ? 0 : 1);
                    buttonManager.SetRows(9);
                    buttonManager.SetCols(9);
                    buttonManager.SetBombs(10);
                    buttonManager.SetTimeLimit(60);
                    buttonManager.SetDifficulty("beginner");
                    buttonManager.StartGameBtn("loadScreen");
                    break;
                case "Intermediate":
                    buttonManager.SetGameMode(mode == GameStatus.Mode.CLASSIC ? 0 : 1);
                    buttonManager.SetRows(16);
                    buttonManager.SetCols(16);
                    buttonManager.SetBombs(30);
                    buttonManager.SetTimeLimit(180);
                    buttonManager.SetDifficulty("intermediate");
                    buttonManager.StartGameBtn("loadScreen");
                    break;
                case "Expert":
                    buttonManager.SetGameMode(mode == GameStatus.Mode.CLASSIC ? 0 : 1);
                    buttonManager.SetRows(16);
                    buttonManager.SetCols(30);
                    buttonManager.SetBombs(100);
                    buttonManager.SetTimeLimit(420);
                    buttonManager.SetDifficulty("expert");
                    buttonManager.StartGameBtn("loadScreen");
                    break;
                case "Custom":
                    buttonManager.SetGameMode(0);
                    customSettingsPanel.SetActive(true);
                    break;
                case "Cancel":
                    buttonManager.LoadScene("StartScene 1");
                    break;
            }
        }
    }
    
    private void AddRigidBody()
    {
        for (int i = 0; i < 2; i++)
        {
            classicBlock.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
            classicBlock.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().mass = 10;
            survivalBlock.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
            survivalBlock.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().mass = 10;
        }
        GameObject.Find("ClassicText").SetActive(false);
        GameObject.Find("SurvivalText").SetActive(false);
    }
    
    private void FixedUpdate()
    {
        if (controlBlock.transform.position.y < -200)
        {
            blocksContainer.SetActive(false);
        }

        if (gameModeBlock.activeInHierarchy && classicBlock.transform.GetChild(0).transform.position.y < -50)
        {
            gameModeBlock.SetActive(false);
        }
    }
}

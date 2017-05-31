using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation : MonoBehaviour {
    
    public bool endFade;
    public bool endMove;
    public bool difficultyEndFade;
    public bool survivalEndFade;

    private void Start()
    {
        endFade = false;
        endMove = false;
        difficultyEndFade = false;
        survivalEndFade = false;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (endFade)
        {
            gameObject.GetComponent<Animator>().SetTrigger("EndFade");
        }
        if (endMove)
        {
            gameObject.GetComponent<Animator>().SetTrigger("EndMove");
        }
        if (difficultyEndFade)
        {
            gameObject.GetComponent<Animator>().SetTrigger("EndFade");
        }
        if (survivalEndFade)
        {
            gameObject.GetComponent<Animator>().SetTrigger("SurvivalEndFade");
        }
    }

    public void StartAnimation()
    {
        gameObject.GetComponent<Animator>().SetTrigger("Fade");
    }

    public void StartMoveAnimation()
    {
        gameObject.GetComponent<Animator>().SetTrigger("StartMove");
    }

    public void StartDifficultyFadeAnimation()
    {
        gameObject.GetComponent<Animator>().SetTrigger("StartFade");
    }

    public void StartSurvivalFadeAnimation()
    {
        gameObject.GetComponent<Animator>().SetTrigger("SurvivalFade");
    }
}

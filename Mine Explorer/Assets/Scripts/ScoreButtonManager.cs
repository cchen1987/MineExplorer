using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreButtonManager : MonoBehaviour {

    public GameObject beginnerButton;
    public GameObject intermediateButton;
    public GameObject expertButton;
    public GameObject customButton;

    private Color32 backgroundColor;
    private Color32 textColor;
    private Color32 highlightedBgColor;
    private Color32 highlightedTextColor;

    private void Start()
    {
        beginnerButton = GameObject.Find("BeginnerButton");
        intermediateButton = GameObject.Find("IntermediateButton");
        expertButton = GameObject.Find("ExpertButton");
        customButton = GameObject.Find("CustomButton");

        backgroundColor = new Color32(43, 43, 43, 255);
        textColor = new Color32(171, 171, 171, 255);
        highlightedBgColor = new Color32(107, 107, 107, 255);
        highlightedTextColor = Color.white;
    }

    public void HighlightButton()
    {
        beginnerButton.GetComponent<Image>().color = backgroundColor;
        beginnerButton.transform.GetChild(0).GetComponent<Text>().color = textColor;
        intermediateButton.GetComponent<Image>().color = backgroundColor;
        intermediateButton.transform.GetChild(0).GetComponent<Text>().color = textColor;
        expertButton.GetComponent<Image>().color = backgroundColor;
        expertButton.transform.GetChild(0).GetComponent<Text>().color = textColor;
        customButton.GetComponent<Image>().color = backgroundColor;
        customButton.transform.GetChild(0).GetComponent<Text>().color = textColor;
        gameObject.GetComponent<Image>().color = highlightedBgColor;
        gameObject.transform.GetChild(0).GetComponent<Text>().color = highlightedTextColor;
    }
}

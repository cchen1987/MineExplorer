using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderConfiguration : MonoBehaviour {

    public Text text;
    public Slider slider;
    public ButtonManager buttonManager;

    private Slider rows;
    private Slider columns;
    private bool isMineSlider;

    private void Start()
    {
        if (gameObject.name == "Mines")
        {
            rows = GameObject.Find("Rows").GetComponent<Slider>();
            columns = GameObject.Find("Columns").GetComponent<Slider>();
            isMineSlider = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (isMineSlider)
        {
            slider.maxValue = rows.value * columns.value / 2;
        }
        text.text = slider.value.ToString();
	}
}

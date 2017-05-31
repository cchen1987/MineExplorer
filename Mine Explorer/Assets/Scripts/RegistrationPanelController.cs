using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistrationPanelController : MonoBehaviour {

    private void Awake()
    {
        //Debug.Log("Modo: " + PlayerPrefs.GetInt("modo"));
        //Debug.Log("id: " + PlayerPrefs.GetInt("id"));
        //PlayerPrefs.DeleteKey("id");
        if (PlayerPrefs.GetInt("id") != 0)
            gameObject.SetActive(false);
    }
}

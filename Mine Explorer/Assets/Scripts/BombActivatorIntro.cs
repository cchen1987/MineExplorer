using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombActivatorIntro : MonoBehaviour {
    
    public bool isBombTimerActivated;
    public GameObject explosion;
    public GameObject animatedTitles;
    public Animation titleAnimation;
    public AudioSource daySound;
    public AudioSource nightSound;
    private bool showTitle;

    private void Awake()
    {
        isBombTimerActivated = false;
        showTitle = false;
    }

    private void Update()
    {
        if (isBombTimerActivated)
        {
            StartCoroutine(Timer());
        }
        if (gameObject.transform.parent.transform.position.y <= -10)
        {
            titleAnimation.StartAnimation();
            animatedTitles.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Floor")
        {
            isBombTimerActivated = true;
        }
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1.5f);
        explosion.SetActive(true);
        if (daySound.isPlaying)
            daySound.Stop();
        if (nightSound.isPlaying)
            nightSound.Stop();
    }
}

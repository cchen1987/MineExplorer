using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour {
    
    public Material nightSky;
    public Material daySky;
    public Material sunsetSky;

    private GameObject sun;
    public AudioSource daySound;
    public AudioSource nightSound;
    public AudioSource riverSound;

    public GameObject blocksContainer;

    public Material randomMaterial;
    private Material randomCoverMaterial;
    private Material randomBaseMaterial;
    private Color32 coverColor;
    private Color32 baseColor;

    public GameObject[] spotLights;

    public GameObject[] customBlocks;

    // Use this for initialization
    void Start () {
        LoadTimeEffect();
        LoadSceneColors();
    }

    private void LoadSceneColors()
    {
        byte green = (byte)Random.Range(25, 35);
        baseColor = new Color32(
            (byte)Random.Range(60, 80),
            green,
            (byte)(green - 20),
            255
            );

        randomCoverMaterial = new Material(randomMaterial);
        randomBaseMaterial = new Material(randomMaterial);

        randomBaseMaterial.color = baseColor;

        int count = blocksContainer.transform.childCount;
        for (int i = 0; i < count; i++)
            AssignRandomMaterialColor(blocksContainer.transform.GetChild(i).gameObject);

        count = customBlocks.Length;
        for (int i = 0; i < count; i++)
            AssignRandomMaterialColor(customBlocks[i]);
    }

    private void AssignRandomMaterialColor(GameObject currentBlock)
    {
        byte red = (byte)Random.Range(25, 45);
        coverColor = new Color32(
            red,
            (byte)Random.Range(80, 150),
            (byte)(red - 5),
            255
            );

        randomCoverMaterial.color = coverColor;

        Debug.Log(coverColor);

        GameObject cover;
        if (currentBlock.transform.FindChild("Cover") != null)
        {
            cover = currentBlock.transform.FindChild("Cover").gameObject;
            cover.GetComponent<MeshRenderer>().material = randomCoverMaterial;
        }

        GameObject baseBlock = currentBlock.transform.FindChild("Base").gameObject;
        baseBlock.GetComponent<MeshRenderer>().material = randomBaseMaterial;
    }

    private void LoadTimeEffect()
    {
        sun = GameObject.Find("Sun");
        if (System.DateTime.Now.Hour >= 21 || System.DateTime.Now.Hour < 6)
        {
            nightSound.Play();
            sun.transform.rotation = Quaternion.Euler(125, -160, -30);
            sun.GetComponent<Light>().color = Color.gray;
            sun.GetComponent<Light>().shadowStrength = 0.85f;
            RenderSettings.skybox = nightSky;
            foreach (GameObject light in spotLights)
                light.SetActive(true);
        }
        else
        {
            if (System.DateTime.Now.Hour >= 6 && System.DateTime.Now.Hour < 9)
            {
                sun.transform.rotation = Quaternion.Euler(30, -40, 0);
                sun.GetComponent<Light>().shadowStrength = 0.65f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 9 && System.DateTime.Now.Hour < 12)
            {
                sun.transform.rotation = Quaternion.Euler(60, -60, 0);
                sun.GetComponent<Light>().intensity = 1.2f;
                sun.GetComponent<Light>().shadowStrength = 0.55f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 12 && System.DateTime.Now.Hour < 16)
            {
                sun.transform.rotation = Quaternion.Euler(115, -180, -75);
                sun.GetComponent<Light>().intensity = 1.5f;
                sun.GetComponent<Light>().shadowStrength = 0.4f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 16 && System.DateTime.Now.Hour < 18)
            {
                sun.transform.rotation = Quaternion.Euler(120, -230, 0);
                sun.GetComponent<Light>().intensity = 1.2f;
                sun.GetComponent<Light>().shadowStrength = 0.55f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 18 && System.DateTime.Now.Hour < 21)
            {
                sun.transform.rotation = Quaternion.Euler(150, -120, 0);
                sun.GetComponent<Light>().shadowStrength = 0.65f;
                RenderSettings.skybox = sunsetSky;
            }
            daySound.Play();
        }
    }
}

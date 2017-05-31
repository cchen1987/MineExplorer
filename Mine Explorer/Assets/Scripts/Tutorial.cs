using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    private GameObject cameraController;
    public Block topLeftBlock;
    public Block bottomRightBlock;
    public Block middleBlock;

    public GameObject emptyBlockContainer;
    public GameObject mineContainer;
    public GameObject blocksContainer;
    
    // Use this for initialization
    void Start ()
    {
        cameraController = GameObject.Find("CameraController");
        cameraController.transform.position = new Vector3(
            middleBlock.transform.position.x,
            cameraController.transform.position.y,
            cameraController.transform.position.z
            );
        cameraController.GetComponent<CameraController>().SetTopLeftMapCorner(topLeftBlock.transform.position);
        cameraController.GetComponent<CameraController>().SetBottomRightCorner(bottomRightBlock.transform.position);

        for (int i = 0; i < mineContainer.transform.childCount; i++)
        {
            mineContainer.transform.GetChild(i).GetComponent<Block>().SetBomb();
        }

        for (int i = 0; i < blocksContainer.transform.childCount; i++)
        {
            blocksContainer.transform.GetChild(i).GetComponent<Block>().SetNumber();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

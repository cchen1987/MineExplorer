using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialTouchpad : MonoBehaviour, IPointerDownHandler,
    IPointerUpHandler, IDragHandler
{
    private GameObject mainCamera;
    private GameObject mainCameraGameObject;
    private CameraController mainCameraController;
    private Transform pointerStartPosition;
    private const float CAMERA_MOVEMENT_ADJUST = 0.4f;
    private const float CAMERA_SPEEDX_ADJUST = 0.3f;
    private const float CAMERA_SPEEDY_ADJUST = 0.5f;

    private Vector3 startPosition;

    public GameObject mineContainer;
    public GameObject flagSetBlockContainer;
    public GameObject blocksContainer;
    public GameObject emptyBlockContainer;
    public GameObject sectionsContainer;
    private GameObject currentSection;
    private List<GameObject> blocksToShow;
    private int[] numSections;

    private bool handAction;
    private Text minesLeftText;
    private GameStatus gameStatus;
    public MapManager mapManager;

    private Image actionButtonImage;
    private Image resetButtonImage;

    public Sprite hand;
    public Sprite flag;
    public Sprite sunglassFace;
    public Sprite deadFace;

    private bool isDragging;
    private Ray ray;
    private RaycastHit hit;

    private List<string> mines;

    private void Start()
    {
        actionButtonImage = GameObject.Find("ActionImage").GetComponent<Image>();
        resetButtonImage = GameObject.Find("Face").GetComponent<Image>();
        mainCamera = GameObject.Find("CameraController");
        mainCameraGameObject = GameObject.Find("Main Camera");
        mainCameraController = mainCamera.GetComponent<CameraController>();
        pointerStartPosition = transform;
        startPosition = transform.position;
        gameStatus = GameObject.Find("GameStatus").GetComponent<GameStatus>();
        minesLeftText = GameObject.Find("MineLeft").GetComponent<Text>();
        isDragging = false;
        handAction = true;
    }

    public void SwapActions()
    {
        handAction = !handAction;

        if (handAction)
        {
            actionButtonImage.overrideSprite = hand;
        }
        else
        {
            actionButtonImage.overrideSprite = flag;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Get the position of the screen touch
        pointerStartPosition.position = eventData.position;
        ray = Camera.main.ScreenPointToRay(eventData.position);
        if (numSections == null)
        {
            numSections = new int[sectionsContainer.transform.childCount];
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        // Calculate camera movement limits
        mainCamera.transform.position = new Vector3(
            Mathf.Clamp(mainCamera.transform.position.x + (pointerStartPosition.position.x - eventData.position.x) * Time.deltaTime * CAMERA_SPEEDX_ADJUST,
            0, mainCameraController.GetRightBorder()),
            mainCamera.transform.position.y,
            Mathf.Clamp(mainCamera.transform.position.z + (pointerStartPosition.position.y - eventData.position.y) * Time.deltaTime * CAMERA_SPEEDY_ADJUST,
            -(mainCamera.transform.position.y / Mathf.Tan(mainCameraGameObject.transform.rotation.x)) * CAMERA_MOVEMENT_ADJUST,
            mainCameraController.GetTopLeftCorner().z -
            (mainCamera.transform.position.y / Mathf.Tan(mainCameraGameObject.transform.rotation.x)) * CAMERA_MOVEMENT_ADJUST)
            );

        pointerStartPosition.position = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging && !gameStatus.isMineExploded && !gameStatus.isGameFinished &&
            Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Mine")))
        {
            Block block = hit.transform.parent.gameObject.GetComponent<Block>();

            if (gameStatus.IsPaused())
            {
                gameStatus.Restart();
            }
            if (handAction)
            {
                //Debug.Log(hit.transform.gameObject.name);
                //Debug.Log(block.row + " " + block.col);
                if (block.gameObject.name.Contains("Section"))
                {
                    currentSection = block.transform.parent.gameObject;
                    string sectionName = block.transform.parent.gameObject.name;
                    ShowBlockBySection(block, false);
                }
                else
                {
                    blocksToShow = new List<GameObject>();
                    ShowBlock(block, false);
                    foreach (GameObject b in blocksToShow)
                    {
                        b.GetComponent<Block>().ShowBlock();
                        InstantiateLight(b.GetComponent<Block>());
                    }
                }
            }
            else if (!handAction && !block.IsShown())
            {
                SetFlagOrQuestionMark(block);
            }

            if (gameStatus.minesLeft == 0 && gameStatus.numberBlocks == 0)
            {
                gameStatus.isGameFinished = true;
                gameStatus.ShowCongratulationPanel();
                if (gameStatus.mode == GameStatus.Mode.SURVIVAL)
                {
                    GameObject.Find("InputField").SetActive(false);
                    GameObject.Find("AcceptButton").SetActive(false);
                    GameObject.Find("OkButton").SetActive(true);
                }
                resetButtonImage.overrideSprite = sunglassFace;
                Debug.Log("Win!!!");
            }
        }
        transform.position = GameObject.Find("TouchPad Anchor").transform.position;
        isDragging = false;
    }

    private void ShowBlockBySection(Block block, bool isRecursive)
    {
        if (!isRecursive)
        {
            if (block.IsFlagSet())
            {
                block.UnsetFlag();
                gameStatus.minesLeft++;
                UpdateMinesLeftText();
            }
            if (block.IsQuestionMarkSet())
            {
                block.UnsetQuestionMark();
            }

            if (block.name.Contains("Mine") && !block.IsShown())
            {
                block.Explode();
                int count = mineContainer.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    GameObject mine = mineContainer.transform.GetChild(i).gameObject;
                    if ((mine.transform.childCount >= 4 && mine.transform.GetChild(3).name != "Flag") ||
                        mine.transform.childCount < 4)
                    {
                        mine.GetComponent<Block>().ShowBlock();
                    }
                }

                count = flagSetBlockContainer.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    flagSetBlockContainer.transform.GetChild(i).gameObject.GetComponent<Block>().ShowWrongFlag();
                }

                gameStatus.isMineExploded = true;
                gameStatus.isGameFinished = true;
                //Debug.Log("Explode");
                resetButtonImage.overrideSprite = deadFace;
            }
            else if (!block.IsShown())
            {
                block.ShowBlock();
                if (block.gameObject.name.Contains("Section"))
                {
                    // Get the current section of the block
                    int count = currentSection.transform.childCount;
                    Debug.Log("numsections size: " + numSections.Length);
                    // If has any flag set in the current section, show block recursively
                    if (numSections[int.Parse(currentSection.name.Split(' ')[1])] != 0)
                    {
                        //Debug.Log("Flag set block: " + block.gameObject.name);
                        ShowBlock(block.GetComponent<Block>(), true);
                    }
                    else if (block.tag == "0")
                    {
                        GameObject child;
                        for (int i = 0; i < count; i++)
                        {
                            child = currentSection.transform.GetChild(i).gameObject;
                            if (!child.GetComponent<Block>().IsFlagSet() && !child.GetComponent<Block>().IsQuestionMarkSet())
                            {
                                child.GetComponent<Block>().ShowBlock();
                                InstantiateLight(child.GetComponent<Block>());
                            }
                        }
                    }
                }
                InstantiateLight(block);
                gameStatus.numberBlocks--;
            }
        }
        else
        {
            Debug.Log("in else");
            if (!block.IsFlagSet() && !block.IsQuestionMarkSet() && !block.IsShown())
            {
                Debug.Log("in if");
                block.ShowBlock();
                //Debug.Log(block.IsShown());
                gameStatus.numberBlocks--;

                GameObject[] closeBlocks = GetCloseBlocksInSection(currentSection.name, block.row, block.col);
                int count2 = 0;
                int length = closeBlocks.Length;
                bool nextToMine = false;
                GameObject closeBlock;
                for (int i = 0; i < length && !nextToMine; i++)
                {
                    closeBlock = closeBlocks[i];
                    if (closeBlock.tag != "0")
                    {

                    }
                    else
                    {
                        ShowBlock(closeBlock.GetComponent<Block>(), true);
                    }
                }
                foreach (GameObject b in closeBlocks)
                {
                    if (b.tag != "0")
                    {
                        if (0 < length && (
                                !closeBlocks[0].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[0].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (1 < length && (
                                !closeBlocks[1].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[1].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (2 < length && (
                                !closeBlocks[2].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[2].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (3 < length && (
                                !closeBlocks[3].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[3].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (4 < length && (
                                !closeBlocks[4].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[4].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (5 < length && (
                                !closeBlocks[5].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[5].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (6 < length && (
                                !closeBlocks[6].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[6].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;
                        if (7 < length && (
                                !closeBlocks[7].GetComponent<Block>().IsFlagSet() ||
                                !closeBlocks[7].GetComponent<Block>().IsQuestionMarkSet()))
                            count2++;

                        Debug.Log("Count: " + count2);
                        //Debug.Log(b);

                        if (count2 != closeBlocks.Length)
                        {
                            break;
                        }
                        else
                        {
                            ShowBlock(b.GetComponent<Block>(), true);
                        }
                    }
                    else
                    {
                        ShowBlock(b.GetComponent<Block>(), true);
                    }
                }
            }
        }
    }

    private void InstantiateLight(Block block)
    {
        if (mapManager.TimeEfectActivated && mapManager.IsNight && block.gameObject.tag != "0")
        {
            block.InstantiateLight();
        }
    }

    private void ShowBlock(Block block, bool isRecursive)
    {
        if (!isRecursive)
        {
            if (block.IsFlagSet())
            {
                block.UnsetFlag();
                gameStatus.minesLeft++;
                UpdateMinesLeftText();
                ResetParent(block);
            }
            if (block.IsQuestionMarkSet())
            {
                block.UnsetQuestionMark();
            }

            if (block.name.Contains("Mine") && !block.IsShown())
            {
                block.Explode();
                gameStatus.isMineExploded = true;
                gameStatus.isGameFinished = true;
                int count = flagSetBlockContainer.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    flagSetBlockContainer.transform.GetChild(i).gameObject.GetComponent<Block>().ShowWrongFlag();
                }

                //Debug.Log("Explode");
                resetButtonImage.overrideSprite = deadFace;
            }
            //else if (!block.IsShown())
            else if (!block.gameObject.name.Contains("Shown"))
            {
                //block.ShowBlock();
                block.gameObject.name += "-Shown";
                blocksToShow.Add(block.gameObject);
                gameStatus.numberBlocks--;
                //Debug.Log(block.row + " " + block.col);
                if (block.gameObject.tag == "0")
                {
                    foreach (GameObject b in GetCloseBlocks(block.row, block.col))
                    {
                        //Debug.Log(b);
                        if (b != null)
                        {
                            ShowBlock(b.GetComponent<Block>(), true);
                        }
                    }
                }
            }
        }
        else
        {
            if (!block.IsFlagSet() && !block.IsQuestionMarkSet() && !block.gameObject.name.Contains("Shown")/*!block.IsShown()*/)
            {
                //block.ShowBlock();
                block.gameObject.name += "-Shown";
                blocksToShow.Add(block.gameObject);
                gameStatus.numberBlocks--;

                if (block.gameObject.tag == "0")
                {
                    foreach (GameObject b in GetCloseBlocks(block.row, block.col))
                    {
                        //Debug.Log(b);
                        if (b != null)
                        {
                            ShowBlock(b.GetComponent<Block>(), true);
                        }
                    }

                }
            }
        }
    }
    
    private void ResetParent(Block block)
    {
        //if (block.gameObject.name.Contains("Section"))
        //{
        //    int sectionNumber = int.Parse(block.gameObject.name.Split(' ')[1].Split('-')[0]);
        //    block.transform.SetParent(sectionsContainer.transform.GetChild(sectionNumber));
        //}
        //else
        if (block.gameObject.tag != "0")
        {
            block.transform.SetParent(blocksContainer.transform);
        }
        else if (!block.gameObject.name.StartsWith("Mine"))
        {
            block.transform.SetParent(emptyBlockContainer.transform);
        }
    }

    private void AssignParent(Block block)
    {
        Debug.Log("Block name: " + block.gameObject.name);
        Debug.Log("Block tag: " + block.gameObject.tag);
        if (!block.gameObject.name.StartsWith("Mine"))
        {
            block.transform.SetParent(flagSetBlockContainer.transform);
        }
    }

    private GameObject[] GetCloseBlocks(int row, int col)
    {
        GameObject[] blocks = new GameObject[] {    GameObject.Find("Block-" + (row+1) + "-" + col),
                                                    GameObject.Find("Block-" + (row+1) + "-" + (col+1)),
                                                    GameObject.Find("Block-" + row + "-" + (col+1)),
                                                    GameObject.Find("Block-" + (row-1) + "-" + (col+1)),
                                                    GameObject.Find("Block-" + (row-1) + "-" + col),
                                                    GameObject.Find("Block-" + (row-1) + "-" + (col-1)),
                                                    GameObject.Find("Block-" + row + "-" + (col-1)),
                                                    GameObject.Find("Block-" + (row+1) + "-" + (col-1))};
        return blocks;
    }

    private GameObject[] GetCloseBlocksInSection(string sectionName, int row, int col)
    {
        List<GameObject> blocks = new List<GameObject>();
        GameObject closeBlock;

        closeBlock = GameObject.Find("Block-" + (row + 1) + "-" + col + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + (row + 1) + "-" + (col + 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + row + "-" + (col + 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + (row - 1) + "-" + (col + 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + (row - 1) + "-" + col + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + (row - 1) + "-" + (col - 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + row + "-" + (col - 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);
        closeBlock = GameObject.Find("Block-" + (row + 1) + "-" + (col - 1) + "-" + sectionName);
        if (closeBlock != null) blocks.Add(closeBlock);


        return blocks.ToArray();
    }

    private void SetFlagOrQuestionMark(Block block)
    {
        if (!block.IsFlagSet() && !block.IsQuestionMarkSet())
        {
            block.SetFlag();
            AssignParent(block);
            gameStatus.minesLeft--;
            //if (block.gameObject.name.Contains("Section"))
            //{
            //    int sectionNumber = int.Parse(block.gameObject.name.Split('-')[3].Split(' ')[0]);
            //    numSections[sectionNumber]++;
            //}
        }
        else if (block.IsFlagSet())
        {
            block.UnsetFlag();
            ResetParent(block);
            gameStatus.minesLeft++;
            block.SetQuestionMark();
        }
        else if (block.IsQuestionMarkSet())
        {
            block.UnsetQuestionMark();
            //if (block.gameObject.name.Contains("Section"))
            //{
            //    int sectionNumber = int.Parse(block.gameObject.name.Split('-')[3].Split(' ')[0]);
            //    numSections[sectionNumber]--;
            //}
        }
        UpdateMinesLeftText();
    }

    private void UpdateMinesLeftText()
    {
        if (gameStatus.minesLeft < 0)
        {
            minesLeftText.text = "0000";
        }
        else
        {
            minesLeftText.text = new string('0', 4 - gameStatus.minesLeft.ToString().Length) + gameStatus.minesLeft;
        }
    }
}

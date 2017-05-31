using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TouchPad : MonoBehaviour, IPointerDownHandler,
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
    public GameObject environmentContainer;
    private GameObject currentSection;
    private List<GameObject> blocksToShow;
    private int[] numSections;

    private bool firstClick;
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
        firstClick = true;
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

                ShowWrongFlags();

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

    private void AddRigidBody()
    {
        GameObject currentObject;
        int count = environmentContainer.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            currentObject = environmentContainer.transform.GetChild(i).gameObject;
            if (currentObject.transform.childCount == 2)
            {
                currentObject.transform.GetChild(1).gameObject.AddComponent<Rigidbody>();
                currentObject.transform.GetChild(1).gameObject.GetComponent<Rigidbody>().mass = 15;
            }
            else if (currentObject.transform.childCount == 3)
            {
                currentObject.transform.GetChild(1).gameObject.AddComponent<Rigidbody>();
                currentObject.transform.GetChild(1).gameObject.GetComponent<Rigidbody>().mass = 3;
                currentObject.transform.GetChild(2).gameObject.AddComponent<Rigidbody>();
                currentObject.transform.GetChild(2).gameObject.GetComponent<Rigidbody>().mass = 3;
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
                // TODO: corregir errores
                if (firstClick)
                {
                    MoveMine(block);
                    AddBlocksToShow(block);
                }
                else
                {
                    AddRigidBody();
                    block.Explode();
                    gameStatus.isMineExploded = true;
                    gameStatus.isGameFinished = true;
                    ShowAllMines();
                    ShowWrongFlags();

                    //Debug.Log("Explode");
                    resetButtonImage.overrideSprite = deadFace;
                }
            }
            //else if (!block.IsShown())
            else if (!block.gameObject.name.Contains("Shown"))
            {
                //block.ShowBlock();
                AddBlocksToShow(block);
            }
        }
        else
        {
            if (!block.IsFlagSet() && !block.IsQuestionMarkSet() && !block.gameObject.name.Contains("Shown")/*!block.IsShown()*/)
            {
                //block.ShowBlock();
                AddBlocksToShow(block);
            }
        }
        firstClick = false;
    }

    private void AddBlocksToShow(Block block)
    {
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

    private GameObject[] GetCloseMines(int row, int col)
    {
        GameObject[] blocks = new GameObject[] {    GameObject.Find("Mine-" + (row+1) + "-" + col),
                                                    GameObject.Find("Mine-" + (row+1) + "-" + (col+1)),
                                                    GameObject.Find("Mine-" + row + "-" + (col+1)),
                                                    GameObject.Find("Mine-" + (row-1) + "-" + (col+1)),
                                                    GameObject.Find("Mine-" + (row-1) + "-" + col),
                                                    GameObject.Find("Mine-" + (row-1) + "-" + (col-1)),
                                                    GameObject.Find("Mine-" + row + "-" + (col-1)),
                                                    GameObject.Find("Mine-" + (row+1) + "-" + (col-1))};
        return blocks;
    }

    private void ShowAllMines()
    {
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
    }

    private void ShowWrongFlags()
    {
        int count = flagSetBlockContainer.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            flagSetBlockContainer.transform.GetChild(i).GetComponent<Block>().ShowWrongFlag();
            if (!mapManager.shadowActivated)
            {
                flagSetBlockContainer.transform.GetChild(i).FindChild("Wrong Flag").GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                flagSetBlockContainer.transform.GetChild(i).FindChild("Wrong Flag").GetComponent<MeshRenderer>().receiveShadows = false;
            }
        }
    }

    private void MoveMine(Block block)
    {
        int currentMineRow = block.row;
        int currentMineCol = block.col;
        int destinationMineRow = -1;
        int destinationMineCol = -1;

        GameObject[] neighbourBlocks = GetCloseBlocks(currentMineRow, currentMineCol);
        Debug.Log("Bloque: " + neighbourBlocks.Length);
        for (int i = 0; i < 8; i++)
        {
            if (neighbourBlocks[i] != null)
            {
                ModifyBlocks(neighbourBlocks[i], false);
            }
        }

        GameObject destinationBlock = blocksContainer.transform.GetChild(0).gameObject;
        if (destinationBlock != null)
        {

            destinationBlock.gameObject.name = destinationBlock.name.Replace("Block", "Mine");
            destinationBlock.transform.SetParent(mineContainer.transform);
            destinationBlock.gameObject.tag = "0";
            destinationBlock.GetComponent<Block>().SetBomb();

            block.gameObject.name = block.gameObject.name.Replace("Mine", "Block");
            block.gameObject.tag = "0";
            block.gameObject.transform.SetParent(blocksContainer.transform);
            Destroy(block.gameObject.transform.FindChild("Landmine").gameObject);

            destinationMineRow = destinationBlock.GetComponent<Block>().row;
            destinationMineCol = destinationBlock.GetComponent<Block>().col;
            neighbourBlocks = GetCloseBlocks(destinationMineRow, destinationMineCol);

            for (int i = 0; i < 8; i++)
            {
                if (neighbourBlocks[i] != null)
                {
                    ModifyBlocks(neighbourBlocks[i], true);
                }
            }

            neighbourBlocks = GetCloseMines(currentMineRow, currentMineCol);
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if (neighbourBlocks[i] != null)
                {
                    count++;
                }
            }
            try
            {
                int childCount = block.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (block.transform.GetChild(i).name.StartsWith("Number"))
                        Destroy(block.transform.GetChild(i).gameObject);
                }

                childCount = destinationBlock.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (destinationBlock.transform.GetChild(i).name.StartsWith("Number"))
                        Destroy(destinationBlock.transform.GetChild(i).gameObject);
                }
            }
            catch (Exception e) { }
            block.gameObject.tag = count.ToString();
            block.InstantiateNumber(count);
        }
    }

    private void ModifyBlocks(GameObject neighbourBlock, bool isSum)
    {
        int number = int.Parse(neighbourBlock.tag);
        try
        {
            Destroy(neighbourBlock.transform.FindChild(("Number" + number)).gameObject);
        }
        catch(Exception e) { }

        if (isSum)
            number++;
        else
            number--;

        neighbourBlock.tag = number.ToString();
        if (number > 0)
            neighbourBlock.GetComponent<Block>().InstantiateNumber(number);
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
            if (!mapManager.shadowActivated)
            {
                block.transform.FindChild("Flag").GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                block.transform.FindChild("Flag").GetComponent<MeshRenderer>().receiveShadows = false;
            }
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
            if (!mapManager.shadowActivated)
            {
                block.transform.FindChild("QuestionMark").GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                block.transform.FindChild("QuestionMark").GetComponent<MeshRenderer>().receiveShadows = false;
            }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MapManager : MonoBehaviour
{
    private const int CENTER = 0, UP = 1, UP_RIGHT = 2, RIGHT = 3, DOWN_RIGHT = 4,
        DOWN = 5, DOWN_LEFT = 6, LEFT = 7, UP_LEFT = 8;

    public GameObject block;
    public GameObject cube;
    public GameObject[,] blocks;
    public GameObject[] decoration;

    private int rows, cols, bombs;
    public bool TimeEfectActivated { get; set; }
    public bool IsNight { get; set; }
    private bool environmentActivated;
    public bool shadowActivated;

    private GameObject cameraController;
    private GameObject sun;
    private AudioSource soundEfects;

    public GameObject mineContainer;
    public GameObject blocksContainer;
    public GameObject emptyBlockContainer;
    public GameObject sectionsContainer;
    public GameObject environmentContainer;

    public GameObject sea;
    public Material nightSky;
    public Material daySky;
    public Material sunsetSky;

    public Material randomMaterial;
    private Material randomCoverMaterial;
    private Material randomBaseMaterial;
    private Color32 coverColor;
    private Color32 baseColor;

    private void Start()
    {
        cameraController = GameObject.Find("CameraController");
        sun = GameObject.Find("Sun");
        TimeEfectActivated = PlayerPrefs.GetInt("light") == 1;
        IsNight = false;
        environmentActivated = PlayerPrefs.GetInt("environment") == 1;
        shadowActivated = PlayerPrefs.GetInt("shadow") == 1;
        soundEfects = cameraController.transform.FindChild("Main Camera").
            gameObject.transform.FindChild("AmbientSound").GetComponent<AudioSource>();

        rows = PlayerPrefs.GetInt("rows");
        cols = PlayerPrefs.GetInt("columns");
        bombs = PlayerPrefs.GetInt("bombs");

        blocks = new GameObject[rows, cols];

        LoadMaterialColors();
        LoadTimeEfect();
        LoadMap();
    }

    private void LoadMaterialColors()
    {
        byte green = (byte)Random.Range(25, 35);
        byte red = (byte)Random.Range(25, 45);
        coverColor = new Color32(
            red,
            (byte)Random.Range(80, 95),
            (byte)(red - 5),
            255
            );

        baseColor = new Color32(
            (byte)Random.Range(60, 80),
            green,
            (byte)(green - 20),
            255
            );
            
        randomCoverMaterial = new Material(randomMaterial);
        randomBaseMaterial = new Material(randomMaterial);

        randomCoverMaterial.color = coverColor;
        randomBaseMaterial.color = baseColor;

        cube.GetComponent<MeshRenderer>().material = randomBaseMaterial;
        foreach (GameObject deco in decoration)
        {
            AssignRandomMaterialColor(deco);
        }
    }

    private void LoadTimeEfect()
    {
        Debug.Log(System.DateTime.Now.Hour);
        if (TimeEfectActivated)
        {
            if (System.DateTime.Now.Hour >= 6 && System.DateTime.Now.Hour < 9)
            {
                sun.transform.rotation = Quaternion.Euler(30, -40, 0);
                sun.GetComponent<Light>().intensity = 0.6f;
                sun.GetComponent<Light>().shadowStrength = 0.65f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 9 && System.DateTime.Now.Hour < 12)
            {
                sun.transform.rotation = Quaternion.Euler(60, -60, 0);
                sun.GetComponent<Light>().intensity = 0.8f;
                sun.GetComponent<Light>().shadowStrength = 0.55f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 12 && System.DateTime.Now.Hour < 16)
            {
                sun.transform.rotation = Quaternion.Euler(115, -180, -75);
                sun.GetComponent<Light>().intensity = 0.9f;
                sun.GetComponent<Light>().shadowStrength = 0.4f;
                RenderSettings.skybox = daySky;
            }
            else if (System.DateTime.Now.Hour >= 16 && System.DateTime.Now.Hour < 18)
            {
                sun.transform.rotation = Quaternion.Euler(120, -230, 0);
                sun.GetComponent<Light>().intensity = 0.8f;
                sun.GetComponent<Light>().shadowStrength = 0.55f;
                RenderSettings.skybox = sunsetSky;
            }
            else if (System.DateTime.Now.Hour >= 18 && System.DateTime.Now.Hour < 21)
            {
                sun.transform.rotation = Quaternion.Euler(150, -120, 0);
                sun.GetComponent<Light>().intensity = 0.6f;
                sun.GetComponent<Light>().shadowStrength = 0.65f;
                IsNight = true;
                RenderSettings.skybox = nightSky;
            }
            else
            {
                sun.transform.rotation = Quaternion.Euler(125, -160, -30);
                sun.GetComponent<Light>().color = new Color32(90, 90, 90, 255);
                sun.GetComponent<Light>().intensity = 0.4f;
                sun.GetComponent<Light>().shadowStrength = 0.75f;
                soundEfects = cameraController.transform.FindChild("Main Camera").
                    gameObject.transform.FindChild("NightSound").GetComponent<AudioSource>();
                IsNight = true;
                RenderSettings.skybox = nightSky;
            }
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(65, -130, 0);
            sun.GetComponent<Light>().intensity = 1f;
            sun.GetComponent<Light>().shadowStrength = 0.75f;
        }
        Debug.Log(sun.GetComponent<Light>().color);
        Debug.Log(sun.GetComponent<Light>().intensity);
    }
    
    public void LoadMap()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                blocks[row, col] = CreateObject(block, new Vector3(col, 0, row), Quaternion.identity);
                blocks[row, col].GetComponent<Block>().row = row;
                blocks[row, col].GetComponent<Block>().col = col;
                blocks[row, col].name = "Block-" + row + "-" + col;
                //Debug.Log("Block: " + row + " " + col);
                
                AssignRandomMaterialColor(blocks[row, col]);
            }
        }

        // Create position of bombs
        int rowBomb;
        int colBomb;
        List<Position> bombsPosition = new List<Position>();

        do
        {
            rowBomb = Random.Range(0, rows - 1);
            colBomb = Random.Range(0, cols - 1);

            if (!blocks[rowBomb, colBomb].GetComponent<Block>().IsBomb())
            {
                blocks[rowBomb, colBomb].GetComponent<Block>().SetBomb();
                blocks[rowBomb, colBomb].name = "Mine-" + rowBomb + "-" + colBomb;
                bombsPosition.Add(new Position(rowBomb, colBomb));
                blocks[rowBomb, colBomb].transform.SetParent(mineContainer.transform);
                //Debug.Log("bombs: " + bombsPosition.Count + "Position: " + rowBomb + " " + colBomb);
                //Debug.Log("Comprobacion: " + blocks[rowBomb, colBomb].GetComponent<Block>().IsBomb());
            }
        } while (bombsPosition.Count < bombs);

        // Set numbers to blocks
        int movement;
        Position positionToCheck;
        foreach (Position p in bombsPosition)
        {
            //Debug.Log("position bomb : " + p.row + " " + p.col);
            movement = CENTER;
            do
            {
                positionToCheck = Move(movement, p);
                if (IsReachable(positionToCheck) && !blocks[positionToCheck.row, positionToCheck.col].GetComponent<Block>().IsBomb())
                {
                    //Debug.Log(positionToCheck.row + " " + positionToCheck.col);
                    blocks[positionToCheck.row, positionToCheck.col].name = "Block-" + positionToCheck.row + "-" + positionToCheck.col;
                    blocks[positionToCheck.row, positionToCheck.col].GetComponent<Block>().SetNumber();
                    blocks[positionToCheck.row, positionToCheck.col].GetComponent<Block>().IncrementNumber();
                    blocks[positionToCheck.row, positionToCheck.col].transform.SetParent(blocksContainer.transform);
                }
                movement++;
            } while (movement <= UP_LEFT);
        }


        foreach (GameObject go in blocks)
        {
            Block b = go.GetComponent<Block>();
            //Debug.Log("bloques: " + b.name);
            if (b.IsNumber())
            {
                b.InstantiateNumber();
            }
            go.tag = "" + b.number;
        }

        GameObject[] emptyBlocks = GameObject.FindGameObjectsWithTag("0");
        foreach (GameObject emptyBlock in emptyBlocks)
        {
            if (!emptyBlock.name.Contains("Mine"))
            {
                emptyBlock.transform.SetParent(emptyBlockContainer.transform);
            }
        }
        
        //GameObject section = null;
        // Assign sections to empty blocks and number blocks next to them.
        //while (emptyBlockContainer.transform.childCount > 0)
        //{
        //    Debug.Log("Childs: " + emptyBlockContainer.transform.childCount);
        //    section = new GameObject();
        //    section.name = "Section " + sectionsContainer.transform.childCount;
        //    AssignSection(
        //        section, 
        //        emptyBlockContainer.transform.GetChild(0).gameObject
        //        );
        //    section.transform.SetParent(sectionsContainer.transform);
        //    Debug.Log(section.name);
        //}

        // Move camera to the middle of the map
        cameraController.transform.position = new Vector3(
            blocks[0, cols / 2].transform.position.x,
            cameraController.transform.position.y,
            cameraController.transform.position.z
            );
        cameraController.GetComponent<CameraController>().SetTopLeftMapCorner(blocks[rows - 1, 0].transform.position);
        cameraController.GetComponent<CameraController>().SetBottomRightCorner(blocks[0, cols - 1].transform.position);

        if (environmentActivated)
        {
            GenerateEnvironment();
            
            int totalRows = rows + 1;
            int totalCols = cols + 1;

            float seaWidthScale = totalRows + 26f;
            float seaLengthScale = totalCols + 18f;
            float seaHeightPos = 0.5f;
            float scale = seaWidthScale > seaLengthScale ? seaWidthScale : seaLengthScale;

            GameObject seaObject = CreateObject(sea, 
                new Vector3(
                    blocks[0, cols / 2].transform.position.x, 
                    seaHeightPos, 
                    blocks[rows / 2, cols / 2].transform.position.z),
                Quaternion.Euler(0, 90, 0));
            seaObject.transform.localScale = new Vector3(scale, 1, scale);
        }

        if (!shadowActivated)
        {
            RemoveShadowCaster(mineContainer);
            RemoveShadowCaster(blocksContainer);
            RemoveShadowCaster(emptyBlockContainer);
            RemoveEnvironmentShadowCaster(environmentContainer);
        }

        soundEfects.loop = true;
        soundEfects.Play();
    }

    private void RemoveShadowCaster(GameObject container)
    {
        GameObject child;
        GameObject child2;
        int count = container.transform.childCount;
        int count2;
        for (int i = 0; i < count; i++)
        {
            child = container.transform.GetChild(i).gameObject;
            count2 = child.transform.childCount;
            for (int j = 0; j < count2; j++)
            {
                child2 = child.transform.GetChild(j).gameObject;
                child2.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                child2.GetComponent<MeshRenderer>().receiveShadows = false;
            }
        }
    }

    private void RemoveEnvironmentShadowCaster(GameObject container)
    {
        GameObject child;
        GameObject child2;
        GameObject child3;
        int count = container.transform.childCount;
        int count2, count3;
        for (int i = 0; i < count; i++)
        {
            child = container.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Cube"))
            {
                child.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                child.GetComponent<MeshRenderer>().receiveShadows = false;
            }
            else
            {
                count2 = child.transform.childCount;
                for (int j = 0; j < count2; j++)
                {
                    child2 = child.transform.GetChild(j).gameObject;
                    child2.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                    child2.GetComponent<MeshRenderer>().receiveShadows = false;
                }

                switch (child.name.Replace("(Clone)", ""))
                {
                    case "Tree1":
                    case "Tree2":
                    case "Pine":
                        child3 = child.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;
                        child3.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                        child3.GetComponent<MeshRenderer>().receiveShadows = false;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void GenerateEnvironment()
    {
        List<int> leftSide = new List<int>();
        int randomNumber = 0;
        for (int row = 0; row < rows + 1; row++)
        {
            if (row == 0)
            {
                randomNumber = Random.Range(1, 4);
                leftSide.Add(randomNumber);
                CreateObject(decoration[randomNumber], new Vector3(-1, 0, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
            }
            else
            {
                if (leftSide[row - 1] > 0)
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;

                    leftSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(-1, randomNumber == 0 ? 0 : 0.5f, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else if (leftSide.Count >= 2 && leftSide[row - 2] < 1)
                {
                    randomNumber = Random.Range(1, 4);
                    leftSide.Add(randomNumber);
                    CreateObject(decoration[randomNumber], new Vector3(-1, 0, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    leftSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(-1, randomNumber == 0 ? 0 : 0.5f, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
            }
        }

        List<int> topSide = new List<int>();
        for (int col = 0; col < cols; col++)
        {
            if (col == 0)
            {
                if (leftSide[rows] < 1 && leftSide[rows - 1] < 1)
                {
                    randomNumber = Random.Range(1, 4);
                    topSide.Add(randomNumber);
                    CreateObject(decoration[randomNumber], new Vector3(col, 0, rows), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    topSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(col, randomNumber == 0 ? 0 : 0.5f, rows), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
            }
            else
            {
                if (topSide[col - 1] > 0)
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    topSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(col, randomNumber == 0 ? 0 : 0.5f, rows), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else if (topSide.Count >= 2 && topSide[col - 2] < 1)
                {
                    randomNumber = Random.Range(1, 4);
                    topSide.Add(randomNumber);
                    CreateObject(decoration[randomNumber], new Vector3(col, 0, rows), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    topSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(col, randomNumber == 0 ? 0 : 0.5f, rows), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
            }
        }

        List<int> rightSide = new List<int>();
        for (int row = rows; row >= 0; row--)
        {
            if (row == rows)
            {
                if (topSide[cols - 1] < 1 && topSide[rows - 2] < 1)
                {
                    randomNumber = Random.Range(1, 4);
                    rightSide.Add(randomNumber);
                    CreateObject(decoration[randomNumber], new Vector3(cols, 0, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    rightSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(cols, randomNumber == 0 ? 0 : 0.5f, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
            }
            else
            {

                if (rightSide[rows - row - 1] > 0)
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    rightSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(cols, randomNumber == 0 ? 0 : 0.5f, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }

                else if (rightSide.Count >= 2 && rightSide[rows - row - 1] < 1)
                {
                    randomNumber = Random.Range(1, 4);
                    rightSide.Add(randomNumber);
                    CreateObject(decoration[randomNumber], new Vector3(cols, 0, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }
                else
                {
                    randomNumber = Random.Range(0, 2) >= 1 ? 0 : -1;
                    rightSide.Add(randomNumber);
                    CreateObject(randomNumber == 0 ? decoration[0] : cube, new Vector3(cols, randomNumber == 0 ? 0 : 0.5f, row), Quaternion.identity).transform.SetParent(environmentContainer.transform);
                }

                //CreateObject(cube, new Vector3(cols, 0.5f, row), Quaternion.identity);
            }
        }
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
        
        randomCoverMaterial = new Material(randomMaterial);
        randomCoverMaterial.color = coverColor;

        GameObject cover;
        if (currentBlock.transform.FindChild("Cover") != null)
        {
            cover = currentBlock.transform.FindChild("Cover").gameObject;
            cover.GetComponent<MeshRenderer>().material = randomCoverMaterial;
        }

        GameObject baseBlock = currentBlock.transform.FindChild("Base").gameObject;
        baseBlock.GetComponent<MeshRenderer>().material = randomBaseMaterial;
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

    private void AssignSection(GameObject section, GameObject block)
    {
        if (block == null) return;
        if (block.name.StartsWith("Mine")) return;
        if (block.transform.parent.name.StartsWith("Section")) return;

        block.transform.SetParent(section.transform);
        block.name += ("-" + section.name);
        if (block.tag != "0") return;

        GameObject[] closerBlocks = GetCloseBlocks(block.GetComponent<Block>().row, block.GetComponent<Block>().col);

        for (int i = 0; i < closerBlocks.Length; i++)
            AssignSection(section, closerBlocks[i]);
    }

    private GameObject CreateObject(GameObject gameObject, Vector3 v3, Quaternion q)
    {
        GameObject go = Instantiate(gameObject, v3, q) as GameObject;
        return go;
    }

    private Position Move(int movement, Position position)
    {
        Position p = new Position(position.row, position.col);
        switch (movement)
        {
            case UP:
                p.row += 1;
                break;
            case UP_RIGHT:
                p.row += 1;
                p.col += 1;
                break;
            case RIGHT:
                p.col += 1;
                break;
            case DOWN_RIGHT:
                p.col += 1;
                p.row -= 1;
                break;
            case DOWN:
                p.row -= 1;
                break;
            case DOWN_LEFT:
                p.row -= 1;
                p.col -= 1;
                break;
            case LEFT:
                p.col -= 1;
                break;
            case UP_LEFT:
                p.col -= 1;
                p.row += 1;
                break;
        }
        return p;
    }

    private bool IsReachable(Position position)
    {
        return position.row >= 0 && position.row < rows &&
            position.col >= 0 && position.col < cols;
    }

}

public class Position
{
    public int col;
    public int row;

    public Position(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStatus : MonoBehaviour {

    public enum Mode { CLASSIC, SURVIVAL };
    public enum Difficulty { BEGINNER, INTERMEDIATE, EXPERT, CUSTOM };

    public int minesLeft;
    public int totalMines;
    public int numberBlocks;
    public int totalBlocks;
    public float currentTime;
    public Mode mode;
    public Difficulty difficulty;

    public int numClicks;
    public float score;

    public bool isGamePaused;
    public bool isGameFinished;
    public bool isMineExploded;
    
    public Text minesLeftText;
    public Text currentTimeText;
    public Text congratulationText;
    public Text adviseText;
    public GameObject questionTextObject;
    public GameObject exitOkButton;
    public GameObject exitNoButton;

    public GameObject acceptButton;
    public GameObject okButton;
    public GameObject nickInput;

    private DataService dataService;
    private IEnumerable<BeginnerScore> beginnerScores;
    private IEnumerable<IntermediateScore> intermediateScores;
    private IEnumerable<ExpertScore> expertScores;
    private IEnumerable<CustomScore> customScores;

    private Color32 FIRSTPLACE = new Color32(247, 219, 67, 255);
    private Color32 SECONDPLACE = new Color32(188, 188, 188, 255);
    private Color32 THIRDPLACE = new Color32(236, 161, 93, 255);
    public GameObject gameOverPanel;
    public GameObject congratulationPanel;
    public GameObject medal;
    public Image resetButtonImage;
    public Sprite deadFace;
    private string nickName;
    private int currentRank;

    public GameObject uploadButton;
    public GameObject progressPanel;
    public GameObject progressCircle;
    public GameObject rankPanel;
    public Text rankText;
    private int angle;

    private const string DEFAULT_CONGRAT_TEXT = "You have finished the level";

    private void Start()
    {
        mode = PlayerPrefs.GetInt("mode") == 0 ? Mode.CLASSIC : Mode.SURVIVAL;
        difficulty = PlayerPrefs.GetString("difficulty") == "beginner" ? Difficulty.BEGINNER :
            PlayerPrefs.GetString("difficulty") == "intermediate" ? Difficulty.INTERMEDIATE :
            PlayerPrefs.GetString("difficulty") == "expert" ? Difficulty.EXPERT : Difficulty.CUSTOM;
        currentTime = mode == Mode.SURVIVAL ? PlayerPrefs.GetInt("time") : 0f;
        currentTimeText.text = new string('0', 4 - ((int)currentTime).ToString().Length) + (int)currentTime;
        totalBlocks = PlayerPrefs.GetInt("rows") * PlayerPrefs.GetInt("columns");
        minesLeft = PlayerPrefs.GetInt("bombs");
        numberBlocks = totalBlocks - minesLeft;
        minesLeftText.text = new string('0', 4 - minesLeft.ToString().Length) + minesLeft;
        isGamePaused = true;
        isMineExploded = false;
        isGameFinished = false;

        dataService = new DataService("scores.db");
        dataService.CreateDB();
        currentRank = 0;
        angle = 0;
    }

    private void FixedUpdate()
    {
        if (!isGamePaused && !isMineExploded && !isGameFinished)
        {
            if (mode == Mode.SURVIVAL)
            {
                StartCoroutine(CountDown());
            }
            else if (mode == Mode.CLASSIC)
            {
                StartCoroutine(Timing());
            }
        }
        if (isMineExploded)
        {
            StartCoroutine(ShowGameOverPanel());
        }
        if (progressPanel.activeInHierarchy)
        {
            StartCoroutine(LoadProgress());
        }
    }

    IEnumerator LoadProgress()
    {
        if (angle <= -360)
        {
            angle = 0;
        }

        progressCircle.transform.rotation = Quaternion.Euler(50, 0, angle);
        Debug.Log(progressCircle.transform.rotation);

        angle -= 10;
        yield return null;
    }

    public void SetRankText(string text)
    {
        progressPanel.SetActive(false);
        rankPanel.SetActive(true);
        rankText.text = text;
    }

    public void ShowCongratulationPanel()
    {
        congratulationPanel.SetActive(true);
        congratulationText.text = DEFAULT_CONGRAT_TEXT;
        if (mode == Mode.CLASSIC)
        {
            switch (difficulty)
            {
                case Difficulty.BEGINNER:
                    CalculateBeginnerRank();
                    congratulationText.text += " in " + currentTime.ToString("0.00") + " seconds";
                    break;
                case Difficulty.INTERMEDIATE:
                    CalculateIntermediateRank();
                    congratulationText.text += " in " + currentTime.ToString("0.00") + " seconds";
                    break;
                case Difficulty.EXPERT:
                    CalculateExpertRank();
                    congratulationText.text += " in " + currentTime.ToString("0.00") + " seconds";
                    break;
                case Difficulty.CUSTOM:
                    score = PlayerPrefs.GetInt("rows") * PlayerPrefs.GetInt("columns") * 2.5f +
                        PlayerPrefs.GetInt("bombs") * 5f - currentTime * 2;
                    CalculateCustomRank();
                    congratulationText.text += " with " + score.ToString("0.00") + " points";
                    break;
            }

            Debug.Log("Rank " + currentRank);
            switch (currentRank)
            {
                case 1:
                    medal.GetComponent<Image>().color = FIRSTPLACE;
                    medal.transform.GetChild(0).GetComponent<Text>().text = "1";
                    medal.SetActive(true);
                    break;
                case 2:
                    medal.GetComponent<Image>().color = SECONDPLACE;
                    medal.transform.GetChild(0).GetComponent<Text>().text = "2";
                    medal.SetActive(true);
                    break;
                case 3:
                    medal.GetComponent<Image>().color = THIRDPLACE;
                    medal.transform.GetChild(0).GetComponent<Text>().text = "3";
                    medal.SetActive(true);
                    break;
            }
        }
        else
        {
            acceptButton.SetActive(false);
            nickInput.SetActive(false);
            okButton.SetActive(true);
        }
    }

    private void CalculateBeginnerRank()
    {
        int rank = 0;
        bool rankFound = false;
        if (dataService.GetBeginnerScoreCount() == 0)
        {
            currentRank = 1;
        }
        else
        {
            int count = 0;
            beginnerScores = dataService.GetBeginnerScores();
            foreach (BeginnerScore tempScore in beginnerScores)
            {
                Debug.Log("beginner " + rank + ": " + tempScore.Time);
                rank++;
                if (currentTime < tempScore.Time && !rankFound)
                {
                    rankFound = true;
                    currentRank = rank;
                }
                count++;
            }

            if (!rankFound && count < 3)
            {
                currentRank = count + 1;
            }
        }

        if (currentRank == 1)
        {
            PlayerPrefs.SetFloat("bestBeginnerScore", currentTime);
            uploadButton.SetActive(true);
        }
    }

    private void CalculateIntermediateRank()
    {
        int rank = 0;
        bool rankFound = false;
        if (dataService.GetIntermediateScoreCount() == 0)
        {
            currentRank = 1;
        }
        else
        {
            int count = 0;
            intermediateScores = dataService.GetIntermediateScores();
            foreach (IntermediateScore tempScore in intermediateScores)
            {
                rank++;
                if (currentTime < tempScore.Time && !rankFound)
                {
                    rankFound = true;
                    currentRank = rank;
                }
                count++;
            }

            if (!rankFound && count < 3)
            {
                currentRank = count + 1;
            }
        }

        if (currentRank == 1)
        {
            PlayerPrefs.SetFloat("bestIntermediateScore", currentTime);
            uploadButton.SetActive(true);
        }
    }

    private void CalculateExpertRank()
    {
        int rank = 0;
        bool rankFound = false;
        if (dataService.GetExpertScoreCount() == 0)
        {
            currentRank = 1;
        }
        else
        {
            int count = 0;
            expertScores = dataService.GetExpertScores();
            foreach (ExpertScore tempScore in expertScores)
            {
                rank++;
                if (currentTime < tempScore.Time && !rankFound)
                {
                    rankFound = true;
                    currentRank = rank;
                }
                count++;
            }

            if (!rankFound && count < 3)
            {
                currentRank = count + 1;
            }
        }

        if (currentRank == 1)
        {
            PlayerPrefs.SetFloat("bestExpertScore", currentTime);
            uploadButton.SetActive(true);
        }
    }

    private void CalculateCustomRank()
    {
        int rank = 0;
        bool rankFound = false;
        if (dataService.GetCustomScoreCount() == 0)
        {
            currentRank = 1;
        }
        else
        {
            int count = 0;
            customScores = dataService.GetCustomScores();
            foreach (CustomScore tempScore in customScores)
            {
                rank++;
                if (score > tempScore.Score && !rankFound)
                {
                    rankFound = true;
                    currentRank = rank;
                }
                count++;
            }

            if (!rankFound && count < 3)
            {
                currentRank = count + 1;
            }
        }
    }

    public void InsertScore()
    {
        switch(difficulty)
        {
            case Difficulty.BEGINNER:
                InsertBeginnerScore();
                break;
            case Difficulty.INTERMEDIATE:
                InsertIntermediateScore();
                break;
            case Difficulty.EXPERT:
                InsertExpertScore();
                break;
            case Difficulty.CUSTOM:
                InsertCustomScore();
                break;
        }
    }

    private void InsertCustomScore()
    {
        nickName = GameObject.Find("InputField").GetComponent<InputField>().text;
        nickName = nickName.Trim() == "" ? "Player" : nickName;
        if (dataService.GetCustomScoreCount() < 10)
        {
            dataService.CreateCustomScore(
                nickName,
                PlayerPrefs.GetInt("rows") + "x" + PlayerPrefs.GetInt("columns"),
                PlayerPrefs.GetInt("bombs"),
                currentTime,
                score
                );
        }
        else
        {
            CustomScore tempScore = dataService.GetLowestCustomScore();
            if (tempScore.Score < score)
            {
                tempScore.Nick = nickName;
                tempScore.Size = PlayerPrefs.GetInt("rows") + "x" + PlayerPrefs.GetInt("columns");
                tempScore.Mines = PlayerPrefs.GetInt("bombs");
                tempScore.Time = currentTime;
                tempScore.Score = score;
                dataService.UpdateCustomScore(tempScore);
            }
        }
    }

    private void InsertExpertScore()
    {
        nickName = GameObject.Find("InputField").GetComponent<InputField>().text;
        nickName = nickName.Trim() == "" ? "Player" : nickName;
        if (dataService.GetExpertScoreCount() < 10)
        {
            dataService.CreateExpertScore(nickName, currentTime);
        }
        else
        {
            ExpertScore tempScore = dataService.GetLowestExpertScore();
            if (tempScore.Time > currentTime)
            {
                tempScore.Nick = nickName;
                tempScore.Time = currentTime;
                dataService.UpdateExpertScore(tempScore);
            }
        }
    }

    private void InsertIntermediateScore()
    {
        nickName = GameObject.Find("InputField").GetComponent<InputField>().text;
        nickName = nickName.Trim() == "" ? "Player" : nickName;
        if (dataService.GetIntermediateScoreCount() < 10)
        {
            dataService.CreateIntermediateScore(nickName, currentTime);
        }
        else
        {
            IntermediateScore tempScore = dataService.GetLowestIntermediateScore();
            if (tempScore.Time > currentTime)
            {
                tempScore.Nick = nickName;
                tempScore.Time = currentTime;
                dataService.UpdateIntermediateScore(tempScore);
            }
        }
    }

    private void InsertBeginnerScore()
    {
        nickName = GameObject.Find("InputField").GetComponent<InputField>().text;
        nickName = nickName.Trim() == "" ? "Player" : nickName;
        if (dataService.GetBeginnerScoreCount() < 10)
        {
            dataService.CreateBeginnerScore(nickName, currentTime);
        }
        else
        {
            BeginnerScore tempScore = dataService.GetLowestBeginnerScore();
            if (tempScore.Time > currentTime)
            {
                tempScore.Nick = nickName;
                tempScore.Time = currentTime;
                dataService.UpdateBeginnerScore(tempScore);
            }
        }
    }

    IEnumerator Timing()
    {
        currentTime += Time.deltaTime;
        currentTimeText.text = new string('0', 4 - ((int)currentTime).ToString().Length) + (int)currentTime;
        yield return new WaitForSeconds(1);
    }

    IEnumerator CountDown()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            if (minesLeft != 0 && numberBlocks != 0)
            {
                resetButtonImage.overrideSprite = deadFace;
                gameOverPanel.SetActive(true);
            }
            isGameFinished = true;
        }
        currentTimeText.text = new string('0', 4 - ((int)currentTime).ToString().Length) + (int)currentTime;
        yield return new WaitForSeconds(1);
    }

    IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSeconds(1);
        gameOverPanel.SetActive(true);
        isMineExploded = false;
    }

    public void Pause()
    {
        isGamePaused = true;
    }

    public void SetAdviseText()
    {
        if (isGameFinished)
        {
            adviseText.gameObject.SetActive(false);
        }
        else
        {
            adviseText.text = "You will lose all the progress.";
        }
    }

    public void Restart()
    {
        isGamePaused = false;
    }

    public bool IsPaused()
    {
        return isGamePaused;
    }

    public bool IsMineExploded()
    {
        return isMineExploded;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene("LoadScreen");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("StartScene 1");
    }
}

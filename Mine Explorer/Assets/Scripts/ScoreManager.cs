using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    private DataService dataService;

    public GameObject scoresPanel;
    public GameObject customScoresPanel;

    private string currentScoreTable;
    public bool isLocalScore;

    private const string NICK = "Anonymous";
    private const string TIME = "0000";

    private List<ScoresModel> scores;
    public WebServiceController webServiceController;

    public GameObject customButton;
    public GameObject progressCircle;
    public GameObject progressPanel;
    public GameObject errorPanel;
    public Text errorText;
    private int angle;

    public GameObject beginnerButton;
    public GameObject rankTextPanel;
    private Color32 highlightedBgColor;
    private Color32 highlightedTextColor;
    private Color32 backgroundColor;
    private Color32 textColor;
    private bool customScoresShown;

    // Use this for initialization
    void Start () {
        backgroundColor = new Color32(43, 43, 43, 255);
        textColor = new Color32(171, 171, 171, 255);
        backgroundColor = new Color32(43, 43, 43, 255);
        textColor = new Color32(171, 171, 171, 255);
        highlightedBgColor = new Color32(107, 107, 107, 255);
        highlightedTextColor = Color.white;
        isLocalScore = true;
        dataService = new DataService("scores.db");
        dataService.CreateDB();
        ShowNormalScores("beginner_score");
        angle = 0;
        customScoresShown = false;
	}

    private void Update()
    {
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

        progressCircle.transform.rotation = Quaternion.Euler(0, 0, angle);
        //Debug.Log(progressCircle.transform.rotation);

        angle -= 10;
        yield return null;
    }

    public void SetErrorText(string text)
    {
        progressPanel.SetActive(false);
        errorPanel.SetActive(true);
        errorText.text = text;
    }
    
    public void ChangeRankMode()
    {
        isLocalScore = !isLocalScore;

        if (isLocalScore)
            customButton.SetActive(true);
        else
            customButton.SetActive(false);

        if (customScoresShown)
        {
            customButton.GetComponent<Image>().color = backgroundColor;
            customButton.transform.GetChild(0).GetComponent<Text>().color = textColor;
            beginnerButton.GetComponent<Image>().color = highlightedBgColor;
            beginnerButton.transform.GetChild(0).GetComponent<Text>().color = highlightedTextColor;
            customScoresPanel.SetActive(false);
            rankTextPanel.SetActive(true);
            ShowNormalScores("beginner_score");
        }
        else
            ShowNormalScores(currentScoreTable);
        Debug.Log("Local score: " + isLocalScore);
    }

    public void SetScore(List<ScoresModel> scores, string type)
    {
        this.scores = scores;
        int size = scores.Count;
        for (int i = 0; i < size; i++)
        {
            scoresPanel.transform.GetChild(i).transform.GetChild(1).GetComponent<Text>().text = scores[i].Nick;
            switch (type)
            {
                case "beginner_score":
                    scoresPanel.transform.GetChild(i).transform.GetChild(2).GetComponent<Text>().text = scores[i].BeginnerScore.ToString("0.00");
                    break;
                case "intermediate_score":
                    scoresPanel.transform.GetChild(i).transform.GetChild(2).GetComponent<Text>().text = scores[i].IntermediateScore.ToString("0.00");
                    break;
                case "expert_score":
                    scoresPanel.transform.GetChild(i).transform.GetChild(2).GetComponent<Text>().text = scores[i].ExpertScore.ToString("0.00");
                    break;
                default:
                    break;
            }
            if (scores[i].Id == PlayerPrefs.GetInt("id"))
                scoresPanel.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(true);
            Debug.Log(scores[i].Id);
        }
        progressPanel.SetActive(false);
    }

    public void ShowNormalScores(string tableName)
    {
        currentScoreTable = tableName;
        ResetScoresPanel();
        if (isLocalScore)
        {
            switch (tableName)
            {
                case "beginner_score":
                    int count = 0;
                    foreach (BeginnerScore score in dataService.GetBeginnerScores())
                    {
                        scoresPanel.transform.GetChild(count).transform.GetChild(1).GetComponent<Text>().text = score.Nick;
                        scoresPanel.transform.GetChild(count).transform.GetChild(2).GetComponent<Text>().text = score.Time.ToString("0.00");
                        count++;
                        Debug.Log(score.Id);
                    }
                    break;
                case "intermediate_score":
                    count = 0;
                    foreach (IntermediateScore score in dataService.GetIntermediateScores())
                    {
                        scoresPanel.transform.GetChild(count).transform.GetChild(1).GetComponent<Text>().text = score.Nick;
                        scoresPanel.transform.GetChild(count).transform.GetChild(2).GetComponent<Text>().text = score.Time.ToString("0.00");
                        count++;
                    }
                    break;
                case "expert_score":
                    count = 0;
                    foreach (ExpertScore score in dataService.GetExpertScores())
                    {
                        scoresPanel.transform.GetChild(count).transform.GetChild(1).GetComponent<Text>().text = score.Nick;
                        scoresPanel.transform.GetChild(count).transform.GetChild(2).GetComponent<Text>().text = score.Time.ToString("0.00");
                        count++;
                    }
                    break;
            }
        }
        else
        {
            progressPanel.SetActive(true);
            webServiceController.GetScoresSinceIndex(0, tableName);
        }
        customScoresShown = false;
    }

    public void ResetScoresPanel()
    {
        for (int i = 0; i < 10; i++)
        {
            scoresPanel.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(false);
            scoresPanel.transform.GetChild(i).transform.GetChild(1).GetComponent<Text>().text = NICK;
            scoresPanel.transform.GetChild(i).transform.GetChild(2).GetComponent<Text>().text = TIME;
        }
    }

    public void ShowCustomScores()
    {
        customScoresShown = true;
        int count = 0;
        IEnumerable<CustomScore> customScores = dataService.GetCustomScores();
        foreach (CustomScore score in customScores)
        {
            customScoresPanel.transform.GetChild(count).transform.GetChild(0).GetComponent<Text>().text = score.Nick;
            customScoresPanel.transform.GetChild(count).transform.GetChild(1).GetComponent<Text>().text = score.Size;
            customScoresPanel.transform.GetChild(count).transform.GetChild(2).GetComponent<Text>().text = score.Mines.ToString();
            customScoresPanel.transform.GetChild(count).transform.GetChild(3).GetComponent<Text>().text = ((int)score.Time).ToString();
            customScoresPanel.transform.GetChild(count).transform.GetChild(4).GetComponent<Text>().text = score.Score.ToString("0.00");
            count++;
        }
    }
}

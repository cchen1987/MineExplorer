using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using UnityEngine.UI;

public class WebServiceController : MonoBehaviour {

    private const string CONNECTION_STRING = "http://acstudio.esy.es/mineexplorer/";
    private const string REGISTER = "register.php";
    private const string UPDATE_SCORE = "updateScore.php";
    private const string GET_SCORE_BY_ID = "getScoreById.php?id=";
    private const string GET_SCORES_AT_INDEX = "getScoresAtIndex.php";
    private const string CONTENT_TYPE = "Content-Type";
    private const string TYPE = "text/json";
    private WWW connection;
    private string connectionURL;
    private Dictionary<string, string> postHeader;
    private UTF8Encoding encoder;

    public GameObject registrationPanel;
    public InputField nickInput;
    public Text errorText;
    public Text responseText;
    public Dropdown rankOptions;

    public GameStatus gameStatus;
    public ScoreManager scoreManager;

    private const int MAX_CONNECTION_TIME = 15;
    private float connectionTime;
    private bool timeOut;

    // Use this for initialization
    void Start ()
    {
        encoder = new UTF8Encoding();
        postHeader = new Dictionary<string, string>();
        postHeader.Add(CONTENT_TYPE, TYPE);
	}

    public void Register()
    {
        string nick = nickInput.text;
        if (nick != null && nick.Trim() != "")
        {
            StartCoroutine(RegisterUserCoroutine(nick));
        }
        else
        {
            responseText.text = "Nick cannot be empty!";
            errorText.gameObject.SetActive(true);
            responseText.gameObject.SetActive(true);
        }
    }

    public void GetScoreById(int id)
    {
        StartCoroutine(GetScoreByIdCoroutine(id));
    }

    public void GetScoresSinceIndex(int index, string type)
    {
        StartCoroutine(GetScoresSinceIndexCoroutine(index, type));
    }

    public void UpdateScore()
    {
        StartCoroutine(UpdateScoreCoroutine());
    }

    IEnumerator RegisterUserCoroutine(string nick)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            responseText.text = "No internet conection.";
            errorText.gameObject.SetActive(true);
            responseText.gameObject.SetActive(true);
        }
        else
        {
            connectionURL = CONNECTION_STRING + REGISTER;
            UserRegistrationRequest registrationRequest = new UserRegistrationRequest()
            {
                Nick = nick,
                RegisterDate = DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss\\Z")
            };

            string data = JsonUtility.ToJson(registrationRequest);
            connection = new WWW(connectionURL, encoder.GetBytes(data), postHeader);

            connectionTime = 0;
            while (!connection.isDone && !timeOut)
            {
                yield return new WaitForSeconds(1f);
                connectionTime += 1f;
                Debug.Log(connectionTime);

                if (connectionTime >= MAX_CONNECTION_TIME)
                    timeOut = true;
            }
            yield return connection;

            if (timeOut)
            {
                responseText.text = "Connection time out.";
                errorText.gameObject.SetActive(true);
                responseText.gameObject.SetActive(true);
            }
            else if (connection.error != null)
            {
                responseText.text = "Error connecting to server.";
                errorText.gameObject.SetActive(true);
                responseText.gameObject.SetActive(true);
            }
            else
            {
                GeneralResponse response = null;
                if (connection.text != null && connection.text != "")
                {
                    response = JsonUtility.FromJson<GeneralResponse>(connection.text);
                }

                if (response.Status == 1)
                {
                    PlayerPrefs.SetString("registerDate", registrationRequest.RegisterDate);
                    PlayerPrefs.SetString("nick", nick);
                    PlayerPrefs.SetInt("id", response.Id);
                    errorText.text = "Registration successfull!";
                    errorText.color = Color.white;
                    errorText.gameObject.SetActive(true);
                    responseText.gameObject.SetActive(false);
                    yield return new WaitForSeconds(1.5f);
                    registrationPanel.SetActive(false);
                }
                else
                {
                    responseText.text = response.Message;
                    errorText.gameObject.SetActive(true);
                    responseText.gameObject.SetActive(true);
                }
            }
            timeOut = false;
        }
    }

    IEnumerator GetScoreByIdCoroutine(int id)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            scoreManager.SetErrorText("No internet conection.");
        }
        else
        {
            connectionURL = CONNECTION_STRING + GET_SCORE_BY_ID + id;
            connection = new WWW(connectionURL);
            connectionTime = 0;
            while (!connection.isDone && !timeOut)
            {
                yield return new WaitForSeconds(1f);
                connectionTime += 1f;
                Debug.Log(connectionTime);

                if (connectionTime >= MAX_CONNECTION_TIME)
                    timeOut = true;
            }
            yield return connection;

            if (timeOut)
            {

            }
            else if (connection.error != null)
            {
                
            }
            else
            {
                GeneralResponse response = null;
                if (connection.text != null && connection.text != "")
                {
                    response = JsonUtility.FromJson<GeneralResponse>(connection.text);
                }

                Debug.Log(JsonUtility.ToJson(response));

                if (response.Status == 1)
                {
                    // TODO
                }
                else
                {
                    // TODO
                }
            }
            timeOut = false;
        }
    }

    IEnumerator GetScoresSinceIndexCoroutine(int index, string type)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            scoreManager.SetErrorText("No internet conection.");
        }
        else
        {
            connectionURL = CONNECTION_STRING + GET_SCORES_AT_INDEX;
            GeneralRequest request = new GeneralRequest()
            {
                Index = index,
                Type = type
            };
            string data = JsonUtility.ToJson(request);
            connection = new WWW(connectionURL, encoder.GetBytes(data), postHeader);

            connectionTime = 0;
            while (!connection.isDone && !timeOut)
            {
                yield return new WaitForSeconds(1f);
                connectionTime += 1f;
                Debug.Log("time: " + connectionTime);

                if (connectionTime >= MAX_CONNECTION_TIME)
                    timeOut = true;
            }
            yield return connection;

            if (timeOut)
            {
                scoreManager.progressPanel.SetActive(false);
                scoreManager.SetErrorText("Connection time out.");
            }
            else if (connection.error != null)
            {
                scoreManager.SetErrorText("Error connecting to server.");
            }
            else
            {
                GeneralResponse response = null;
                if (connection.text != null && connection.text != "")
                {
                    response = JsonUtility.FromJson<GeneralResponse>(connection.text);
                }
                
                Debug.Log(JsonUtility.ToJson(response));

                if (response.Status == 1)
                {
                    scoreManager.SetScore(response.Scores, type);
                }
                else
                {
                    scoreManager.SetErrorText(response.Message);
                }
            }
            timeOut = false;
        }
    }

    IEnumerator UpdateScoreCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            scoreManager.SetErrorText("No internet conection.");
        }
        else
        {
            connectionURL = CONNECTION_STRING + UPDATE_SCORE;
            GeneralRequest request = new GeneralRequest()
            {
                Id = PlayerPrefs.GetInt("id"),
                Nick = (nickInput.text != null && nickInput.text.Trim() != "" ? nickInput.text : ""),
                RegisterDate = PlayerPrefs.GetString("registerDate"),
                Type = (PlayerPrefs.GetString("difficulty") == "beginner" ? "beginner_score" :
                    (PlayerPrefs.GetString("difficulty") == "intermediate" ? "intermediate_score" : "expert_score")),
                Score = (PlayerPrefs.GetString("difficulty") == "beginner" ? PlayerPrefs.GetFloat("bestBeginnerScore") :
                    (PlayerPrefs.GetString("difficulty") == "intermediate" ? PlayerPrefs.GetFloat("bestIntermediateScore") :
                    PlayerPrefs.GetFloat("bestExpertScore")))
            };
            string data = JsonUtility.ToJson(request);
            //Debug.Log("Data: " + data);
            connection = new WWW(connectionURL, encoder.GetBytes(data), postHeader);

            connectionTime = 0;
            while (!connection.isDone && !timeOut)
            {
                yield return new WaitForSeconds(1f);
                connectionTime += 1f;
                Debug.Log(connectionTime);

                if (connectionTime >= MAX_CONNECTION_TIME)
                    timeOut = true;
            }
            yield return connection;

            if (timeOut)
            {
                gameStatus.SetRankText("Connection time out.");
            }
            else if (connection.error != null)
            {
                gameStatus.SetRankText("Error connecting to server.");
            }
            else
            {
                GeneralResponse response = null;
                if (connection.text != null && connection.text != "")
                {
                    response = JsonUtility.FromJson<GeneralResponse>(connection.text);
                }

                Debug.Log(JsonUtility.ToJson(response));

                if (response.Status == 1)
                {
                    if (nickInput.text != null && nickInput.text.Trim() != "")
                    {
                        PlayerPrefs.SetString("nick", nickInput.text);
                    }

                    gameStatus.SetRankText(response.Message);
                }
                else
                {
                    gameStatus.SetRankText(response.Message);
                }
            }
            timeOut = false;
        }
    }

    public void CancelAllOperation()
    {
        StopAllCoroutines();
    }
}
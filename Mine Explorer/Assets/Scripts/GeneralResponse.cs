using System;
using System.Collections.Generic;

[Serializable]
public class GeneralResponse {
    public int Status;
    public int Id;
    public List<ScoresModel> Scores;
    public string Message;
}

using SQLite4Unity3d;
using UnityEngine;
#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using System.Collections.Generic;

public class DataService
{

    private SQLiteConnection _connection;

    public DataService(string DatabaseName)
    {

#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("Final PATH: " + dbPath);

    }

    public void CreateDB()
    {
        _connection.CreateTable<BeginnerScore>();
        _connection.CreateTable<IntermediateScore>();
        _connection.CreateTable<ExpertScore>();
        _connection.CreateTable<CustomScore>();

        //_connection.InsertAll(new[] {
        //    new BeginnerScore {
        //        Nick = "Chen",
        //        Time = 21
        //    },
        //    new BeginnerScore {
        //        Nick = "Chen2",
        //        Time = 21
        //    },
        //    new BeginnerScore {
        //        Nick = "Chen3",
        //        Time = 21
        //    },
        //    new BeginnerScore {
        //        Nick = "Chen4",
        //        Time = 21
        //    },
        //    new BeginnerScore {
        //        Nick = "Chen5",
        //        Time = 21
        //    },
        //});

        //_connection.InsertAll(new[] {
        //    new IntermediateScore
        //    {
        //        Nick = "juan",
        //        Time = 222
        //    }
        //});

        //_connection.InsertAll(new[] {
        //    new ExpertScore
        //    {
        //        Nick = "manolo",
        //        Time = 243
        //    },
        //    new ExpertScore
        //    {
        //        Nick = "manolo3",
        //        Time = 11
        //    },
        //    new ExpertScore
        //    {
        //        Nick = "manolo2",
        //        Time = 534
        //    },
        //    new ExpertScore
        //    {
        //        Nick = "manolo6",
        //        Time = 2654
        //    }
        //});
    }

    public IEnumerable<BeginnerScore> GetBeginnerScores()
    {
        return _connection.Table<BeginnerScore>().OrderBy(x => x.Time);
    }

    public BeginnerScore GetLowestBeginnerScore()
    {
        return _connection.Table<BeginnerScore>().OrderByDescending(x => x.Time).First();
    }

    public IEnumerable<IntermediateScore> GetIntermediateScores()
    {
        return _connection.Table<IntermediateScore>().OrderBy(x => x.Time);
    }

    public IntermediateScore GetLowestIntermediateScore()
    {
        return _connection.Table<IntermediateScore>().OrderByDescending(x => x.Time).First();
    }

    public IEnumerable<ExpertScore> GetExpertScores()
    {
        return _connection.Table<ExpertScore>().OrderBy(x => x.Time);
    }

    public ExpertScore GetLowestExpertScore()
    {
        return _connection.Table<ExpertScore>().OrderByDescending(x => x.Time).First();
    }

    public IEnumerable<CustomScore> GetCustomScores()
    {
        return _connection.Table<CustomScore>().OrderByDescending(x => x.Score);
    }

    public CustomScore GetLowestCustomScore()
    {
        return _connection.Table<CustomScore>().OrderBy(x => x.Time).First();
    }
    
    public BeginnerScore CreateBeginnerScore(string nick, float time)
    {
        BeginnerScore p = new BeginnerScore
        {
            Nick = nick,
            Time = time
        };
        _connection.Insert(p);
        return p;
    }

    public IntermediateScore CreateIntermediateScore(string nick, float time)
    {
        IntermediateScore p = new IntermediateScore
        {
            Nick = nick,
            Time = time
        };
        _connection.Insert(p);
        return p;
    }

    public ExpertScore CreateExpertScore(string nick, float time)
    {
        ExpertScore p = new ExpertScore
        {
            Nick = nick,
            Time = time
        };
        _connection.Insert(p);
        return p;
    }

    public CustomScore CreateCustomScore(string nick, string size, int mines, float time, float score)
    {
        CustomScore p = new CustomScore
        {
            Nick = nick,
            Size = size,
            Mines = mines,
            Time = time,
            Score = score
        };
        _connection.Insert(p);
        return p;
    }

    public int GetBeginnerScoreCount()
    {
        int count = 0;
        IEnumerable<BeginnerScore> scores = GetBeginnerScores();
        foreach (BeginnerScore score in scores)
        {
            count++;
        }

        return count;
    }

    public int GetIntermediateScoreCount()
    {
        int count = 0;
        IEnumerable<IntermediateScore> scores = GetIntermediateScores();
        foreach (IntermediateScore score in scores)
        {
            count++;
        }

        return count;
    }

    public int GetExpertScoreCount()
    {
        int count = 0;
        IEnumerable<ExpertScore> scores = GetExpertScores();
        foreach (ExpertScore score in scores)
        {
            count++;
        }

        return count;
    }

    public int GetCustomScoreCount()
    {
        int count = 0;
        IEnumerable<CustomScore> scores = GetCustomScores();
        foreach (CustomScore score in scores)
        {
            count++;
        }

        return count;
    }

    public int UpdateBeginnerScore(BeginnerScore beginnerScore)
    {
        BeginnerScore bs = GetLowestBeginnerScore();
        bs.Nick = beginnerScore.Nick;
        bs.Time = beginnerScore.Time;
        return _connection.Update(bs);
    }

    public int UpdateIntermediateScore(IntermediateScore intermediateScore)
    {
        IntermediateScore isc = GetLowestIntermediateScore();
        isc.Nick = intermediateScore.Nick;
        isc.Time = intermediateScore.Time;
        return _connection.Update(isc);
    }

    public int UpdateExpertScore(ExpertScore expertScore)
    {
        ExpertScore ec = GetLowestExpertScore();
        ec.Nick = expertScore.Nick;
        ec.Time = expertScore.Time;
        return _connection.Update(ec);
    }

    public int UpdateCustomScore(CustomScore customScore)
    {
        CustomScore cs = GetLowestCustomScore();
        cs.Nick = customScore.Nick;
        cs.Size = customScore.Size;
        cs.Mines = customScore.Mines;
        cs.Time = customScore.Time;
        cs.Score = customScore.Score;
        return _connection.Update(cs);
    }

    /*
    public IEnumerable<Person> GetPersonsNamedRoberto()
    {

        return _connection.Table<Person>().Where(x => x.Name == "Roberto");
    }

    public Person GetJohnny()
    {
        return _connection.Table<Person>().Where(x => x.Name == "Johnny").FirstOrDefault();
    }
    */
}

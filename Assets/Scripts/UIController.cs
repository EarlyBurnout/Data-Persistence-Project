using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using static UnityEngine.Analytics.IAnalytic;
using JetBrains.Annotations;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    public GameObject nameField;
    public GameObject[] scoreList;

    public string nameInput;
    public int score;
    public string scoreTop;

    // create files (if no) and make an instance
    private void Awake()
    {
        MakeScoreFiles();
        LoadScoreTop();
        LoadLeaderboard();
        LoadName();
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // save info class
    [System.Serializable]
    class SaveData
    {
        public string nameInput;
        public int score;
    }

    // launch main scene
    public void StartGame()
    {
        nameInput = nameField.GetComponent<TMP_InputField>().text;
        score = 0;
        SceneManager.LoadScene(1);
    }

    // quit game
    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void LoadName()
    {
        string path = Application.persistentDataPath + "/lastscore.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            nameField.GetComponent<TMP_InputField>().text = data.nameInput;
        }
    }
    // load best score for main, load leaderboard
    public void LoadScoreTop()
    {
        // best for game
        string path = Application.persistentDataPath + "/save1.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            scoreTop = "Best score: " + data.nameInput + " - " + data.score.ToString();
        }
    }

    public void LoadLeaderboard()
    {
        // for leaderboard
        for (int i = 0; i < 3; i++)
        {
            string path = Application.persistentDataPath + "/save" + (i+1) + ".json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                scoreList[i].GetComponent<TMP_Text>().text = (i+1).ToString() + ". " + data.nameInput + " - " + data.score.ToString();
            }
        }
    }

    // save last user's score and update leaderboard
    public void SaveScore(string nameInput, int score)
    {
        SaveData data = new SaveData();
        data.nameInput = nameInput;
        data.score = score;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/lastscore.json", json);
        CheckFiles(score);
    }

    public void UpdateSaves(string saveNameFrom, string saveNameTo) // lastscore or saveX, saveY
    {
        // get paths of files
        string pathFrom = Application.persistentDataPath + "/" + saveNameFrom + ".json";
        string pathTo = Application.persistentDataPath + "/" + saveNameTo + ".json";
        if (File.Exists(pathFrom) && File.Exists(pathTo))
        {
            // get info from
            string jsonFrom = File.ReadAllText(pathFrom);
            // exchange
            File.WriteAllText(pathTo, jsonFrom);
        }
    }

    // compare score to top 3
    // if >1 -> make save2-save3, save1-save2, rewrite save1 (EXACTLY THIS ORDER)
    // if >2 -> make save2-save3, rewrite save2
    // if >3 -> rewrite save3
    public void CheckFiles(int score)
    {
        // 1
        string path = Application.persistentDataPath + "/save1.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data.score < score)
            {
                UpdateSaves("save2", "save3");
                UpdateSaves("save1", "save2");
                UpdateSaves("lastscore", "save1");
            }
            else
            {
                // 2
                path = Application.persistentDataPath + "/save2.json";
                if (File.Exists(path))
                {
                    string json2 = File.ReadAllText(path);
                    SaveData data2 = JsonUtility.FromJson<SaveData>(json2);
                    if (data2.score < score)
                    {
                        UpdateSaves("save2", "save3");
                        UpdateSaves("lastscore", "save2");
                    }
                    else
                    {
                        // 3
                        path = Application.persistentDataPath + "/save3.json";
                        if (File.Exists(path))
                        {
                            string json3 = File.ReadAllText(path);
                            SaveData data3 = JsonUtility.FromJson<SaveData>(json3);
                            if (data3.score < score)
                            {
                                UpdateSaves("lastscore", "save3");
                            }
                        }
                    }
                }
            }
        }
    }

    // create score files for top 3 scores if no such files
    public void MakeScoreFiles()
    {
        for (int i  = 0; i < 3; i++)
        {
            string path = Application.persistentDataPath + "/save" + (i + 1) + ".json";
            if (!File.Exists(path))
            {
                SaveData data = new SaveData();
                data.nameInput = "none";
                data.score = 0;
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(path, json);
            }
        }
    }
}

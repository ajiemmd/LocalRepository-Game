using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavaManager : Singleton<SavaManager>
{
    string sceneName = "level";

    public string SceneName { get { return PlayerPrefs.GetString(sceneName); } }
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
            Debug.Log("返回主界面");
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            SavaPlayerData();
            Debug.Log("保存游戏");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
            Debug.Log("载入游戏");
        }

    }

    public void SavaPlayerData()
    {
        Sava(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }
    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void Sava(Object data,string key)
    {
        var jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(Object data, string key)
    {
        if(PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }

}

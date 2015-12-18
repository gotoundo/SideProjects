﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AppManager : MonoBehaviour {
    
    public static AppManager Main;

    public List<LevelData> Levels;
    public LevelData DemoLevel;

    public static bool CheatingEnabled = false;

    public static LevelData CurrentLevel;

    public static void LoadLevel(LevelData level)
    {
        FoWManager.ClearSingleton();
        CurrentLevel = level;
        LoadScene(level.SceneID);
    }

    public static void LoadScene(string SceneID)
    {
        SceneManager.LoadScene(SceneID);
    }

    void Awake()
    {
        if (Main == null)
        {
            Main = this;
            DontDestroyOnLoad(gameObject);
            if (!SaveTool.Load())
                SaveTool.ResetSaveData();
        }
        else
            Destroy(gameObject);

    }
    // Use this for initialization
    void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}



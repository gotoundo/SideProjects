using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class SaveTool
{
    const string saveFileName = "/savedGames.gd";

    public static void ResetSaveData()
    {
        SaveData.current = new SaveData();
        Save();
    }

    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + saveFileName); //you can call it anything you want
        bf.Serialize(file, SaveData.current);
        file.Close();
        Debug.Log("Saved " + Application.persistentDataPath + saveFileName);
    }

    public static bool Load()
    {
        if (File.Exists(Application.persistentDataPath + saveFileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + saveFileName, FileMode.Open);

            try
            {
                SaveData.current = (SaveData)bf.Deserialize(file);
                file.Close();
            }
            catch (Exception err)
            {
                Debug.LogError("Could not access Save Data at " + Application.persistentDataPath + saveFileName + " -  " + err.ToString());
                return false;
            }

            if (SaveData.current == null)
            {
                Debug.LogError("Failed to load save at " + Application.persistentDataPath + saveFileName);
                return false;
            }

            Debug.Log("Loaded " + Application.persistentDataPath + saveFileName);
            return true;
        }
        else
        {
            Debug.Log("Failed to load " + Application.persistentDataPath + saveFileName);
            return false;
        }

    }
}

[System.Serializable]
public class SaveData
{
    public static SaveData current;

    public List<LevelData.LockID> UnlockedLevels;
    public bool cheatsEnabled()
    {
#if UNITY_EDITOR
        return true;
#endif
        return false;
    }

    public static void VictoryUnlock(LevelData.LockID id)
    {
        if (!current.UnlockedLevels.Contains(id))
            current.UnlockedLevels.Add(id);
    }

    public SaveData()
    {
        UnlockedLevels = new List<LevelData.LockID>();
    }
    
}


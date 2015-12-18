using UnityEngine;
using System.Collections.Generic;

public class LevelData : MonoBehaviour {

    public enum LockID { L01,L02,L03,L04,L05,L06,L07,L08,L09,L10,L11,L12}

    public string SceneID;
    public string LevelName;
    public DialogInfo GameStartDialog;
    public int StartingGold;
    public int MaxCastleLevel = 3;
    public List<BasicUnit> BannedStructures;

    [TextArea(3, 10)]
    public string ObjectiveText;

    public List<LockID> LocksRequired;
    public List<LockID> LocksGranted;
    //public DialogInfo GameWinDialog;
    //public DialogInfo GameLoseDialog;

    [System.Serializable]
    public class DialogInfo
    {
        //public enum Trigger {GameStart,GameWin,GameLose};
        //public Trigger trigger;
        public string StoryTitle;
        public string StoryHonorific = "Empress,";
        [TextArea(3, 10)]
        public string StoryText;
        public int PortraitID;
        public string AcceptText;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SaveWin()
    {
        foreach(LockID id in LocksGranted)
            SaveData.VictoryUnlock(id);

        SaveTool.Save();
    }
}

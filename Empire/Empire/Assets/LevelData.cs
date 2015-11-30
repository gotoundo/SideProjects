using UnityEngine;
using System.Collections.Generic;

public class LevelData : MonoBehaviour {
    public string LevelID;
    public string LevelName;
    public DialogInfo GameStartDialog;
    public int StartingGold;
    public int MaxCastleLevel = 3;
    public List<BasicUnit> BannedStructures;
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
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIStartGameButton : MonoBehaviour {

    public LevelData levelData;

    Button myButton;

    

	// Use this for initialization
	void Start () {
        myButton = GetComponentInChildren<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelData != null)
        {
            myButton.GetComponentInChildren<Text>().text = levelData.LevelName;

            bool interactable = true;
            foreach (LevelData.LockID requiredID in levelData.LocksRequired)
            {
                if (!SaveData.current.UnlockedLevels.Contains(requiredID))
                    interactable = false;
            }
            myButton.interactable = interactable;
        }
    }

    public void Click()
    {
        FoWManager.ClearSingleton();
        AppManager.LoadLevel(levelData);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class UIMainMenu : MonoBehaviour {

    public GameObject MapSelectWindow;
    public GameObject MapSelectList;
    public GameObject MapSelectButtonTemplate;

    public List<LevelData> Levels;

	// Use this for initialization
	void Start () {
        MapSelectWindow.SetActive(false);

        foreach(LevelData levelData in Levels)
        {
            GameObject mapButton = Instantiate(MapSelectButtonTemplate);
            mapButton.GetComponent<UIStartGameButton>().levelData = levelData;
            mapButton.transform.SetParent(MapSelectList.transform);
        }
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

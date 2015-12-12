using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIStoryWindow : MonoBehaviour {

    public UIWindowTitle WindowTitle;
    public Text AddressText;
    public Text MessageText;
    public Text ConfirmText;

    public void SetStoryFields(LevelData.DialogInfo Info)
    {
        WindowTitle.title = Info.StoryTitle;
        AddressText.text = Info.StoryHonorific;
        MessageText.text = Info.StoryText;
        ConfirmText.text = Info.AcceptText;

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

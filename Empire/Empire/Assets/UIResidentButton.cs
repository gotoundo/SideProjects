using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIResidentButton : MonoBehaviour {
    public BasicUnit resident;
    Text buttonText;
	// Use this for initialization
	void Start () {
        buttonText = GetComponentInChildren<Text>();

    }
	
	// Update is called once per frame
	void Update () {

        if(resident!=null)
        {
            buttonText.text = resident.name + " " + (int)(100*resident.getHealthPercentage) + "%";
        }
	
	}

    public void Click()
    {
        GameManager.Main.StartInspection(resident);
        GameManager.Main.CenterCameraOnUnit(resident, false);
        GameManager.Main.MenuAction();
    }
}

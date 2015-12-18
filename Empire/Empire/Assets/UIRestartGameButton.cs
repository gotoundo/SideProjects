using UnityEngine;
using System.Collections;

public class UIRestartGameButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        AppManager.LoadLevel(AppManager.CurrentLevel);
    }
}

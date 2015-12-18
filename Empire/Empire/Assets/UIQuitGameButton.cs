using UnityEngine;
using System.Collections;

public class UIQuitGameButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        AppManager.LoadScene("IntroScene");
    }
}

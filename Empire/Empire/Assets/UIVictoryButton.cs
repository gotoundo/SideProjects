using UnityEngine;
using System.Collections;

public class UIVictoryButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        Application.LoadLevel("IntroScene");
    }
}

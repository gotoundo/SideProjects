using UnityEngine;
using System.Collections;

public class UIStartGameButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        Application.LoadLevel("MainScene");
    }
}

using UnityEngine;
using System.Collections;

public class UIOptionsOpenButton : MonoBehaviour {

    public bool OpensOptions = true;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click ()
    {
        if (OpensOptions)
            GameManager.Main.OpenOptionsPanel();
        else
            GameManager.Main.CloseOptionsPanel();
    }
}

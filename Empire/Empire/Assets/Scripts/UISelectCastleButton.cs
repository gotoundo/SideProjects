using UnityEngine;
using System.Collections;

public class UISelectCastleButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Click()
    {
        BasicUnit castle = GameObject.FindGameObjectWithTag("Castle").GetComponent<BasicUnit>();
        if (GameManager.Main.InspectedUnit != castle)
        {
            GameManager.Main.StartInspection(castle);
        }
        else
        {
            GameManager.Main.CenterCameraOnUnit(castle, false);
        }
    }
}

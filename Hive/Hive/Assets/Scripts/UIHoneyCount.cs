using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHoneyCount : MonoBehaviour {


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(GameEngine.singleton.PlayerQueen)
        GetComponent<Text>().text = "Honey Stored: "+(int)GameEngine.singleton.PlayerQueen.GetComponent<DroneBase>().HoneyHeld;
	
	}
}

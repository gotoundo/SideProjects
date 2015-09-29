using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {
    public static GameEngine singleton;

    public GameObject PlayerQueen;
    public GameObject Marker;
    public GameObject PlayableArea;
    public GameObject UpgradePanel;




	// Use this for initialization
	void Start () {
        singleton = this;

	
	}
	
	// Update is called once per frame
	void Update () {
        
	
	}
}

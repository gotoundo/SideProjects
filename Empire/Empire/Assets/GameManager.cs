using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public static GameManager Main;
    public float Date;
    public Team Player;
    public List<Team> AllTeams;
    public bool Running = false;

    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
    }

	// Use this for initialization
	void Start () {
        Running = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

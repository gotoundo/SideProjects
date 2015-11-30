using UnityEngine;
using System.Collections.Generic;

public class AppManager : MonoBehaviour {

    public static AppManager Main;

    public List<LevelData> Levels;

    void Awake()
    {
        if (Main == null)
        {
            Main = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }
    // Use this for initialization
    void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}



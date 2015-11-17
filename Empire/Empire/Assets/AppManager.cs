using UnityEngine;
using System.Collections;

public class AppManager : MonoBehaviour {

    public static AppManager Main;

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



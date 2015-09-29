using UnityEngine;
using System.Collections;

public class UpgradeBase : MonoBehaviour {

    


    protected DroneBase myDrone;
    void Start()
    {
        myDrone = GetComponent<DroneBase>();
    }

    // Update is called once per frame
    void Update () {
	
	}
}

using UnityEngine;
using System.Collections;

public class HoneyPot : MonoBehaviour {

    //public float HoneyCapacity;
    //public float HoneyHeld;

    DroneBase myDrone;

    // Use this for initialization
    void Start () {
        
        myDrone = GetComponent<DroneBase>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!myDrone.Dead)
        {
            MeshRenderer mrenderer = GetComponent<MeshRenderer>();
            mrenderer.material.color = new Color(myDrone.HoneyHeld / myDrone.HoneyCapacity, myDrone.HoneyHeld / myDrone.HoneyCapacity, 0, myDrone.HoneyHeld / myDrone.HoneyCapacity);
        }
	}
}

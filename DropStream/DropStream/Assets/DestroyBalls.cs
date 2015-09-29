using UnityEngine;
using System.Collections;

public class DestroyBalls : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision) {
		
		GameObject collidingObject = collision.gameObject;
		if (collidingObject.CompareTag ("Ball")) {
			Destroy (collidingObject);
		}
		
	}
}

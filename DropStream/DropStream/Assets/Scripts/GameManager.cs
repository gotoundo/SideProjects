using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static int currentScore;

	//public static bool releaseBalls;

	// Use this for initialization
	void Start () {
		//releaseBalls = false;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/*public void toggleReleaseBalls()
	{
		releaseBalls = !releaseBalls;
		if (!releaseBalls) {
			currentScore = 0;
			foreach(GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
				Destroy(ball);
		}
	}*/
}

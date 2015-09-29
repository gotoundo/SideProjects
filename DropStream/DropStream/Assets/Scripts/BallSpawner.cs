using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour {

	public GameObject Ball;
	public float Cooldown;
	float remainingCooldown;
	//public float LocationRandomness;
	public Vector3 BallVelocity ;

	// Use this for initialization
	void Start () {
		remainingCooldown = Cooldown;
	}
	
	// Update is called once per frame
	void Update () {

		//if (GameManager.releaseBalls) {
			remainingCooldown -= Time.deltaTime;
			if (remainingCooldown <= 0) {
				remainingCooldown = Cooldown;
				Vector3 randomOffset = new Vector3 (Random.Range (-transform.localScale.x, transform.localScale.y) / 2,
			                                   Random.Range (-transform.localScale.y, transform.localScale.y) / 2, 0);
				GameObject newBall = (GameObject)Instantiate (Ball, transform.position + randomOffset, transform.rotation);
				newBall.GetComponent<Rigidbody> ().velocity = BallVelocity;
			}
		//}

	}
}

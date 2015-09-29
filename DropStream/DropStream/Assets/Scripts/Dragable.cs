using UnityEngine;
using System.Collections;

public class Dragable : MonoBehaviour {

	private Color startingColor;
	// Use this for initialization
	void Start () {
		startingColor = GetComponent<Renderer> ().material.color;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate()
	{
		if(destroyBalls)
			GetComponent<Renderer> ().material.color = Color.red;
	}

	private Vector3 screenPoint;
	private Vector3 offset;
	bool destroyBalls = false;

	//


	void OnCollisionStay(Collision collision) {
		BurnBalls (collision);

	}

	void OnCollisionEnter(Collision collision){
		BurnBalls (collision);
	}

	void BurnBalls(Collision collision)
	{
		if (destroyBalls && collision != null) {
			GameObject collidingObject = collision.gameObject;
			if (collidingObject.CompareTag ("Ball")) {
				GameManager.currentScore++;
				Destroy (collidingObject);
			}
		}
	}



	
	void OnMouseDown(){

			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		destroyBalls = true;


	}

	void OnMouseUp()
	{
		destroyBalls = false;
		GetComponent<Renderer> ().material.color = startingColor;
	}
	
	void OnMouseDrag(){

			Vector3 cursorPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorPoint) + offset;
			transform.position = cursorPosition;

	}
}

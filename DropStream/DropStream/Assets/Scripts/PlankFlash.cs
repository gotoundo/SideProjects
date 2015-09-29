using UnityEngine;
using System.Collections;

public class PlankFlash : MonoBehaviour {

	public Color touchColor;
	public float flashDuration;

	private Color startingColor;
	private float remainingFlashDuration;

	// Use this for initialization
	void Start () {
		startingColor = GetComponent<Renderer> ().material.color;
	}
	
	// Update is called once per frame
	void Update () {

		remainingFlashDuration -= Time.deltaTime;
		if (remainingFlashDuration <= 0) {
			remainingFlashDuration = 0;
			GetComponent<Renderer> ().material.color = startingColor;
		}
	
	}

	void OnCollisionEnter(Collision collision) {

		GetComponent<Renderer> ().material.color = touchColor;
		remainingFlashDuration = flashDuration;

	}
}

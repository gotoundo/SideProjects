using UnityEngine;
using System.Collections;

public class PlankSlide : MonoBehaviour {
	public float Speed;
	public Vector2[] Locations;


	private int destinationID;

	 Vector2 OriginalPosition;
	 Vector2 PreviousPosition;
	 Vector2 NextPosition;
	 Vector2 direction;
	
	void Start () {
		OriginalPosition = new Vector2 (transform.position.x, transform.position.y);
		PreviousPosition = OriginalPosition;
		NextPosition = OriginalPosition+Locations [0];
	}

	// Update is called once per frame
	void Update () {
		if (Vector2.Distance (transform.position, NextPosition) <= Speed*Time.deltaTime) {
			transform.position = NextPosition;
			PreviousPosition = NextPosition;
			destinationID = destinationID + 1 < Locations.Length ? destinationID + 1 : -1;

			if (destinationID != -1)
				NextPosition = PreviousPosition + Locations [destinationID];
			else
				NextPosition = OriginalPosition;
			
		}

		direction = (NextPosition - new Vector2(transform.position.x,transform.position.y) ).normalized;
		transform.Translate (direction * Time.deltaTime*Speed);
	}
}

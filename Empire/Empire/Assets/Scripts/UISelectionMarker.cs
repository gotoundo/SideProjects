using UnityEngine;
using System.Collections;

public class UISelectionMarker : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.SetParent(GameManager.Main.HealthBarFolder.transform);
    }
	
	// Update is called once per frame
	void Update () {
        BasicUnit selectedUnit = GameManager.Main.InspectedUnit;
        if (selectedUnit == null)
            transform.localScale = Vector3.zero;
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.position = Camera.main.WorldToScreenPoint(selectedUnit.transform.position + new Vector3(0, 0, 2));
            transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
        }
    }
}

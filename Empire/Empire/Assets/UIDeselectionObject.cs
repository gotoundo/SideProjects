using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
//using System;

public class UIDeselectionObject : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
{
    float dragSpeed = .1f;
    public void OnDrag(PointerEventData eventData)
    {
       // if (!GameManager.Main.PlacementMode)
            GameManager.Main.camera.transform.position -= new Vector3(eventData.delta.x, 0, eventData.delta.y) * dragSpeed;
    }


    
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Main.EndInspection();
    }

    public void OnScroll(PointerEventData eventData)
    {
        GameManager.Main.camera.transform.position -= new Vector3(0, eventData.scrollDelta.y, 0);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

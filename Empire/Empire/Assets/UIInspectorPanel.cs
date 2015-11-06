using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using System;

public class UIInspectorPanel : MonoBehaviour,IDragHandler {
    public Text text;
    
    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
        

        //throw new NotImplementedException();
    }

    void Awake()
    { text = text ? text : GetComponentInChildren<Text>(); }


    // Use this for initialization
    void Start () {
        
	
	}
	
	// Update is called once per frame
	void Update () {
        

    }

    public void InspectNewObject()
    {
        text.text = GameManager.Main.InspectedUnit.name;
    }
}

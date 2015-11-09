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
		BasicUnit inspectedUnit = GameManager.Main.InspectedUnit;
        
		text.text = inspectedUnit.name;
		text.text += "\n" + inspectedUnit.currentState.ToString();
		text.text += "\n" + inspectedUnit.currentHealth + "/" + inspectedUnit.getMaxHP+" HP";
		text.text += "\n" + inspectedUnit.Gold + " Gold";
		for (int i =0; i<inspectedUnit.EquipmentSlots.Length; i++) {
			if(inspectedUnit.EquipmentSlots[i].Instance!=null)
				text.text += "\n Item: "+inspectedUnit.EquipmentSlots[i].Instance.name;
		}
    }

    public void InspectNewObject()
    {
        text.text = GameManager.Main.InspectedUnit.name;
    }
}

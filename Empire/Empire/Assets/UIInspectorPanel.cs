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
		if (inspectedUnit != null) {
        
			text.text = inspectedUnit.name;
			text.text += "\n" + inspectedUnit.currentState.ToString ();
			text.text += "\n" + Mathf.RoundToInt(inspectedUnit.currentHealth) + "/" + inspectedUnit.getMaxHP + " HP";
			text.text += "\n" + inspectedUnit.Gold + " Gold";

            if (!inspectedUnit.Tags.Contains(BasicUnit.Tag.Structure))
            {
                text.text += "\n" + inspectedUnit.GetStat(BasicUnit.Stat.Strength) + " Strength";
                text.text += "\n" + inspectedUnit.GetStat(BasicUnit.Stat.Dexterity) + " Dexterity";
                text.text += "\n" + inspectedUnit.GetStat(BasicUnit.Stat.Intelligence) + " Intelligence";

                for (int i = 0; i < inspectedUnit.EquipmentSlots.Count; i++)
                {
                    if (inspectedUnit.EquipmentSlots[i].Instance != null)
                        text.text += "\n Item: " + inspectedUnit.EquipmentSlots[i].Instance.name;
                }
            }
		} else
			GameManager.Main.EndInspection ();
    }

    public void InspectNewObject()
    {
        text.text = GameManager.Main.InspectedUnit.name;
    }
}

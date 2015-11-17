using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIHeroList : MonoBehaviour {
    public Text textList;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        textList.text = "Heroes:";
        foreach (BasicUnit unit in GameManager.Main.Player.GetUnits())
        {
            if(unit.Tags.Contains(BasicUnit.Tag.Hero))
            textList.text += "\n" + unit.gameObject.name + " " + unit.Level + " " + unit.currentState.ToString() + " HP:"+(int)(100*unit.currentHealth/unit.getMaxHP)+"%";
        }
	
	}
}

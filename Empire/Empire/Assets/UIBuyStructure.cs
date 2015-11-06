using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBuyStructure : MonoBehaviour {
    public BasicUnit StructureTemplate;
    Button UIButton;

   

	// Use this for initialization
	void Start () {
        UIButton = GetComponent<Button>();
        UIButton.GetComponentInChildren<Text>().text = StructureTemplate.GoldCost + " | " + StructureTemplate.gameObject.name;
	}
	
	// Update is called once per frame
	void Update () {
        UIButton.enabled = GameManager.Main.Player.CanAffordStructure(StructureTemplate);
    }
    public void BuyStructure()
    {
        GameManager.Main.Player.EnterPlaceStructureMode(StructureTemplate);
    }
}

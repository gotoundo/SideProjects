using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBuyStructure : MonoBehaviour {
    public BasicUnit StructureTemplate;
    Button UIButton;

   

	// Use this for initialization
	void Start () {
        UIButton = GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
        UIButton.interactable = GameManager.Main.Player.CanAffordStructure(StructureTemplate) && GameManager.Main.Player.AllowedToBuildUnit(StructureTemplate);
    }
    public void BuyStructure()
    {
        GameManager.Main.Player.EnterPlaceStructureMode(StructureTemplate);
        
    }
}

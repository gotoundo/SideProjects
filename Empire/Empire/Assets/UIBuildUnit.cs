using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBuildUnit : MonoBehaviour {
    public Text buttonText;
	// Use this for initialization
	void Start () {
	
	}
    BasicUnit selectedUnit;
    // Update is called once per frame
    void Update () {
        selectedUnit = GameManager.Main.InspectedUnit;
        if(selectedUnit.SpawnType!=null)
            buttonText.text = selectedUnit.SpawnType.gameObject.name + " ("+ selectedUnit.SpawnType.GetComponent<BasicUnit>().GoldCost +" gold)";
	
	}

    public void Click()
    {
        
        if (selectedUnit.CanSpawn())
        {
            selectedUnit.Spawn(selectedUnit.SpawnType, selectedUnit.gameObject.transform.position); 
        }
    }
}

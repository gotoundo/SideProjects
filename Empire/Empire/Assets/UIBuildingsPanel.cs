using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIBuildingsPanel : MonoBehaviour {
    public GameObject BuildingButtonTemplate;
    public List<Button> StructureButtons;

    void Awake()
    {
        StructureButtons = new List<Button>();
    }
	void Start () {
        UpdateButtons();
    }
	
	// Update is called once per frame
	void Update () {
        
	
	}

    public void UpdateButtons()
    {
        foreach (Button button in StructureButtons)
            Destroy(button.gameObject);
        StructureButtons = new List<Button>();

        //List<BasicUnit> availableStructures = new List<BasicUnit>();
        foreach(BasicUnit structureTemplate in GameManager.Main.Player.BuildableStructureTemplates())
        {
            GameObject buildButton = Instantiate(BuildingButtonTemplate);
            StructureButtons.Add(buildButton.GetComponent<Button>());
            buildButton.transform.SetParent(transform);
            UIBuyStructure structureTool = buildButton.GetComponent<UIBuyStructure>();
            buildButton.GetComponent<Button>().GetComponentInChildren<Text>().text = structureTemplate.GoldCost + " | " + structureTemplate.gameObject.name;
            structureTool.StructureTemplate = structureTemplate;
        }
    }
}

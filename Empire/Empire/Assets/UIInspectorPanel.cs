using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using System;

public class UIInspectorPanel : MonoBehaviour,IDragHandler {
    public Text text;
    public GameObject HireButtonTemplate;
    public GameObject ResearchButtonTemplate;
    public GameObject StructureLevelUpButtonTemplate;
    public GameObject LevelUpSlot;
    
    public List<Button> HireButtons;
    public List<Button> ResearchButtons;
    public Button StructureLevelUpButton;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
        

        //throw new NotImplementedException();
    }

    void Awake()
    {
        text = text ? text : GetComponentInChildren<Text>();
        HireButtons = new List<Button>();
        ResearchButtons = new List<Button>();
    }


    // Use this for initialization
    void Start () {
        
	
	}

    // Update is called once per frame
    void Update()
    {
        BasicUnit myUnit = GameManager.Main.InspectedUnit;
        if (myUnit != null)
        {
            text.text = myUnit.name;
            text.text += "\n" + myUnit.currentState.ToString();
            text.text += "\n Level " + myUnit.Level + (!myUnit.Tags.Contains(BasicUnit.Tag.Structure) ? " (" + (int)myUnit.XP + " xp)" : "");
            text.text += "\n" + Mathf.RoundToInt(myUnit.currentHealth) + "/" + myUnit.getMaxHP + " HP";
            text.text += "\n" + myUnit.Gold + " Gold";

            if (!myUnit.Tags.Contains(BasicUnit.Tag.Structure))
            {
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Strength) + " Strength";
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Dexterity) + " Dexterity";
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Intelligence) + " Intelligence";

                for (int i = 0; i < myUnit.EquipmentSlots.Count; i++)
                {
                    if (myUnit.EquipmentSlots[i].Instance != null)
                        text.text += "\n Item: " + myUnit.EquipmentSlots[i].Instance.name;
                }
            }
        }
        else
            GameManager.Main.EndInspection();
    }

    public void InspectNewObject()
    {
        BasicUnit inspectedUnit = GameManager.Main.InspectedUnit;

        text.text = inspectedUnit.name;

        foreach (Button button in HireButtons)
            Destroy(button.gameObject);
        foreach (Button button in ResearchButtons)
            Destroy(button.gameObject);
        if (StructureLevelUpButton != null)
            Destroy(StructureLevelUpButton.gameObject);
        

        HireButtons = new List<Button>();
        ResearchButtons = new List<Button>();

        if (inspectedUnit.team == GameManager.Main.Player)
        {

            foreach (BasicUnit.UnitSpawner spawner in inspectedUnit.Spawners)
            {
                if (spawner.canBeHired)
                {
                    GameObject hireButton = Instantiate(HireButtonTemplate);
                    HireButtons.Add(hireButton.GetComponent<Button>());
                    hireButton.transform.SetParent(transform);
                    UIBuildUnit buildUnit = hireButton.GetComponent<UIBuildUnit>();
                    buildUnit.buttonText.text = "Hire " + spawner.SpawnType.gameObject.name + " (" + spawner.SpawnType.GoldCost + ")";
                    buildUnit.spawner = spawner;
                }
            }

            foreach (BasicUpgrade upgrade in inspectedUnit.AvailableUpgrades)
            {
                if (upgrade.IsVisible())
                {
                    GameObject upgradeButton = Instantiate(ResearchButtonTemplate);
                    ResearchButtons.Add(upgradeButton.GetComponent<Button>());
                    upgradeButton.transform.SetParent(transform);
                    UIResearchButton researchUIObject = upgradeButton.GetComponent<UIResearchButton>();
                    researchUIObject.buttonText.text = "Research " + upgrade.gameObject.name + " (" + upgrade.Cost + ")";
                    researchUIObject.upgrade = upgrade;
                }
            }

            if(inspectedUnit.Tags.Contains(BasicUnit.Tag.Structure) && inspectedUnit.AnotherStructureLevelExists())
            {
                GameObject levelUpButton = Instantiate(StructureLevelUpButtonTemplate);
                StructureLevelUpButton = levelUpButton.GetComponent<Button>();
                levelUpButton.transform.SetParent(LevelUpSlot.transform);
                UILevelUpStructure levelUpUIObject = levelUpButton.GetComponent<UILevelUpStructure>();
                levelUpUIObject.buttonText.text = "Level Up " + inspectedUnit.gameObject.name + " (" + inspectedUnit.LevelUpCost()+ ")";
            }

        }
    }
}

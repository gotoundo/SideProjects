using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using System;

public class UIInspectorPanel : MonoBehaviour,IDragHandler {
    public Text text;
    public GameObject HireButtonTemplate;
    public GameObject UpgradeButtonTemplate;
    public GameObject StructureButtonTemplate;
    public GameObject LevelUpButtonTemplate;
    public GameObject ResidentButtonTemplate;

    public GameObject HirePanel;
    public GameObject UpgradePanel;
    public GameObject StructurePanel;
    public GameObject LevelUpPanel;
    public GameObject ResidentPanel;
    
    public List<Button> HireButtons;
    public List<Button> UpgradeButtons;
    public List<Button> StructureButtons;
    public List<Button> LevelUpButtons;
    public List<Button> ResidentButtons;

    public Text HireTitle;


    public void OnDrag(PointerEventData eventData)
    {
      //  transform.position += (Vector3)eventData.delta;
        

        //throw new NotImplementedException();
    }

    void Awake()
    {
        text = text ? text : GetComponentInChildren<Text>();
        HireButtons = new List<Button>();
        UpgradeButtons = new List<Button>();
        StructureButtons = new List<Button>();
        LevelUpButtons = new List<Button>();
        ResidentButtons = new List<Button>();
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

            if(myUnit.HasTag(BasicUnit.Tag.Hero)||myUnit.HasTag(BasicUnit.Tag.Structure))
            {
                text.text+= " (Lvl " + myUnit.Level + ")";
            }

            if (myUnit.HasTag(BasicUnit.Tag.Hero))
            {
                text.text += "\n" + (int)myUnit.XP + " xp";
                text.text += "\n" + myUnit.currentState.ToString();
            }
            text.text += "\n" + Mathf.RoundToInt(myUnit.currentHealth) + "/" + myUnit.getMaxHP + " HP";

            if (myUnit.HasTag(BasicUnit.Tag.Hero)|| myUnit.HasTag(BasicUnit.Tag.Structure))
                text.text += "\n" + myUnit.Gold + " Gold";

            if (myUnit.HasTag(BasicUnit.Tag.Hero))
            {
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Strength) + " Strength";
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Dexterity) + " Dexterity";
                text.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Intelligence) + " Intelligence";

                for (int i = 0; i < myUnit.EquipmentSlots.Count; i++)
                {
                    if (myUnit.EquipmentSlots[i].Instance != null)
                        text.text += "\n Item: " + myUnit.EquipmentSlots[i].Instance.name;
                }
                text.text += "\n Salves: " + myUnit.Potions.Count;
            }

            if(myUnit.maxHirelings > 0)
            {
                HireTitle.text = "Hire Units - " + myUnit.GetHiredHeroes().Count + "/" + myUnit.maxHirelings;
            }
        }
        else
            GameManager.Main.EndInspection();
    }


    public void ObjectUpdated()
    {
        InspectNewObject();
    }

    public void InspectNewObject()
    {
        BasicUnit inspectedUnit = GameManager.Main.InspectedUnit;

        text.text = inspectedUnit.name;

        foreach (Button button in HireButtons)
            Destroy(button.gameObject);
        foreach (Button button in UpgradeButtons)
            Destroy(button.gameObject);
        foreach (Button button in StructureButtons)
            Destroy(button.gameObject);
        foreach (Button button in LevelUpButtons)
            Destroy(button.gameObject);
        foreach (Button button in ResidentButtons)
            Destroy(button.gameObject);


        HireButtons = new List<Button>();
        UpgradeButtons = new List<Button>();
        StructureButtons = new List<Button>();
        LevelUpButtons = new List<Button>();
        ResidentButtons = new List<Button>();

        HirePanel.SetActive(false);
        UpgradePanel.SetActive(false);
        StructurePanel.SetActive(false);
        LevelUpPanel.SetActive(false);
        ResidentPanel.SetActive(false);

        if (inspectedUnit.team == GameManager.Main.Player)
        {
            foreach (BasicUnit.UnitSpawner spawner in inspectedUnit.Spawners)
            {
                if (spawner.canBeHired)
                {
                    GameObject hireButton = createAndParentButton(HirePanel, HireButtonTemplate, HireButtons, spawner.SpawnType.templateID + " (" + spawner.SpawnType.ScaledGoldCost() + ")");
                    hireButton.GetComponent<UIBuildUnit>().spawner = spawner;
                }
            }

            foreach (BasicUpgrade upgrade in inspectedUnit.AvailableUpgrades)
            {
                if (upgrade.IsVisible())
                {
                    GameObject upgradeButton = createAndParentButton(UpgradePanel, UpgradeButtonTemplate, UpgradeButtons, upgrade.name + " (" + upgrade.Cost + ")");
                    upgradeButton.GetComponent<UIResearchButton>().upgrade = upgrade;
                }
            }

            foreach (BasicUnit structureTemplate in inspectedUnit.StructuresUnlocked())
            {
                GameObject structureButton = createAndParentButton(StructurePanel, StructureButtonTemplate, StructureButtons, structureTemplate.templateID + " (" + structureTemplate.ScaledGoldCost() + ")");
                structureButton.GetComponent<UIBuyStructure>().StructureTemplate = structureTemplate;
            }

            if (inspectedUnit.HasTag(BasicUnit.Tag.Structure) && inspectedUnit.AnotherStructureLevelExists())
            {
                createAndParentButton(LevelUpPanel, LevelUpButtonTemplate, LevelUpButtons,
                    "Level " + (inspectedUnit.Level + 1) + "(" + inspectedUnit.LevelUpCost() + ")");
            }

            if (inspectedUnit.HasTag(BasicUnit.Tag.Structure))
            {
                foreach (GameObject resident in inspectedUnit.AllSpawns)
                {
                    if (resident != null && resident.GetComponent<BasicUnit>().HasTag(BasicUnit.Tag.Hero))
                    {
                        GameObject residentButton = createAndParentButton(ResidentPanel, ResidentButtonTemplate, ResidentButtons, resident.name);
                        residentButton.GetComponent<UIResidentButton>().resident = resident.GetComponent<BasicUnit>();
                    }
                }
            }


        }
    }

    GameObject createAndParentButton(GameObject panel, GameObject buttonTemplate, List<Button> buttonCollection, string buttonText)
    {
        GameObject buttonObject = Instantiate(buttonTemplate);
        panel.SetActive(true);
        buttonObject.transform.SetParent(panel.transform);

        Button buttonData = buttonObject.GetComponent<Button>();
        buttonData.GetComponentInChildren<Text>().text = buttonText;
        buttonCollection.Add(buttonData);

        return buttonObject;
    }
}

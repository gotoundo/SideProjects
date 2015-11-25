using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using System;

public class UIInspectorPanel : MonoBehaviour {

    public static UIInspectorPanel Main;

    public Text StatusText;
    public GameObject HireButtonTemplate;
    public GameObject UpgradeButtonTemplate;
    public GameObject StructureButtonTemplate;
    public GameObject LevelUpButtonTemplate;
    public GameObject ResidentButtonTemplate;

    List<GameObject> Panels;
    public GameObject HirePanel;
    public GameObject UpgradePanel;
    public GameObject StructurePanel;
    public GameObject LevelUpPanel;
    public GameObject ResidentPanel;
    public GameObject DescriptionPanel;

    public List<List<Button>> ButtonCollections;
    public List<Button> HireButtons;
    public List<Button> UpgradeButtons;
    public List<Button> StructureButtons;
    public List<Button> LevelUpButtons;
    public List<Button> ResidentButtons;

    public Text HireTitle;
    public Text DescriptionHeader;
    public Text DescriptionBody;
    public Text DescriptionButtonText;


    
    

    void Awake()
    {
        Main = this;
        StatusText = StatusText ? StatusText : GetComponentInChildren<Text>();
        HireButtons = new List<Button>();
        UpgradeButtons = new List<Button>();
        StructureButtons = new List<Button>();
        LevelUpButtons = new List<Button>();
        ResidentButtons = new List<Button>();

        Panels = new List<GameObject>(new GameObject[] { HirePanel, UpgradePanel, StructurePanel,LevelUpPanel,ResidentPanel,DescriptionPanel });
        ButtonCollections = new List<List<Button>>(new List<Button>[] {HireButtons,UpgradeButtons,StructureButtons,LevelUpButtons,ResidentButtons });
    }


    // Use this for initialization
    void Start () {
        
	
	}

    bool descriptionMode = false;

    public bool ToggleDescriptionMode()
    {
        if (descriptionMode)
            ExitDescriptionMode();
        else
            EnterDescriptionMode();
        return descriptionMode;
    }

    public void EnterDescriptionMode()
    {

        StatusText.gameObject.SetActive(false);
        descriptionMode = true;
        foreach (GameObject panel in Panels)
            panel.SetActive(false);
        DescriptionPanel.SetActive(true);
        BasicUnit myUnit = GameManager.Main.InspectedUnit;
        DescriptionHeader.text = myUnit.templateID;
        DescriptionBody.text = myUnit.templateDescription.Replace("[n]", "\n");
        DescriptionButtonText.text = "X";
    }

    public void ExitDescriptionMode()
    {
        StatusText.gameObject.SetActive(true);
        descriptionMode = false;
        InspectNewObject();
        DescriptionButtonText.text = "?";
    }

    // Update is called once per frame
    void Update()
    {
        BasicUnit myUnit = GameManager.Main.InspectedUnit;
        if (myUnit != null)
        {
            if (!descriptionMode)
            {
                UpdateUnitStatus(myUnit);
            }
        }
        else
            GameManager.Main.EndInspection();
    }

    void UpdateUnitStatus(BasicUnit myUnit)
    {
        StatusText.gameObject.SetActive(true);
        StatusText.text = myUnit.name;

        if (myUnit.HasTag(BasicUnit.Tag.Hero) || myUnit.HasTag(BasicUnit.Tag.Structure))
        {
            StatusText.text += " (Lvl " + myUnit.Level + ")";
        }

        if (myUnit.HasTag(BasicUnit.Tag.Hero))
        {
            StatusText.text += "\n" + (int)myUnit.XP + " xp";
            StatusText.text += "\n" + myUnit.currentState.ToString();
        }
        StatusText.text += "\n" + Mathf.RoundToInt(myUnit.currentHealth) + "/" + myUnit.getMaxHP + " HP";

        if (myUnit.HasTag(BasicUnit.Tag.Hero) || myUnit.HasTag(BasicUnit.Tag.Structure))
            StatusText.text += "\n" + myUnit.Gold + " Gold";

        if (myUnit.HasTag(BasicUnit.Tag.Hero))
        {
            StatusText.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Strength) + " Strength";
            StatusText.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Dexterity) + " Dexterity";
            StatusText.text += "\n" + myUnit.GetStat(BasicUnit.Stat.Intelligence) + " Intelligence";

            for (int i = 0; i < myUnit.EquipmentSlots.Count; i++)
            {
                if (myUnit.EquipmentSlots[i].Instance != null)
                    StatusText.text += "\n Item: " + myUnit.EquipmentSlots[i].Instance.name;
            }
            StatusText.text += "\n Salves: " + myUnit.Potions.Count;
        }

        if (myUnit.maxHirelings > 0)
        {
            HireTitle.text = "Hire Units - " + myUnit.GetHiredHeroes().Count + "/" + myUnit.maxHirelings;
        }
    }


    public void ObjectUpdated()
    {
        if(!descriptionMode)
            InspectNewObject();
    }

    public void InspectNewObject()
    {
        descriptionMode = false;
        
        BasicUnit inspectedUnit = GameManager.Main.InspectedUnit;

        foreach (List<Button> buttonCollection in ButtonCollections)
        {
            foreach (Button button in buttonCollection)
                Destroy(button.gameObject);
            buttonCollection.Clear();
        }

        foreach (GameObject panel in Panels)
            panel.SetActive(false);

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

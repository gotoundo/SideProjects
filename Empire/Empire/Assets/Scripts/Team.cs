using UnityEngine;
using System.Collections.Generic;

public class Team : MonoBehaviour {
    public enum ID { Player, Computer}
    public ID identity;

    public int Gold;

    public Dictionary<BasicUnit,int> MaxBuildableUnits;
    protected Dictionary<string, List<BasicUnit>> UnitInstances;
    List<BasicUnit> currentUnits;
    public List<BasicUpgrade.ID> TeamUpgrades;
    public List<BasicUnit> bannedTemplates;

    public float researchTimeMultiplier = 1f;
    public float structureCostMultiplier = 1f;
    public float researchCostMultiplier = 1f;


    // Use this for initialization

    public void AddUnit(BasicUnit unit)
    {
        currentUnits.Add(unit);

        string templateID = unit.templateID;
        if (!UnitInstances.ContainsKey(templateID))
            UnitInstances.Add(templateID, new List<BasicUnit>());
        UnitInstances[templateID].Add(unit);
    }

    public bool HasUnit(BasicUnit unit)
    {
        return currentUnits.Contains(unit);
    }

    public void RemoveUnit(BasicUnit unit)
    {
        if (currentUnits.Contains(unit))
        {
            currentUnits.Remove(unit);

            string templateID = unit.templateID;
            if (UnitInstances.ContainsKey(templateID))
                UnitInstances[templateID].Remove(unit);
        }
    }

    public List<BasicUnit> GetInstances(string templateID)
    {
        if (!UnitInstances.ContainsKey(templateID))
            UnitInstances.Add(templateID, new List<BasicUnit>());
        return UnitInstances[templateID];

    }

    public List<BasicUnit> GetUnits()
    {
        List<BasicUnit> Units = new List<BasicUnit>(currentUnits);
        return Units;
    }

    public List<BasicUnit> BuildableStructureTemplates()
    {
        List<BasicUnit> buildableStructures = new List<BasicUnit>();
        foreach(BasicUnit unit in currentUnits)
        {
            foreach (BasicUnit newStructure in unit.StructuresUnlocked())
            {
                if (!buildableStructures.Contains(newStructure))
                    buildableStructures.Add(newStructure);
            }
        }
        return buildableStructures;
    }
    
    void Awake()
    {
        UnitInstances = new Dictionary<string, List<BasicUnit>>();
        currentUnits = new List<BasicUnit>();
        TeamUpgrades = TeamUpgrades ?? new List<BasicUpgrade.ID>();
        bannedTemplates = bannedTemplates ?? new List<BasicUnit>();
        MaxBuildableUnits = new Dictionary<BasicUnit, int>();
    }

    void Start() {    
        GameManager.Main.AllTeams.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
        while (currentUnits.Contains(null))
            currentUnits.Remove(null);
	}

    public bool CanAffordStructure(BasicUnit structureTemplate)
    {
        return Gold >= structureTemplate.ScaledGoldCost();
    }

    public bool AllowedToBuildUnit(BasicUnit unitTemplate)
    {
        if (unitTemplate.LevelUnlocks.Count == 0)
            return true;

        foreach(BasicUnit.LevelUnlock.BuildRequirement buildRequirement in unitTemplate.LevelUnlocks[0].StructureRequirements)
        {
            //Check Structure Requirements 
            if(buildRequirement.Structure != null)
            {
                string templateID = buildRequirement.Structure.templateID;

                //No Specified Structure Exists
                if (!UnitInstances.ContainsKey(templateID) || UnitInstances[templateID].Count == 0)
                {
                    if (buildRequirement.ShouldExist)
                        return false;
                }
                //Specified Structure Exists
                else
                {
                    List<BasicUnit> Instances = UnitInstances[templateID];
                    bool levelRequirementMet = false;
                    foreach (BasicUnit instance in Instances) //Make sure there exists an instance of the required minimum level
                    {
                        if (!buildRequirement.ShouldExist)
                            return false;

                        if (instance.Level >= buildRequirement.MinLevel)
                            levelRequirementMet = true;
                    }
                    if (!levelRequirementMet)
                        return false;
                }
            }
        }

        foreach (BasicUpgrade.ID upgrade in unitTemplate.LevelUnlocks[0].UpgradesRequired)
        {
            if (!unitTemplate.team.TeamUpgrades.Contains(upgrade))
                return false;
        }

        return true;
    }
    
    public void EnterPlaceStructureMode(BasicUnit StructureTemplate)
    {
        GameManager.Main.StartStructurePlacement(StructureTemplate);
    }

    public BasicUnit PlaceStructure(BasicUnit StructureTemplate, Vector3 position)
    {
        if (CanAffordStructure(StructureTemplate))
        {
            Gold -= StructureTemplate.ScaledGoldCost();

            GameObject NewStucture = (GameObject)Instantiate(StructureTemplate.gameObject, position, StructureTemplate.transform.rotation);
            BasicUnit NewStructureUnit = NewStucture.GetComponent<BasicUnit>();
            NewStructureUnit.StartBuildingConstruction();
            NewStructureUnit.team = this;
            return NewStructureUnit;
        }
        return null;
    }

    public bool CanAffordBounty()
    {
        return Gold >= GameManager.defaultBountyIncrement;
    }

   // public void EnterPlaceExploreBountyMode(BasicBounty BountyTemplate)
  //  {
    //    GameManager.Main.StartBountyPlacement(BountyTemplate);
    //}

    public void PlaceExploreBounty(BasicBounty BountyTemplate, Vector3 position)
    {
        if (CanAffordBounty())
        {
            GameObject NewBounty = (GameObject)Instantiate(BountyTemplate.gameObject, position, BountyTemplate.transform.rotation);
            NewBounty.GetComponent<BasicBounty>().initializeExploreBounty(GameManager.defaultBountyIncrement, position);
        }
    }

    public void PlaceKillBounty(BasicBounty BountyTemplate, BasicUnit target)
    {
        if (CanAffordBounty())
        {
            GameObject NewBounty = Instantiate(BountyTemplate.gameObject);
            NewBounty.GetComponent<BasicBounty>().initializeKillBounty(GameManager.defaultBountyIncrement, target);
        }
    }

    public void PlaceDefendBounty(BasicBounty BountyTemplate, BasicUnit target)
    {
        if (CanAffordBounty())
        {
            GameObject NewBounty = Instantiate(BountyTemplate.gameObject);
            NewBounty.GetComponent<BasicBounty>().initializeDefendBounty(GameManager.defaultBountyIncrement, target);
        }
    }



    /* public void ModifyMaxBuildableUnits(BasicUnit UnitTemplate, int amountDifference)
     {
         if (!MaxBuildableUnits.ContainsKey(UnitTemplate))
             MaxBuildableUnits.Add(UnitTemplate, 0);
         MaxBuildableUnits[UnitTemplate] += amountDifference;
     }

     public void ModifyCurrentUnits(BasicUnit Unit)
     {
         if (!CurrentUnits.Contains(Unit))
             CurrentUnits.Add(Unit);
     }*/


}

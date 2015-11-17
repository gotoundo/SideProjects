using UnityEngine;
using System.Collections.Generic;

public class Team : MonoBehaviour {
    public enum ID { Player, Computer}
    public ID identity;

    public int Gold;

    public Dictionary<BasicUnit,int> MaxBuildableUnits;
    List<BasicUnit> currentUnits;
    public List<BasicUpgrade.ID> TeamUpgrades;

    // Use this for initialization

    public void AddUnit(BasicUnit unit)
    {
        currentUnits.Add(unit);
        GameManager.Main.PossibleStructureAvailabilityChange();
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
            GameManager.Main.PossibleStructureAvailabilityChange();
        }
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
        currentUnits = new List<BasicUnit>();
        TeamUpgrades = TeamUpgrades ?? new List<BasicUpgrade.ID>();
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

    public bool CanAffordStructure(BasicUnit Structure)
    {
        return Gold >= Structure.GoldCost;
    }
    
    public void EnterPlaceStructureMode(BasicUnit StructureTemplate)
    {
        GameManager.Main.StartStructurePlacement(StructureTemplate);
    }

    public void PlaceStructure(BasicUnit StructureTemplate, Vector3 position)
    {
        if (CanAffordStructure(StructureTemplate))
        {
            GameObject NewStucture = (GameObject)Instantiate(StructureTemplate.gameObject, position, StructureTemplate.transform.rotation);
            NewStucture.GetComponent<BasicUnit>().team = this;
            Gold -= StructureTemplate.GoldCost;
            //ModifyMaxBuildableUnits(StructureTemplate, StructureTemplate.MaxSpawns);
        }
    }

    public bool CanAffordBounty()
    {
        return Gold >= GameManager.defaultBountyIncrement;
    }

    public void EnterPlaceExploreBountyMode(BasicBounty BountyTemplate)
    {
        GameManager.Main.StartBountyPlacement(BountyTemplate);
    }

    public void PlaceExploreBounty(BasicBounty BountyTemplate, Vector3 position)
    {
        if (CanAffordBounty())
        {
            GameObject NewBounty= (GameObject)Instantiate(BountyTemplate.gameObject, position, BountyTemplate.transform.rotation);
            NewBounty.GetComponent<BasicBounty>().initializeExploreBounty(GameManager.defaultBountyIncrement, position);
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

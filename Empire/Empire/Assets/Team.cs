using UnityEngine;
using System.Collections.Generic;

public class Team : MonoBehaviour {
    public enum ID { Player, Computer}
    public ID identity;

    public int Gold;

    public Dictionary<BasicUnit,int> MaxBuildableUnits;
    public Dictionary<BasicUnit, int> CurrentUnits;

    // Use this for initialization
    void Start() {
        MaxBuildableUnits = new Dictionary<BasicUnit, int>();
        CurrentUnits = new Dictionary<BasicUnit, int>();
        GameManager.Main.AllTeams.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
	
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
            ModifyMaxBuildableUnits(StructureTemplate, StructureTemplate.MaxSpawns);
        }
    }
    
    

    public void ModifyMaxBuildableUnits(BasicUnit UnitTemplate, int amountDifference)
    {
        if (!MaxBuildableUnits.ContainsKey(UnitTemplate))
            MaxBuildableUnits.Add(UnitTemplate, 0);
        MaxBuildableUnits[UnitTemplate] += amountDifference;
    }
    public void ModifyCurrentUnits(BasicUnit UnitTemplate, int amountDifference)
    {
        if (!CurrentUnits.ContainsKey(UnitTemplate))
            CurrentUnits.Add(UnitTemplate, 0);
        CurrentUnits[UnitTemplate] += amountDifference;
    }


}

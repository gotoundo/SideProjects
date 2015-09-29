using UnityEngine;
using System.Collections;

public class UpgradeSlot : MonoBehaviour {

    public void AddExploreUpgrade(int droneID)
    {
        GameEngine.singleton.PlayerQueen.GetComponent<BQueen>().SpawnType[droneID].AddComponent<BExplore>();
    }
    public void AddAttackUpgrade(int droneID)
    {
        GameEngine.singleton.PlayerQueen.GetComponent<BQueen>().SpawnType[droneID].AddComponent<BAttackEnemies>();
    }
    public void AddHarvestUpgrade(int droneID)
    {
        GameEngine.singleton.PlayerQueen.GetComponent<BQueen>().SpawnType[droneID].AddComponent<BHarvestHoney>();
    }

    // Use this for initialization
    void Start () {
        
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

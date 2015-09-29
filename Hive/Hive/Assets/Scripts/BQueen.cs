using UnityEngine;
using System.Collections;

public class BQueen : UpgradeBase {

    // public GameObject SpawnTypeBase;\
    public GameObject BaseSpawnType;
    public GameObject[] SpawnType;
    public bool playerQueen;

    public float Cooldown;
    float currentCooldown;


    //  currentCooldown = Cooldown;


    /* for (int i = 0; i < SpawnType.Length; i++)
     {
         SpawnType[i] = Instantiate(SpawnTypeBase);
         SpawnType[i].SetActive(false);
     }*/

    // Update is called once per frame


    bool hasSetup = false;

    void Update ()
    {
        if (!hasSetup)
            DoSetup();


        if (myDrone.myQueen == null)
            myDrone.myQueen = gameObject;

        if (!myDrone.Dead)
            QueenLogic();
    }
    int currentSpawn = 0;


    void DoSetup()
    {
        hasSetup = true;
        if (playerQueen)
            for (int i = 0; i < SpawnType.Length; i++)
            {
                SpawnType[i] = Instantiate(BaseSpawnType);
                SpawnType[i].SetActive(false);
            }

    }

    public void AddUpgrade(int droneSlot, System.Type upgradeType)
    {
        SpawnType[droneSlot].AddComponent(upgradeType);
    }


    private void QueenLogic()
    {
        currentCooldown -= Time.deltaTime;

        if (myDrone.HoneyHeld >= SpawnType[currentSpawn].GetComponent<DroneBase>().SpawnCost && currentCooldown <= 0)
        {
            myDrone.HoneyHeld -= SpawnType[currentSpawn].GetComponent<DroneBase>().SpawnCost;
            currentCooldown = Cooldown;

            GameObject drone = (GameObject)Instantiate(SpawnType[currentSpawn], transform.position, transform.rotation);
            drone.SetActive(true);
            drone.tag = gameObject.tag;
            drone.GetComponent<MeshRenderer>().material.color = GetComponent<MeshRenderer>().material.color;

            DroneBase droneBase = drone.GetComponent<DroneBase>();
            droneBase.myQueen = gameObject;
            droneBase.EnemyTag = myDrone.EnemyTag;

           /* if(currentSpawn == 0)
                drone.AddComponent<BAttackEnemies>();
            if (currentSpawn == 1)
                drone.AddComponent<BHarvestHoney>();
            if (currentSpawn == 2)
                drone.AddComponent<BExplore>();
            if (currentSpawn == 3)
                drone.AddComponent<BHarvestHoney>();
                */


            currentSpawn++;
            if (currentSpawn >= SpawnType.Length)
                currentSpawn = 0;
        }

       

    }
}

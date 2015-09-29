using UnityEngine;
using System.Collections;

public class BHarvestHoney : UpgradeBase
{

    // Update is called once per frame
    void Update()
    {
        if(!myDrone.Dead)
            HarvestHoneyLogic();
    }

    private void HarvestHoneyLogic()
    {
        if (myDrone.HoneyHeld >= myDrone.HoneyCapacity)
            myDrone.returningHoney = true;
        else if (myDrone.HoneyHeld == 0)
            myDrone.returningHoney = false;

        if (myDrone.returningHoney)
        {
            myDrone.target = myDrone.myQueen.transform.position;
            myDrone.agent.stoppingDistance = myDrone.HarvestRange / 2;
            if (myDrone.targetWithinHarvestRange())
                depositHoney();
        }
        else
        {
            GameObject[] possibleTargets = myDrone.AcquireTargets("Honey");
            GameObject favoriteCandidate = null;
            float favoriteCandidateScore = 0;
            foreach (GameObject currentCandidate in possibleTargets)
            {
                DroneBase pot = currentCandidate.GetComponent<DroneBase>();
                if (pot == null)
                    Debug.Log("fuckup "+gameObject.name);
                float currentCandidateScore = Mathf.Min(pot.HoneyHeld,myDrone.HoneyCapacity*5) / Vector3.Distance(transform.position, currentCandidate.transform.position);

                if (favoriteCandidate == null || currentCandidateScore > favoriteCandidateScore)
                {
                    favoriteCandidate = currentCandidate;
                    favoriteCandidateScore = currentCandidateScore;
                }
            }

            if (favoriteCandidate != null)
            {
                myDrone.target = favoriteCandidate.transform.position;
                if (myDrone.targetWithinHarvestRange())
                    harvestHoney(favoriteCandidate);
            }
        }
    }


    private void harvestHoney(GameObject honeyPot)
    {
        DroneBase pot = honeyPot.GetComponent<DroneBase>();
        float honeyTransfered = Mathf.Min(myDrone.HoneyHarvestRate * Time.deltaTime, pot.HoneyHeld, myDrone.HoneyCapacity - myDrone.HoneyHeld); //transfer the smallest of Normal Harvest Rate, the Max remaining honey, and my remaining honey capacity
        pot.HoneyHeld -= honeyTransfered;
        myDrone.HoneyHeld += honeyTransfered;
    }

    private void depositHoney()
    {
        float honeyTransfered = Mathf.Min(myDrone.HoneyHarvestRate * Time.deltaTime, myDrone.HoneyHeld);
        myDrone.HoneyHeld -= honeyTransfered;
        myDrone.myQueen.GetComponent<DroneBase>().HoneyHeld += honeyTransfered;
    }
}

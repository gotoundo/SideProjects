using UnityEngine;
using System.Collections;

public class BAttackEnemies : UpgradeBase {
    

    // Update is called once per frame
    void Update ()
    {
        if (!myDrone.Dead)
            AttackEnemiesLogic();
    }

    private void AttackEnemiesLogic()
    {
        GameObject[] possibleTargets = myDrone.AcquireTargets(myDrone.EnemyTag); //GameObject.FindGameObjectsWithTag();
        GameObject favoriteCandidate = null;
        float favoriteCandidateScore = 0;
        foreach (GameObject currentCandidate in possibleTargets)
        {
            DroneBase enemy = currentCandidate.GetComponent<DroneBase>();
            if (enemy == null)
                Debug.Log("what");
            else if (!enemy.Dead)
            {
                float currentCandidateScore = 1 / (enemy.CurrentHealth + Vector3.Distance(transform.position, currentCandidate.transform.position));
                if (favoriteCandidate == null || currentCandidateScore > favoriteCandidateScore)
                {
                    favoriteCandidate = currentCandidate;
                    favoriteCandidateScore = currentCandidateScore;
                }
            }
        }

        if (favoriteCandidate != null && !favoriteCandidate.GetComponent<DroneBase>().Dead)
        {
            myDrone.target = favoriteCandidate.transform.position;
            myDrone.agent.stoppingDistance = myDrone.AttackRange / 2;
            if (myDrone.targetWithinAttackRange())
            {
                DroneBase enemy = favoriteCandidate.GetComponent<DroneBase>();
                enemy.TakeDamage(myDrone.AttackRate * Time.deltaTime);
            }
        }
    }
}

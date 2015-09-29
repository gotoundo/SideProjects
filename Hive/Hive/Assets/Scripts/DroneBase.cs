using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneBase : MonoBehaviour
{
    public static float droneCount = 0;

    public GameObject myQueen;
    public Vector3 target;
    public NavMeshAgent agent;
    public string EnemyTag;

    public float SpawnCost = 10f;

    //Honey
    public float HoneyHeld;
    public float HoneyCapacity;
    public float HoneyRegenRate;
    public float HoneyHarvestRate; //per second
    public bool returningHoney = false;
    public float HarvestRange = 2f;
    public float honeyRemainingAfterDeath = 0.5f;
    
    //Combat
    public float MaxHealth;
    public float CurrentHealth;
    public bool Dead = false;
    public float AttackRate = 1f; //DPS
    public float AttackRange = 2f;

    //Exploration
    public float searchRadius = 3f;
    public float senseRange = 6f;
    public bool marked = false;
    float markedSenseBoost = 10f;

    // Use this for initialization
    void Start()
    {
        droneCount++;
        name = tag + " " + droneCount;
        CurrentHealth = Random.Range(5, MaxHealth);
        agent = GetComponent<NavMeshAgent>();

    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    void LateUpdate()
    {
        if (!Dead)
        {

            HoneyHeld = Mathf.Min(HoneyHeld + HoneyRegenRate * Time.deltaTime, HoneyCapacity);

            if (target != Vector3.zero)
                agent.SetDestination(target);
            else
                agent.SetDestination(transform.position);
        }
        else
        {
            MeshRenderer mrenderer = GetComponent<MeshRenderer>();
            mrenderer.material.color = new Color(1,1,1);
        }

       // target = myQueen.transform.position;
       // target = Vector3.zero;
    }

    public GameObject[] AcquireTargets(string targetTag)
    {
        GameObject[] baseArray = GameObject.FindGameObjectsWithTag(targetTag);
        List<GameObject> finalList = new List<GameObject>();
        foreach(GameObject possibleTarget in baseArray)
        {
            float rangeMultiplier = 1;
            if (possibleTarget.GetComponent<DroneBase>() == null)
                Debug.Log("wtf " + possibleTarget + " doesn't have DroneBase");
            else
            {
                if (possibleTarget.GetComponent<DroneBase>().marked)
                    rangeMultiplier = markedSenseBoost;
                if (Vector3.Distance(transform.position, possibleTarget.transform.position) <= senseRange * rangeMultiplier)
                    finalList.Add(possibleTarget);
            }
        }

        return finalList.ToArray();

    }

    public bool targetWithinHarvestRange()
    {
        return Vector3.Distance(transform.position, target) < HarvestRange;
    }

    public bool targetWithinAttackRange()
    {
        return Vector3.Distance(transform.position, target) < AttackRange;
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Dead = true;
            HoneyHeld += SpawnCost * honeyRemainingAfterDeath;
            tag = "Honey";
            agent.Stop();
        }
    }

}

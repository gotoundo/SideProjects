using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileScript : MonoBehaviour {
    public Color Team;
    public int Sociability;
    int MaxSociability = 10;
    public float Health;
    public float MaxHealth = 10;
    public float InfluenceCooldown;
    float cooldownRemaining;
    const float tileDistance = 1f;
    const float transferSpeed = .01f;
    public GameObject[] Neighbors;

    static bool isFirst = true;
    public bool printLog;
    static int count;

    public static float teamDecider = 1;
	// Use this for initialization
	void Start () {
        Team = Random.Range(0f, 2f) > teamDecider ? Color.red : Color.blue;
        if (Team.Equals(Color.red))
            teamDecider += 1f;
        else
            teamDecider -= 1f;

        Sociability = Random.Range(1, 10);

        name = "Tile " + count;
        count++;
      //  printLog = isFirst;
        isFirst = false;

        	
	}
	
	// Update is called once per frame
	void Update () {
        cooldownRemaining -= Time.deltaTime;
        if (cooldownRemaining <= 0)
        {
            cooldownRemaining = InfluenceCooldown;

            List<GameObject> NeighborList = new List<GameObject>();
            GameObject[] baseArray = GameObject.FindGameObjectsWithTag("Tile");
            foreach(GameObject o in baseArray)
            {
                if (Vector3.Distance(o.transform.position, transform.position) <= tileDistance && !o.Equals(gameObject))
                {
                    NeighborList.Add(o);
                    TileScript otherTile = o.GetComponent<TileScript>();
                    otherTile.becomeInfluenced(Team, Sociability, Health / MaxHealth);
                }
            }
            Neighbors = NeighborList.ToArray();
        }
    }

    public void becomeInfluenced(Color otherTeam, int otherSociability, float otherHealthPercent)
    {
        float affectation = Mathf.Max(1, otherSociability - (MaxSociability - Sociability));
        affectation *= transferSpeed * otherHealthPercent;

        if (otherTeam.Equals(Team))
            Health += affectation;
        else
            Health -= affectation;

        if(printLog)
        {
            Debug.Log("Becoming " + affectation + " " + (otherTeam.Equals(Color.blue) ? "Blue" : "Red"));

        }
    }
    
    void LateUpdate()
    {
        if (Health < 0)
        {
            Health = 1;
            if (Team.Equals(Color.blue))
                Team = Color.red;
            else
                Team = Color.blue;
        }
        else
            Health = Mathf.Min(Health, MaxHealth);

        MeshRenderer mrenderer = GetComponent<MeshRenderer>();
        mrenderer.material.color = new Color(Team.r, Team.g, Team.b, Health / MaxHealth);
    }
}

using UnityEngine;
using System.Collections;

public class GridSwap : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    public float SwapCooldown = 1;
    float remainingCooldown;
    void Update()
    {

        remainingCooldown -= Time.deltaTime;
        if (remainingCooldown <= 0)
        {
            remainingCooldown = SwapCooldown;
            GameObject[] baseArray = GameObject.FindGameObjectsWithTag("Tile");
            GameObject tile1 = baseArray[Random.Range(0, baseArray.Length)];
            GameObject tile2 = baseArray[Random.Range(0, baseArray.Length)];

            Vector3 tile2InitialPosition = tile2.transform.position;

            tile2.transform.position = tile1.transform.position;
            tile1.transform.position = tile2InitialPosition;
        }
    }
}

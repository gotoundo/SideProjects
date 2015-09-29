using UnityEngine;
using System.Collections;

public class BExplore : UpgradeBase
{
    

	
	// Update is called once per frame
	void Update ()
    {
        if (myDrone == null)
            Debug.Log(name + " has no drone logic");
        if(!myDrone.Dead)
            ExploreLogic();
    }

    private void ExploreLogic()
    {
        if (myDrone.target.Equals(Vector3.zero))
            SetNewDestination();

        foreach (GameObject targetToMark in myDrone.AcquireTargets(myDrone.EnemyTag))
            MarkTarget(targetToMark);
        foreach (GameObject targetToMark in myDrone.AcquireTargets("Honey"))
            MarkTarget(targetToMark);

        if (Vector3.Distance(transform.position, myDrone.target) < 3f)
        {
            SetNewDestination();
        }
        myDrone.agent.SetDestination(myDrone.target);
    }

    void MarkTarget(GameObject drone)
    {
        DroneBase droneBase = drone.GetComponent<DroneBase>();
        if(!droneBase.marked)
        {
            GameObject o = (GameObject)Instantiate(GameEngine.singleton.Marker, drone.transform.position, drone.transform.rotation);
            o.transform.SetParent(drone.transform);
            MeshRenderer mrenderer = o.GetComponent<MeshRenderer>();
            mrenderer.material.color = GetComponent<MeshRenderer>().material.color;
            droneBase.marked = true;
        }
    }

    void SetNewDestination()
    {
        myDrone.target = myDrone.myQueen.transform.position + (new Vector3(Random.Range(-1f, 1f),0, Random.Range(-1f, 1f)).normalized * myDrone.searchRadius);
        Vector3 bounds = GameEngine.singleton.PlayableArea.transform.localScale * 10;

        float finalX = myDrone.target.x;
        float finalZ = myDrone.target.z;

        if (Mathf.Abs(finalX) > bounds.x)
            finalX = bounds.x * finalX / Mathf.Abs(finalX);

        if (Mathf.Abs(finalZ) > bounds.z)
            finalZ = bounds.y * finalZ / Mathf.Abs(finalZ);

        myDrone.target = new Vector3(finalX, 0, finalZ);



    }
}

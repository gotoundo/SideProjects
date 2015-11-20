using UnityEngine;
using System.Collections.Generic;

public class UIPlacementModel : MonoBehaviour {

    public enum PlacementMode { Structure, ExploreBounty};

    public PlacementMode currentMode;
    BasicUnit unitTemplate;
    BasicBounty bountyTemplate;
    public List<Collider> collisions;
    
    public bool Blocked
    {
        get { return collisions.Count > 0; }
    }

    public void SetStructureTemplate(BasicUnit unitTemplate)
    {
        currentMode = PlacementMode.Structure;
        this.unitTemplate = unitTemplate;
        assumeDimentions(unitTemplate.gameObject);

    }

    public void SetBountyTemplate(BasicBounty bountyTemplate)
    {
        currentMode = PlacementMode.ExploreBounty;
        this.bountyTemplate = bountyTemplate;
        assumeDimentions(bountyTemplate.gameObject);
    }

    void assumeDimentions(GameObject template)
    {
        collisions = new List<Collider>();
        transform.localScale = template.transform.localScale;
        transform.rotation = template.transform.rotation;
        GetComponent<MeshFilter>().mesh = template.GetComponent<MeshFilter>().sharedMesh;
        assignColor();
    }



	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update() {
        assignColor();
    }

    void assignColor()
    {
        if (unitTemplate != null)
        {
            if (Blocked)
                GetComponent<MeshRenderer>().material.color = Color.red;
            else
                GetComponent<MeshRenderer>().material = unitTemplate.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision!");
        if (isBlockingObject(other) && !collisions.Contains(other))
            collisions.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (isBlockingObject(other))
            collisions.Remove(other);
    }

    bool isBlockingObject(Collider collider)
    {
     //   return true;
        return collider.gameObject.GetComponent<BasicUnit>() != null;
    }


}

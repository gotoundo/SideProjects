using UnityEngine;
using System.Collections.Generic;

public class UIPlacementModel : MonoBehaviour {

    BasicUnit unitTemplate;
    public List<Collider> collisions;

    //MeshFilter meshFilter;
    //MeshRenderer meshRenderer;
    
    public bool Blocked
    {
        get { return collisions.Count > 0; }
    }

    public void SetTemplate(BasicUnit unitTemplate)
    {
        collisions = new List<Collider>();
        this.unitTemplate = unitTemplate;
        transform.localScale = unitTemplate.gameObject.transform.localScale;

        GetComponent<MeshFilter>().mesh = unitTemplate.gameObject.GetComponent<MeshFilter>().sharedMesh;
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

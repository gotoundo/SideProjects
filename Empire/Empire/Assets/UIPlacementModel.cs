﻿using UnityEngine;
using System.Collections.Generic;

public class UIPlacementModel : MonoBehaviour {

    public enum PlacementMode { Structure, ExploreBounty};

    public PlacementMode currentMode;
    BasicUnit unitTemplate;
    BasicBounty bountyTemplate;
    public List<Collider> collisions;
    GameObject TempModel;
    
    public bool Blocked
    {
        get { return collisions.Count > 0; }
    }

    public void SetStructureTemplate(BasicUnit unitTemplate)
    {
        currentMode = PlacementMode.Structure;
        this.unitTemplate = unitTemplate;
        assumeDimentions(unitTemplate.StructureVisual);
        assignColor();
    }

    public void SetBountyTemplate(BasicBounty bountyTemplate)
    {
        currentMode = PlacementMode.ExploreBounty;
        this.bountyTemplate = bountyTemplate;
        assumeDimentions(bountyTemplate.gameObject);
    }

    void assumeDimentions(GameObject template)
    {

        if (TempModel != null)
            Destroy(TempModel);

        collisions = new List<Collider>();

        TempModel = Instantiate(template);
        TempModel.transform.SetParent(transform);
        TempModel.transform.localPosition = Vector3.zero;

        //transform.localScale = template.transform.localScale;
        //transform.rotation = template.transform.rotation;
        //GetComponent<MeshFilter>().mesh = template.GetComponent<MeshFilter>().sharedMesh;
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
        //if (unitTemplate != null)
        //{
            MeshRenderer[] Renderers = TempModel.GetComponentsInChildren<MeshRenderer>();
            if (Blocked)
            {
                foreach (MeshRenderer renderer in Renderers)
                    renderer.material.color = Color.red;
                //TempModel.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                foreach (MeshRenderer renderer in Renderers)
                    renderer.material.color = Color.green;
            }
                //TempModel.GetComponent<MeshRenderer>().material = unitTemplate.StructureVisual.GetComponent<MeshRenderer>().sharedMaterial;
       // }

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

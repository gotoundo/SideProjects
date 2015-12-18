using UnityEngine;
using System.Collections.Generic;

public class UIPlacementModel : MonoBehaviour {

    public enum PlacementMode { Structure, ExploreBounty};

    public PlacementMode currentMode;
    BasicUnit unitTemplate;
    BasicBounty bountyTemplate;
    public List<Collider> collisions;
    GameObject TempModel;

    static UIBountyIcon bountyIcon;


    public void Finish()
    {
        if (bountyIcon)
            Destroy(bountyIcon.gameObject);

        gameObject.SetActive(false);
    }
    
    public bool Blocked
    {
        get { return collisions.Count > 0; }
    }

    public void SetStructureTemplate(BasicUnit unitTemplate)
    {
        currentMode = PlacementMode.Structure;
        this.unitTemplate = unitTemplate;
        assumeDimentions(unitTemplate.StructureVisual);

        BoxCollider myCollider = GetComponent<BoxCollider>();
        BoxCollider newCollider = unitTemplate.gameObject.GetComponent<BoxCollider>();
        myCollider.center = newCollider.center;
        myCollider.size = newCollider.size + new Vector3(3,3,3);

        assignColor();
    }

    public void SetBountyTemplate(BasicBounty bountyTemplate)
    {
        if (bountyIcon)
            Destroy(bountyIcon.gameObject);

        currentMode = PlacementMode.ExploreBounty;
        this.bountyTemplate = bountyTemplate;
        assumeDimentions(bountyTemplate.gameObject);
        bountyIcon = Instantiate(bountyTemplate.iconTemplate.gameObject).GetComponent<UIBountyIcon>();
        bountyIcon.Follow(gameObject);
    }

    void assumeDimentions(GameObject template)
    {

        if (TempModel != null)
            Destroy(TempModel);

        collisions = new List<Collider>();

        TempModel = (GameObject)Instantiate(template, transform.position, template.transform.rotation);
        TempModel.transform.SetParent(transform);

        //BoxCollider bCollider = TempModel.GetComponentInChildren<BoxCollider>();
       // bCollider.size = bCollider.size + new Vector3(5, 5, 5);
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
        MeshRenderer[] Renderers = TempModel.GetComponentsInChildren<MeshRenderer>();

        if (Blocked)
        {
            foreach (MeshRenderer renderer in Renderers)
                renderer.material.color = Color.red;
        }
        else
        {
            foreach (MeshRenderer renderer in Renderers)
                renderer.material.color = Color.green;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision!");
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

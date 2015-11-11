using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    public static GameManager Main;
    new public Camera camera;
   // public float Date;
    public Team Player;
    public List<Team> AllTeams;
    public List<BasicBounty> AllBounties;
    public GameObject BountyTemplate;
    public bool Running = false;
    public bool PlacementMode { get { return PlacementModel.activeInHierarchy; } }

    public float playTime;

    public const int defaultBountyIncrement = 100;

    //public GameObject PlacementCursor;
    public GameObject PlacementModel;
    private GameObject PlacementTemplate;
    public GameObject InspectorPanel;
    public BasicUnit InspectedUnit;

    public Vector3 MapBounds;

    

    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
        AllBounties = new List<BasicBounty>();
        MapBounds = new Vector3(200, 0, 200);
    }

	// Use this for initialization
	void Start () {
        Running = true;
        PlacementModel.SetActive(false);
        InspectorPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        //CheckInspectionState();
        StructurePlacementHandler();
        playTime += Time.deltaTime;

    }

    // STRUCTURE PLACEMENT LOGIC

    void StructurePlacementHandler()
    {
        if (PlacementModel.activeInHierarchy)
        {
            EndInspection(); //don't want two modes at once
            Vector3 clickLocation = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 placementPosition = camera.ScreenToWorldPoint(Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                clickLocation = hit.point;
                placementPosition = hit.point;
            }

            placementPosition = new Vector3(placementPosition.x, 1, placementPosition.z);

            if (Input.GetMouseButton(0) && Vector3.Distance(placementPosition, PlacementModel.transform.position) > 1f)
            {
                    PlacementModel.transform.position = placementPosition;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (!PlacementModel.GetComponent<UIPlacementModel>().Blocked && Vector3.Distance(placementPosition, PlacementModel.transform.position) < 1f)
                {//location confirmed, place structure
                    if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.Structure)
                    {
                        Player.PlaceStructure(PlacementTemplate.GetComponent<BasicUnit>(), placementPosition);
                    }
                    else if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.ExploreBounty)
                    {
                        Player.PlaceExploreBounty(PlacementTemplate.GetComponent<BasicBounty>(), placementPosition);
                    }

                    EndPlacement();
                    return;
                }
            }

           
        }
    }

    public void StartStructurePlacement(BasicUnit structure)
    {
        PlacementModel.SetActive(true);
        PlacementModel.GetComponent<UIPlacementModel>().SetStructureTemplate(structure);
        StartPlacement(structure.gameObject);
    }

    public void StartBountyPlacement(BasicBounty bounty)
    {
        PlacementModel.SetActive(true);
        PlacementModel.GetComponent<UIPlacementModel>().SetBountyTemplate(bounty);
        StartPlacement(bounty.gameObject);
    }

    void StartPlacement(GameObject template)
    {
        PlacementModel.transform.position = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        PlacementModel.transform.position = new Vector3(PlacementModel.transform.position.x, 1f, PlacementModel.transform.position.z);
        PlacementTemplate = template;
    }

    public void EndPlacement()
    {
        PlacementModel.SetActive(false);
        PlacementTemplate = null;
    }


    // INSPECTOR WINDOW LOGIC

    public void StartInspection(BasicUnit unitToInspect)
    {
        InspectorPanel.SetActive(true);
        InspectedUnit = unitToInspect;
        InspectorPanel.GetComponent<UIInspectorPanel>().InspectNewObject();
    }

    public void EndInspection()
    {
        InspectorPanel.SetActive(false);
        InspectedUnit = null;
    }
}

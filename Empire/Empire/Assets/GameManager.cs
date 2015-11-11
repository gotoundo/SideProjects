using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    public static GameManager Main;
    new public Camera camera;
    public float Date;
    public Team Player;
    public List<Team> AllTeams;
    public bool Running = false;
    public bool PlacementMode { get { return PlacementModel.activeInHierarchy; } }

    public float playTime;

    //public GameObject PlacementCursor;
    public GameObject PlacementModel;
    private BasicUnit PlacementTemplate;
    public GameObject InspectorPanel;
    public BasicUnit InspectedUnit;

    public Vector3 MapBounds;

    

    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
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

            /*
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
              RaycastHit hit;
              if (Physics.Raycast(ray,out hit))
              {
                Instantiate(initiateGO,hit.point,Quaternion.identity);
                goReady = false;
              }



    */

            placementPosition = new Vector3(placementPosition.x, 1, placementPosition.z);

            if (Input.GetMouseButton(0) && Vector3.Distance(placementPosition, PlacementModel.transform.position) > 1f)
            {
                    PlacementModel.transform.position = placementPosition;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (!PlacementModel.GetComponent<UIPlacementModel>().Blocked && Vector3.Distance(placementPosition, PlacementModel.transform.position) < 1f)
                {//location confirmed, place structure
                    Player.PlaceStructure(PlacementTemplate, placementPosition);
                    EndStructurePlacement();
                    return;
                }
            }

           
        }
    }

    public void StartStructurePlacement(BasicUnit structure)
    {
        PlacementModel.SetActive(true);
        PlacementModel.GetComponent<UIPlacementModel>().SetTemplate(structure);
        PlacementModel.transform.position = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        PlacementModel.transform.position = new Vector3(PlacementModel.transform.position.x, 1f, PlacementModel.transform.position.z);
        PlacementTemplate = structure;
        //PlacementModel.GetComponent<MeshFilter>().mesh = structure.gameObject.GetComponent<MeshFilter>().sharedMesh;
        //PlacementModel.GetComponent<MeshRenderer>().material = structure.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        //PlacementModel.transform.localScale = structure.gameObject.transform.localScale;
        //PlacementTemplate = structure;
    }

    public void EndStructurePlacement()
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

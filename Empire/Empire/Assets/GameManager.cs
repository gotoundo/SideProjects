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

    //public GameObject PlacementCursor;
    public GameObject PlacementModel;
    public BasicUnit PlacementTemplate;
    public GameObject InspectorPanel;
    public BasicUnit InspectedUnit;

    public Vector3 MapBounds;

    

    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
        InspectorPanel = GameObject.FindGameObjectWithTag("InspectorWindow");
        MapBounds = new Vector3(100, 0, 100);

        PlacementModel.SetActive(false);
        InspectorPanel.SetActive(false);
    }

	// Use this for initialization
	void Start () {
        Running = true;
	}
	
	// Update is called once per frame
	void Update () {
        //CheckInspectionState();
        StructurePlacementHandler();

	}

    // STRUCTURE PLACEMENT LOGIC

    void StructurePlacementHandler()
    {
        if (PlacementModel.activeInHierarchy)
        {
            EndInspection(); //don't want two modes at once
            Vector3 clickLocation = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 placementPosition = camera.ScreenToWorldPoint(Input.mousePosition);
            
            placementPosition = new Vector3(placementPosition.x, 1, placementPosition.z);

            if (Input.GetMouseButton(0) && Vector3.Distance(placementPosition, PlacementModel.transform.position) > 1f)
            {
                    PlacementModel.transform.position = placementPosition;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (Vector3.Distance(placementPosition, PlacementModel.transform.position) < 1f)
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
        PlacementModel.transform.position = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        PlacementModel.transform.position = new Vector3(PlacementModel.transform.position.x, 1f, PlacementModel.transform.position.z);
        PlacementModel.GetComponent<MeshFilter>().mesh = structure.gameObject.GetComponent<MeshFilter>().sharedMesh;
        PlacementModel.GetComponent<MeshRenderer>().material = structure.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
        PlacementModel.transform.localScale = structure.gameObject.transform.localScale;
        PlacementTemplate = structure;
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

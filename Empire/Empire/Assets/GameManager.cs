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

    BasicUnit nextInspectionTarget;

    public float playTime;

    public const int defaultBountyIncrement = 100;

    //public GameObject PlacementCursor;
    public GameObject PlacementModel;
    private GameObject PlacementTemplate;
    public GameObject InspectorPanel;
    public BasicUnit InspectedUnit;
    //public GameObject BuildingsPanel;
    public GameObject HealthBarTemplate;
    //public Canvas MainCanvas;
    public GameObject HealthBarFolder;

    public Vector3 MapBounds;

    Vector3 groundCamOffset;
    Vector3 camTarget;
    Vector3 camSmoothDampV;

    private Vector3 GetWorldPosAtViewportPoint(float vx, float vy)
    {
        Ray worldRay = camera.ViewportPointToRay(new Vector3(vx, vy, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distanceToGround;
        groundPlane.Raycast(worldRay, out distanceToGround);
        Debug.Log("distance to ground:" + distanceToGround);
        return worldRay.GetPoint(distanceToGround);
    }

    void cameraTrackingSetup()
    {
        Vector3 groundPos = GetWorldPosAtViewportPoint(0.5f, 0.5f);
        Debug.Log("groundPos: " + groundPos);
        groundCamOffset = camera.transform.position - groundPos;
        camTarget = camera.transform.position;
    }

    void cameraTrackingClickTarget()
    {
        float mouseX = Input.mousePosition.x / camera.pixelWidth;
        float mouseY = Input.mousePosition.y / camera.pixelHeight;
        Vector3 clickPt = GetWorldPosAtViewportPoint(mouseX, mouseY);
        camTarget = clickPt + groundCamOffset;
    }

    void cameraTrackingUnitTarget(BasicUnit unit)
    {
        camTarget = unit.transform.position + groundCamOffset;
    }

    void cameraTrackingUpdate()
    {
        camera.transform.position = Vector3.SmoothDamp(
            camera.transform.position, camTarget, ref camSmoothDampV, 0.5f);
    }

    void cameraTrackingSnapToTarget()
    {
        camera.transform.position = camTarget;
    }



    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
        AllBounties = new List<BasicBounty>();
        MapBounds = new Vector3(400, 0, 400);
    }

	// Use this for initialization
	void Start () {
        Running = true;
        PlacementModel.SetActive(false);
        InspectorPanel.SetActive(false);
        cameraTrackingSetup();

        GameObject castle = GameObject.FindGameObjectWithTag("Castle");
        cameraTrackingUnitTarget(castle.GetComponent<BasicUnit>());
        cameraTrackingSnapToTarget();
        StartInspection(castle.GetComponent<BasicUnit>());
    }

    // Update is called once per frame
    bool MenuUpdate = false;
    void Update () {
        //CheckInspectionState();
        StructurePlacementHandler();
        playTime += Time.deltaTime;
        if(MenuUpdate)
        {
            RefreshMenu();
            MenuUpdate = false;
        }

        if(nextInspectionTarget != null)
        {
            StartInspection(nextInspectionTarget);
            nextInspectionTarget = null;
        }

    }

    public void CenterCameraOnUnit(BasicUnit unit, bool track)
    {
        cameraTrackingUnitTarget(unit);
        cameraTrackingSnapToTarget();
    }

    // STRUCTURE PLACEMENT LOGIC

    void StructurePlacementHandler()
    {
        if (PlacementModel.activeInHierarchy)
        {
            //EndInspection(); //don't want two modes at once
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

                    BasicUnit placedUnit = null;
                    if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.Structure)
                    {
                        placedUnit = Player.PlaceStructure(PlacementTemplate.GetComponent<BasicUnit>(), placementPosition);
                    }
                    else if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.ExploreBounty)
                    {
                        Player.PlaceExploreBounty(PlacementTemplate.GetComponent<BasicBounty>(), placementPosition);
                    }

                    EndPlacement();
                    if (placedUnit != null)
                        nextInspectionTarget = placedUnit;
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
        MenuAction();
    }


    

    // INSPECTOR WINDOW LOGIC

    public void StartInspection(BasicUnit unitToInspect)
    {
        InspectorPanel.SetActive(true);
        InspectedUnit = unitToInspect;
        InspectorPanel.GetComponent<UIInspectorPanel>().InspectNewObject();
    }

    public void PossibleOptionsChange(BasicUnit updatedUnit)
    {
        if(updatedUnit == InspectedUnit)
            InspectorPanel.GetComponent<UIInspectorPanel>().ObjectUpdated();
    }

    public void MenuAction()
    {
        MenuUpdate = true;
        
    }

    void RefreshMenu()
    {
        InspectorPanel.GetComponent<UIInspectorPanel>().ObjectUpdated();
    }

    public void EndInspection()
    {
        InspectorPanel.SetActive(false);
        InspectedUnit = null;
    }
}

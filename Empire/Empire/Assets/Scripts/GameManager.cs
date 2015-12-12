using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    public static GameManager Main;
   // public LevelData levelData;
    new public Camera camera;
   // public float Date;

    public Team Player;
    public List<Team> AllTeams;
    public List<BasicBounty> AllBounties;
    public GameObject BountyTemplate;
    bool running = false;
    public bool PlacementMode { get { return PlacementModel.activeInHierarchy; } }

    List<BasicUnit> AllInitialEnemyStructures;

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
    public GameObject StoryPanel;
    public GameObject PlayerInfoPanel;
    public GameObject ServicesPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject OptionsPanel;
    public GameObject OptionsButton;

    public Vector3 MapBounds;


    GameObject castle;

    Vector3 groundCamOffset;
    Vector3 camTarget;
    Vector3 camSmoothDampV;
    const float PlacementConfirmDistance = 100f;
    const float DefaultCameraHeight = 40f;

    public float CameraHeightScale()
    {
        return DefaultCameraHeight/camera.transform.position.y;
    }

    private Vector3 GetWorldPosAtViewportPoint(float vx, float vy)
    {
        Ray worldRay = camera.ViewportPointToRay(new Vector3(vx, vy, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distanceToGround;
        groundPlane.Raycast(worldRay, out distanceToGround);
        //Debug.Log("distance to ground:" + distanceToGround);
        return worldRay.GetPoint(distanceToGround);
    }

    void cameraTrackingSetup()
    {
        Vector3 groundPos = GetWorldPosAtViewportPoint(0.5f, 0.5f);
        //Debug.Log("groundPos: " + groundPos);
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

    public static bool Running
    {
        get { return Main.running; }
    }


    void Awake()
    {
        Main = this;
        AllTeams = new List<Team>();
        AllBounties = new List<BasicBounty>();
        if(AppManager.CurrentLevel == null)
        {
            AppManager.CurrentLevel = AppManager.Main.DemoLevel;
        }
    }

    // Use this for initialization
    void Start()
    {
        running = true;
        PlacementModel.SetActive(false);
        InspectorPanel.SetActive(false);
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
        OptionsPanel.SetActive(false);

        cameraTrackingSetup();

        castle = GameObject.FindGameObjectWithTag("Castle");
        CenterCameraOnUnit(castle.GetComponent<BasicUnit>(), false);
        StartInspection(castle.GetComponent<BasicUnit>());

        AllInitialEnemyStructures = new List<BasicUnit>();
        foreach (BasicUnit unit in FindObjectsOfType<BasicUnit>())
            if (unit != null && unit.HasTag(BasicUnit.Tag.Monster) && unit.HasTag(BasicUnit.Tag.Structure))
                AllInitialEnemyStructures.Add(unit);


        StoryPanelStart(AppManager.CurrentLevel.GameStartDialog, false);
    }

    void ShowHUD(bool value)
    {
        InspectorPanel.SetActive(value);
        PlayerInfoPanel.SetActive(value);
        ServicesPanel.SetActive(value);
        OptionsButton.SetActive(value);
    }

    // Update is called once per frame
    bool MenuUpdate = false;
    void Update()
    {
        if (Running)
        {
            StructurePlacementHandler();
            playTime += Time.deltaTime;
            if (MenuUpdate)
            {
                RefreshMenu();
                MenuUpdate = false;
            }

            if (nextInspectionTarget != null)
            {
                StartInspection(nextInspectionTarget);
                nextInspectionTarget = null;
            }

            CameraFixer();    

            CheckGameOver();
        }
    }

    void CameraFixer()
    {
        float xPos = Mathf.Clamp(camera.transform.position.x, 0, MapBounds.x);
        float yPos = Mathf.Clamp(camera.transform.position.y, 20, 200);
        float zPos = Mathf.Clamp(camera.transform.position.z, 0, MapBounds.z);
        camera.transform.position = new Vector3(xPos, yPos, zPos);
    }

    //GAME COMPLETION LOGIC
    void CheckGameOver()
    {
        while (AllInitialEnemyStructures.Contains(null))
            AllInitialEnemyStructures.Remove(null);

        if(castle==null)
        {
            EndGame();
            LoseLogic();
        }
        else if (AllInitialEnemyStructures.Count == 0)
        {
            EndGame();
            WinLogic();
        }
    }

    void EndGame()
    {
        ShowHUD(false);
        running = false;
    }

    void WinLogic()
    {
        WinPanel.SetActive(true);
        AppManager.CurrentLevel.SaveWin();
    }

    void LoseLogic()
    {
        LosePanel.SetActive(true);
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

            Vector3 tappedScreenPoint = camera.WorldToScreenPoint(placementPosition);
            Vector3 currentScreenPoint = camera.WorldToScreenPoint(PlacementModel.transform.position);

            /*if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Tapped Position: " + tappedScreenPoint);
                Debug.Log("Last Position: " + currentScreenPoint);
                Debug.Log("Distance:" + Vector3.Distance(tappedScreenPoint, currentScreenPoint));
            }*/

            if (Input.GetMouseButton(0) && Vector3.Distance(tappedScreenPoint, currentScreenPoint) > PlacementConfirmDistance)
            {
                PlacementModel.transform.position = placementPosition;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (!PlacementModel.GetComponent<UIPlacementModel>().Blocked &&
                    Vector3.Distance(tappedScreenPoint, currentScreenPoint) <= PlacementConfirmDistance)
                {//location confirmed, place structure

                    BasicUnit placedUnit = null;
                    if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.Structure)
                    {
                        placedUnit = Player.PlaceStructure(PlacementTemplate.GetComponent<BasicUnit>(), PlacementModel.transform.position);
                    }
                    else if (PlacementModel.GetComponent<UIPlacementModel>().currentMode == UIPlacementModel.PlacementMode.ExploreBounty)
                    {
                        Player.PlaceExploreBounty(PlacementTemplate.GetComponent<BasicBounty>(), PlacementModel.transform.position);
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
        StartInspection(structure);
        UIInspectorPanel.Main.EnterDescriptionMode();
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
        UIInspectorPanel.Main.ExitDescriptionMode();
        //MenuAction();

    }

    public void CancelPlacement()
    {
        StartInspection(castle.GetComponent<BasicUnit>());
        EndPlacement();
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

    //Story Panel Logic

    public void StoryPanelStart(LevelData.DialogInfo dialogInfo, bool showHud)
    {
        ShowHUD(showHud);
        StoryPanel.GetComponent<UIStoryWindow>().SetStoryFields(dialogInfo);
        StoryPanel.gameObject.SetActive(true);
        running = false;
    }

    public void StoryPanelEnd()
    {
        ShowHUD(true);
        StoryPanel.gameObject.SetActive(false);
        running = true;
        RefreshMenu();
    }

    //Options Panel

    public void OpenOptionsPanel()
    {

        ShowHUD(false);
        OptionsPanel.SetActive(true);
    }

    public void CloseOptionsPanel()
    {
        ShowHUD(true);
        OptionsPanel.SetActive(false);
    }
}

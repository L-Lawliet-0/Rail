using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private static InputManager m_Instance;
    public static InputManager Instance { get { return m_Instance; } }

    public bool MouseHoding;
    private Vector3 InputCache;

    private bool SelectionMode = false;

    private bool RoadMode = false;
    private bool ChosedDesination = false;
    private bool ManipulatePoint = false;

    private bool TrainMode = false;
    private float PressedTime;

    public GameObject Marker;

    //public EventSystem UIcontrol;

    private void Awake()
    {
        m_Instance = this;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PressedTime = 1;
            return;
        }

        if (RoadMode)
        {
            if (!ChosedDesination)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    worldPos.z = 0;
                    ChosedDesination = RoadManager.Instance.TryEndPoint(worldPos);
                }
            }
            else
            {
                if (ManipulatePoint)
                {
                    if (Input.GetMouseButton(0))
                    {
                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        worldPos.z = 0;
                        RoadManager.Instance.UpdateControlPoint(worldPos);
                    }
                    else
                        ManipulatePoint = false;

                    return;
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        worldPos.z = 0;
                        ManipulatePoint = RoadManager.Instance.TryControlPoint(worldPos);
                    }
                }
            }
        }

        if (TrainMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                if (TrainManager.Instance.TryPoint(worldPos))
                    return;
            }
        }

        // update camera position
        if (!MouseHoding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                InputCache = Input.mousePosition;
                MouseHoding = true;
                PressedTime = 0;
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                // update camera position, reverse direction
                CameraController.Instance.MoveCamera(InputCache - Input.mousePosition);
                InputCache = Input.mousePosition;
                PressedTime += Time.deltaTime;
            }
            else
            {
                // release
                MouseHoding = false;
            }
        }

        // update camera zoom
        CameraController.Instance.Zoom(-Input.mouseScrollDelta.y);

        if (!SelectionMode)
        {
            if (!RoadMode && !TrainMode && Input.GetMouseButtonUp(0) && PressedTime < .15f)
            {
                // check if it hits road as well
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                GameObject hitRoad = null;
                GameObject hitTrain = null;

                if (Physics.Raycast(ray, out hit, 11, 1 << LayerMask.NameToLayer("Train")))
                {
                    Debug.LogError("hit train!");
                    hitTrain = hit.collider.gameObject;
                }

                if (Physics.Raycast(ray, out hit, 11, 1 << LayerMask.NameToLayer("Road")))
                {
                    Debug.LogError("Hit road!");
                    hitRoad = hit.collider.transform.parent.gameObject; // the visualization object
                    // highlight the road as well
                }

                // right click, log pressed grid
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                GameMain.Instance.OnGridRightClick(worldPos, hitRoad, hitTrain);
                CityManager.Instance.OnGridClick(worldPos, hitTrain != null);
                EnterSelectionMode();
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0) && PressedTime < .15f)
            {
                ExitSelectionMode();
            }
        }
    }

    public void EnterSelectionMode()
    {
        SelectionMode = true;
        Marker.SetActive(true);
        Marker.transform.position = GameMain.Instance.HighLightGrid.PosV3;
    }

    public void ExitSelectionMode()
    {
        GameMain.Instance.DestroyHex();
        SelectionMode = false;
        HudManager.Instance.SetEmpty();

        Marker.SetActive(false);
        CityManager.Instance.Clear();

        RoadManager.Instance.ClearCache();
        TrainManager.Instance.ClearCache();
    }

    public void EnterRoadMode(GridData.GridSave startGrid)
    {
        // this is testing shit
        SelectionMode = false;

        RoadMode = true;
        ChosedDesination = false;
        ManipulatePoint = false;
        RoadManager.Instance.InitData(startGrid);
    }

    public void ExitRoadMode()
    {
        RoadMode = false;
        ExitSelectionMode();
    }

    public void EnterTrainMode(GridData.GridSave startGrid)
    {
        SelectionMode = false;
        TrainMode = true;

        TrainManager.Instance.Init();
        TrainManager.Instance.NextPoint(startGrid);
    }

    public void EnterRepathMode()
    {
        SelectionMode = false;
        TrainMode = true;

        TrainManager.Instance.Init();
        TrainManager.Instance.Repath();
    }

    public void ExitTrainMode()
    {
        TrainMode = false;
        ExitSelectionMode();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        m_Instance = this;
    }

    void Update()
    {
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

        if (!SelectionMode)
        {
            // update camera position
            if (!MouseHoding)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    InputCache = Input.mousePosition;
                    MouseHoding = true;
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    // update camera position, reverse direction
                    CameraController.Instance.MoveCamera(InputCache - Input.mousePosition);
                    InputCache = Input.mousePosition;
                }
                else
                {
                    // release
                    MouseHoding = false;
                }
            }

            // update camera zoom
            CameraController.Instance.Zoom(-Input.mouseScrollDelta.y);

            if (!RoadMode && Input.GetMouseButtonDown(1))
            {
                // right click, log pressed grid
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                GameMain.Instance.OnGridRightClick(worldPos);
                EnterSelectionMode();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
                ExitSelectionMode();
        }
    }

    public void EnterSelectionMode()
    {
        SelectionMode = true;
    }

    public void ExitSelectionMode()
    {
        GameMain.Instance.DestroyHex();
        SelectionMode = false;
        HudManager.Instance.SetEmpty();
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
        SelectionMode = false;

        GameMain.Instance.DestroyHex();

        HudManager.Instance.SetEmpty();
    }

    public void EnterTrainMode(GridData.GridSave startGrid)
    {
        SelectionMode = false;
        TrainMode = true;

        TrainManager.Instance.Init();
        TrainManager.Instance.NextPoint(startGrid);
    }

    public void ExitTrainMode()
    {
        TrainMode = false;
        SelectionMode = false;

        GameMain.Instance.DestroyHex();

        HudManager.Instance.SetEmpty();
    }
}

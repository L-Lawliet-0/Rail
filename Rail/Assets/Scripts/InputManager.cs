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
    private bool DrawMode = false;
    private bool Drawing = false;
    private bool Erasing = false;

    private void Awake()
    {
        m_Instance = this;
    }

    void Update()
    {
        if (DrawMode)
        {
            if (Drawing || Erasing)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                if (Drawing && Input.GetMouseButton(0))
                {
                    RoadManager.Instance.DrawUpdate(worldPos);
                }
                else if (Erasing && Input.GetMouseButton(1))
                    RoadManager.Instance.DrawUpdate(worldPos, true);
                else
                {
                    Drawing = false;
                    Erasing = false;
                }

                return;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    worldPos.z = 0;
                    Drawing = RoadManager.Instance.TryStartDraw(worldPos);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    worldPos.z = 0;
                    Erasing = RoadManager.Instance.TryStartDraw(worldPos);
                }

                if (Drawing || Erasing)
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

            if (!DrawMode && Input.GetMouseButtonDown(1))
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
    }

    public void EnterDrawMode(GridData.GridSave startGrid)
    {
        // this is testing shit
        SelectionMode = false;

        DrawMode = true;
        Drawing = false;
        RoadManager.Instance.InitData(startGrid);
    }

    public void ExitDrawMode()
    {
        DrawMode = false;
        SelectionMode = false;
    }
}

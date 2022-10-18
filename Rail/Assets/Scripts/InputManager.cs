using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager m_Instance;
    public static InputManager Instance { get { return m_Instance; } }

    public bool MouseHoding;
    private Vector3 InputCache;

    private void Awake()
    {
        m_Instance = this;
    }

    void Update()
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
    }
}

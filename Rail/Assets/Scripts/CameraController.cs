using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController m_Instance;
    public static CameraController Instance { get { return m_Instance; } }

    public const float MaxSize = 2600, MinSize = 48;
    public const float MapWidth = 7200, MapHeight = 5200;

    private Camera m_Camera;
    
    public static Vector3 CameraCenterPos = new Vector3(11616, 4533, -10);

    public float CurrentOrthographicSize { get { return m_Camera.orthographicSize; } set { UpdateOrthographicSize(value); } }
    public float CamWidth;
    public float POStoPIXEL;

    public float minX, maxX, minY, maxY; // the range of camera positions, update when zoom

    private const float ZoomSpeed = 2600f / 1;

    private void Awake()
    {
        m_Instance = this;
        m_Camera = GetComponent<Camera>();
    }

    private void Start()
    {
        CurrentOrthographicSize = MaxSize;
    }

    public void Zoom(float value)
    {
        CurrentOrthographicSize += value * ZoomSpeed * Time.deltaTime;
    }

    public void MoveCamera(Vector3 offset)
    {
        // the input offset is in pixel, update it to position
        Vector3 posOffset = offset / POStoPIXEL;
        transform.position += posOffset;
        ClampCameraPos();
    }

    public void ClampCameraPos()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);
        transform.position = new Vector3(x, y, transform.position.z);
    }

    public void UpdateOrthographicSize(float value)
    {
        value = Mathf.Clamp(value, MinSize, MaxSize);
        m_Camera.orthographicSize = value;

        // update bounding data
        CamWidth = value * 2 * m_Camera.aspect;
        POStoPIXEL = 1 / (m_Camera.ScreenToWorldPoint(Vector3.right).x - m_Camera.ScreenToWorldPoint(Vector3.zero).x);

        if (MapWidth > CamWidth)
        {
            minX = CameraCenterPos.x - MapWidth / 2 + CamWidth / 2;
            maxX = CameraCenterPos.x + MapWidth / 2 - CamWidth / 2;
        }
        else
            minX = maxX = CameraCenterPos.x;
        minY = CameraCenterPos.y - MapHeight / 2 + value;
        maxY = CameraCenterPos.y + MapHeight / 2 - value;
        ClampCameraPos();

        // update border line visualization
        if (value < 1000)
        {
            GameMain.Instance.CityLine.SetActive(true);
            GameMain.Instance.ProvinceLine.SetActive(false);

            CityNamesParent.Instance.ActivateNames(1);
        }
        else if (value < 2000)
        {
            GameMain.Instance.CityLine.SetActive(false);
            GameMain.Instance.ProvinceLine.SetActive(true);

            CityNamesParent.Instance.ActivateNames(0);
        }
        else
        {
            GameMain.Instance.CityLine.SetActive(false);
            GameMain.Instance.ProvinceLine.SetActive(false);

            CityNamesParent.Instance.ActivateNames(-1);
        }
    }
}

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
        if (value != 0)
            CurrentOrthographicSize += value * ZoomSpeed * Time.deltaTime;
    }

    public void MoveCamera(Vector3 offset)
    {
        // the input offset is in pixel, update it to position
        Vector3 posOffset = offset / POStoPIXEL;
        transform.position += posOffset;
        ClampCameraPos();
    }

    private float visible_x_min, visible_x_max, visible_y_min, visible_y_max;
    public void ClampCameraPos()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);
        transform.position = new Vector3(x, y, transform.position.z);

        visible_x_min = transform.position.x - CamWidth / 2;
        visible_x_max = transform.position.x + CamWidth / 2;
        visible_y_min = transform.position.y - CurrentOrthographicSize;
        visible_y_max = transform.position.y + CurrentOrthographicSize;
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

            CityNamesParent.Instance.ActivateNames(1, value / 1000f);
        }
        else if (value < 2000)
        {
            GameMain.Instance.CityLine.SetActive(false);
            GameMain.Instance.ProvinceLine.SetActive(true);

            CityNamesParent.Instance.ActivateNames(0, (value - 1000) / 1000f);
        }
        else
        {
            GameMain.Instance.CityLine.SetActive(false);
            GameMain.Instance.ProvinceLine.SetActive(false);

            CityNamesParent.Instance.ActivateNames(-1);
        }


        float scale = (value - 48f) / (2600 - 48f);
        InputManager.Instance.Marker.transform.localScale = Vector3.one + Vector3.one * Mathf.Lerp(0, 15f, scale);
        IconManager.Instance.UpdateSize(scale);

        Vector3 scale3 = Vector3.one + Vector3.one * Mathf.Lerp(0, 9f, scale);
        foreach (TrainManager.TrainData td in TrainManager.Instance.AllTrains)
        {
            td.TrainSprite.localScale = scale3;
            if (td.otherTransforms != null)
            {
                foreach (Transform tran in td.otherTransforms)
                    tran.localScale = scale3;
            }
        }

        return;

        RoadManager.Instance.UpdateRoadSize(scale);
    }

    // is this position visible by camera?
    public bool VisibleByCamera(Vector3 worldPos)
    {
        return worldPos.x > visible_x_min && worldPos.x < visible_x_max && worldPos.y > visible_y_min && worldPos.y < visible_y_max;

    }

    private const float Tolerance = 1000;
    public bool BiggerVisible(Vector3 worldPos)
    {
        return worldPos.x > visible_x_min - Tolerance && worldPos.x < visible_x_max + Tolerance && worldPos.y > visible_y_min - Tolerance && worldPos.y < visible_y_max + Tolerance;
    }
}

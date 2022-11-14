using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private static GameMain m_Instance;
    public static GameMain Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }

    public GameObject BorderLine, ProvinceLine, CityLine;

    private GameObject HighLightHex;
    public GridData.GridSave HighLightGrid;

    public void OnGridRightClick(Vector3 worldPos)
    {
        // reieve a right click action
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);

        // create a hexagon at grid location for testing purpose
        GameObject hex = new GameObject();
        hex.transform.position = grid.PosV3;
        MeshFilter mf = hex.AddComponent<MeshFilter>();
        mf.mesh = GlobalDataTypes.GetHexagonMesh();
        MeshRenderer mr = hex.AddComponent<MeshRenderer>();

        //Destroy(hex, 2);
        HighLightHex = hex;
        HighLightGrid = grid;

        HudManager.Instance.RightClickHud(grid);
    }

    public Sprite StationIcon, CrossIcon;
    public void BuildStation()
    {
        HudManager.Instance.StationBuild();
    }

    public void BuildCross()
    {
        TryBuildCross();
    }

    public void TryBuildCross()
    {
        if (HighLightHex)
        {
            if (HighLightGrid.StationData != null || HighLightGrid.CrossData != null)
            {
                Debug.LogError("this hex is already occupied by station or cross");
                return;
            }

            GameObject obj = new GameObject();
            obj.transform.position = HighLightGrid.PosV3;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CrossIcon;

            IconManager.Instance.AddOrUpdateIcon(HighLightGrid.Index, obj);

            HighLightGrid.CrossData = new GridData.CrossingSave();

            EconManager.Instance.MoneyCount -= GlobalDataTypes.CrossBuildPrice;

            HighLightGrid = null;
            DestroyHex();

            InputManager.Instance.ExitSelectionMode();
        }
    }

    public void TryBuildStation(int level)
    {
        if (HighLightHex)
        {
            if (HighLightGrid.StationData != null || HighLightGrid.CrossData != null)
            {
                Debug.LogError("this hex is already occupied by station or cross");
                return;
            }

            GameObject obj = new GameObject();
            obj.transform.position = HighLightGrid.PosV3;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = StationIcon;

            IconManager.Instance.AddOrUpdateIcon(HighLightGrid.Index, obj);

            HighLightGrid.StationData = new GridData.StationSave();
            HighLightGrid.StationData.Capacity = GlobalDataTypes.StationCapacity[level];

            EconManager.Instance.MoneyCount -= GlobalDataTypes.StationPrices[level];

            HighLightGrid = null;
            DestroyHex();

            InputManager.Instance.ExitSelectionMode();
        }
    }

    public void DestroyHex()
    {
        Destroy(HighLightHex);
        HighLightHex = null;
    }

    // when we build track, we enter draw mode
    public void BuildTrack()
    {
        if (HighLightGrid != null && (HighLightGrid.StationData != null || HighLightGrid.CrossData != null))
        {
            InputManager.Instance.EnterRoadMode(HighLightGrid);
            HudManager.Instance.TrackMode(HighLightGrid);
        }
    }

    public void PlaceTrain()
    {
        if (HighLightGrid != null && (HighLightGrid.StationData != null || HighLightGrid.CrossData != null) && TrainManager.Instance.GridConnectedRoads.ContainsKey(HighLightGrid.Index))
        {
            InputManager.Instance.EnterTrainMode(HighLightGrid);
            HudManager.Instance.TrainMode(HighLightGrid);
        }
    }
}

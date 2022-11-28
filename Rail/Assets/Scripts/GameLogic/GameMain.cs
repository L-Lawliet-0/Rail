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

    public void OnGridRightClick(Vector3 worldPos, GameObject road = null, GameObject train = null)
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

        // high light this visualization if grid is not station or cross
        if (train)
        {
            int index = TrainManager.Instance.FindIndex(train);
            TrainManager.Instance.AllTrains[index].Selected = true;
        }

        if (road && !train)
        {
            int index = RoadManager.Instance.ObjectToIndex(road);
            if (grid.StationData == null && grid.CrossData == null && RoadManager.Instance.RoadLevels[index] < GlobalDataTypes.MaxLevel)
            {
                RoadManager.Instance.HighLightCache();
            }
            else
                road = null;
        }
        else
            road = null;
        HudManager.Instance.RightClickHud(grid, road, train);
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

            int cost = EconManager.GetCrossCost(HighLightGrid);
            if (EconManager.Instance.MoneyCount < cost)
            {
                LogPanel.Instance.AppendMessage("Not Enough Money!!!!!");
                HighLightGrid = null;
                DestroyHex();

                InputManager.Instance.ExitSelectionMode();
                return;
            }
            EconManager.Instance.MoneyCount -= cost;

            GameObject obj = new GameObject();
            obj.transform.position = HighLightGrid.PosV3;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CrossIcon;

            IconManager.Instance.AddOrUpdateIcon(HighLightGrid.Index, obj);

            HighLightGrid.CrossData = new GridData.CrossingSave();

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

            int cost = EconManager.GetStationCost(HighLightGrid, level);
            if (EconManager.Instance.MoneyCount < cost)
            {
                LogPanel.Instance.AppendMessage("Not Enough Money!!!!!");
                HighLightGrid = null;
                DestroyHex();

                InputManager.Instance.ExitSelectionMode();
                return;
            }
            EconManager.Instance.MoneyCount -= cost;

            GameObject obj = new GameObject();
            obj.transform.position = HighLightGrid.PosV3;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = StationIcon;

            IconManager.Instance.AddOrUpdateIcon(HighLightGrid.Index, obj);
            IconManager.Instance.ChangeIconColor(HighLightGrid.Index, GlobalDataTypes.RarityColors[level]);

            HighLightGrid.StationData = new GridData.StationSave(level);

            CityManager.Instance.AddStationToCity(HighLightGrid.Index);

            HighLightGrid = null;
            DestroyHex();

            InputManager.Instance.ExitSelectionMode();
        }
    }

    public void UpgradeCrossToStation(GridData.GridSave grid)
    {
        // remove money
        int cost = EconManager.GetCrossUpgradeCost(grid);
        if (EconManager.Instance.MoneyCount < cost)
        {
            LogPanel.Instance.AppendMessage("Not Enough Money!!!!!");
            InputManager.Instance.ExitSelectionMode();
            return;
        }
        EconManager.Instance.MoneyCount -= cost;

        // add station data to it
        grid.CrossData = null;
        grid.StationData = new GridData.StationSave(0);

        CityManager.Instance.AddStationToCity(grid.Index);

        IconManager.Instance.SwapIconTexture(grid.Index, StationIcon);

        InputManager.Instance.ExitSelectionMode();
    }

    public void UpgradeStation(GridData.GridSave grid)
    {
        int cost = EconManager.GetStationUpgradeCost(grid, grid.StationData.Level);
        if (EconManager.Instance.MoneyCount < cost)
        {
            LogPanel.Instance.AppendMessage("Not Enough Money!!!!!");
            InputManager.Instance.ExitSelectionMode();
            return;
        }
        EconManager.Instance.MoneyCount -= cost;

        grid.StationData.Upgrade();

        IconManager.Instance.ChangeIconColor(grid.Index, GlobalDataTypes.RarityColors[grid.StationData.Level]);

        InputManager.Instance.ExitSelectionMode();
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

    // change the way the train move around
    public void RepathTrain()
    {
        TrainManager.Instance.RepathMode = true;
        InputManager.Instance.EnterTrainMode(HighLightGrid);
    }
}

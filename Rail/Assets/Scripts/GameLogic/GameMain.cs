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

        DrawCoverage(grid);

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
            CalculateStationCoverage(HighLightGrid);

            CityManager.Instance.AddStationToCity(HighLightGrid.Index);

            HighLightGrid = null;
            DestroyHex();

            InputManager.Instance.ExitSelectionMode();
        }
    }

    public const int Radius = 60;
    private GameObject CoverageObj;

    private void DrawCoverage(GridData.GridSave grid)
    {
        CoverageObj = new GameObject();
        CoverageObj.transform.position = Vector3.zero;
        int check = Radius / 10 * 2;
        int y = grid.Index / GlobalDataTypes.xCount;
        int x = grid.Index % GlobalDataTypes.xCount;

        int xLow = x - check;
        int xHigh = x + check;
        int yLow = y - check;
        int yHigh = y + check;

        for (int i = yLow; i <= yHigh; i++)
        {
            for (int j = xLow; j <= xHigh; j++)
            {
                int index = i * GlobalDataTypes.xCount + j;
                //Debug.LogError(index + "!!!");
                if (index >= 0 && index < GridData.Instance.GridDatas.Count)
                {
                    GridData.GridSave temp = GridData.Instance.GridDatas[index];
                    if (Vector3.Distance(temp.PosV3, grid.PosV3) <= Radius && CityManager.Instance.GridToCity.ContainsKey(temp.Index))
                    {
                        GameObject hex = new GameObject();
                        hex.transform.position = temp.PosV3;
                        MeshFilter mf = hex.AddComponent<MeshFilter>();
                        mf.mesh = GlobalDataTypes.GetHexagonMesh();
                        MeshRenderer mr = hex.AddComponent<MeshRenderer>();
                        mr.material = GlobalDataTypes.Instance.TestHexMaterial;
                        mr.material.SetColor("_BaseColor", Color.gray);
                        mr.sortingOrder = -1;
                        hex.transform.SetParent(CoverageObj.transform);
                    }
                }
            }
        }
    }

    private void CalculateStationCoverage(GridData.GridSave grid)
    {
        List<int> cities = new List<int>();
        List<float> coverage = new List<float>();
        int radius = Radius; // km
        int check = radius / 10 * 2;
        int y = grid.Index / GlobalDataTypes.xCount;
        int x = grid.Index % GlobalDataTypes.xCount;

        int xLow = x - check;
        int xHigh = x + check;
        int yLow = y - check;
        int yHigh = y + check;

        for (int i = yLow; i <= yHigh; i++)
        {
            for (int j = xLow; j <= xHigh; j++)
            {
                int index = i * GlobalDataTypes.xCount + j;
                //Debug.LogError(index + "!!!");
                if (index >= 0 && index < GridData.Instance.GridDatas.Count)
                {
                    GridData.GridSave temp = GridData.Instance.GridDatas[index];
                    if (Vector3.Distance(temp.PosV3, grid.PosV3) <= radius && CityManager.Instance.GridToCity.ContainsKey(temp.Index) && !temp.Covered)
                    {
                        temp.Covered = true;
                        int cityIndex = CityManager.Instance.GridToCity[temp.Index];
                        if (!cities.Contains(cityIndex))
                        {
                            cities.Add(cityIndex);
                            coverage.Add(0);
                        }
                        for (int k = 0; k < cities.Count; k++)
                        {
                            if (cities[k] == cityIndex)
                            {
                                coverage[k] += 1f / CityManager.Instance.CityGridCount[cityIndex];
                                break;
                            }
                        }
                    }
                }
            }
        }

        // convert percentage to multiplier
        Dictionary<int, float> dict = new Dictionary<int, float>();
        for (int i = 0; i < cities.Count; i++)
        {
            float cover = coverage[i];
            float minus = 1 - cover; // eg. coverage is .6, left is .4
            coverage[i] = 1 - Mathf.Pow(minus, 1f / 20); // each time left .96254, final left is .4, use 20 for a little faster flush
            dict.Add(cities[i], coverage[i]);

            Debug.LogError("City: " + CityManager.Instance.CityDatas[cities[i]].CityName.Split(',')[1] + " Coverage: " + coverage[i]);
        }

        grid.StationData.CityCoverage = dict;
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
        CalculateStationCoverage(grid);

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
        Destroy(CoverageObj);
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

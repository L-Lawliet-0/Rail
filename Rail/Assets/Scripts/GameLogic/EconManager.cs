using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EconManager : MonoBehaviour
{
    private static EconManager m_Instance;
    public static EconManager Instance { get { return m_Instance; } }

    public int MoneyCount { get { return m_Money; } set { UpdateMoney(value); } }
    private int m_Money;

    

    public Text MoneyTxt;
    public Text DailyCost;

    private int DailySpend;

    private bool MarkDirty;



    private void Awake()
    {
        m_Instance = this;
    }

    private void Start()
    {
        MoneyCount = GlobalDataTypes.Instance.StartBudget;
    }

    private void UpdateMoney(int value)
    {
        m_Money = value;
        MoneyTxt.text = "Money: " + m_Money.ToString();
    }

    public static int GetCrossCost(GridData.GridSave targetGrid, int level = 0)
    {
        return CostHelper(targetGrid, GlobalDataTypes.CrossBuildPrice);
    }

    public static int GetCrossUpgradeCost(GridData.GridSave targetGrid, int level = 0)
    {
        return CostHelper(targetGrid, GlobalDataTypes.CrossToStationPrice);
    }

    public static int GetStationCost(GridData.GridSave targetGrid, int level = 0)
    {
        return CostHelper(targetGrid, GlobalDataTypes.StationPrices[level]);
    }

    public static int GetStationUpgradeCost(GridData.GridSave targetGrid, int level = 0)
    {
        return CostHelper(targetGrid, GlobalDataTypes.StationUpgradePrices[level]);
    }

    public static int GetPathCost(List<GridData.GridSave> path, int level = 0)
    {
        int cost = 0;
        foreach (GridData.GridSave grid in path)
        {
            cost += CostHelper(grid, GlobalDataTypes.TrackPrices[level]);
        }
        return cost;
    }

    public static int GetPathCost(List<int> path, int level = 0)
    {
        int cost = 0;
        foreach (int index in path)
        {
            GridData.GridSave grid = GridData.Instance.GridDatas[index];
            cost += CostHelper(grid, GlobalDataTypes.TrackPrices[level]);
        }
        return cost;
    }

    public static int GetPathUpgradeCost(List<GridData.GridSave> path, int level = 0)
    {
        int cost = 0;
        foreach (GridData.GridSave grid in path)
        {
            cost += CostHelper(grid, GlobalDataTypes.TrackUpgradePrices[level]);
        }
        return cost;
    }

    public static int GetPathUpgradeCost(List<int> path, int level = 0)
    {
        int cost = 0;
        foreach (int index in path)
        {
            GridData.GridSave grid = GridData.Instance.GridDatas[index];
            cost += CostHelper(grid, GlobalDataTypes.TrackUpgradePrices[level]);
        }
        return cost;
    }

    private static int CostHelper(GridData.GridSave grid, float basePrice)
    {
        int cost = 0;
        float gdp;
        if (grid.name.Equals("sea"))
            gdp = CityManager.SEAGDP;
        else
            gdp = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]].GDP * CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]].Population;


        Debug.LogError("gdp : " + gdp);
        cost = Mathf.FloorToInt(GlobalDataTypes.GDPtoMult(gdp) * basePrice);

        Instance.MarkDirty = true;

        return cost;
    }

    public int CalculateDailySpend()
    {
        // daily spend = station + track + train

        int spend = 0;

        // station
        foreach (KeyValuePair<int, List<int>> pair in CityManager.Instance.CityStations)
        {
            foreach (int index in pair.Value)
            {
                GridData.GridSave grid = GridData.Instance.GridDatas[index];
                spend += CostHelper(grid, GlobalDataTypes.StationDailySpend[grid.StationData.Level]);
            }
        }

        // trains
        foreach (TrainManager.TrainData train in TrainManager.Instance.AllTrains)
        {
            spend += GlobalDataTypes.TrainDailySpend[train.Level];
        }

        // tracks
        for (int i = 0; i < RoadManager.Instance.AllRoads.Count; i++)
        {
            spend += GetPathCost(RoadManager.Instance.AllRoads[i], RoadManager.Instance.RoadLevels[i]);
        }

        return spend;
    }

    private void Update()
    {
        if (MarkDirty)
        {
            DailySpend = CalculateDailySpend();
            DailyCost.text = "Daily Spend : " + DailySpend;

            MarkDirty = false;
        }
    }
}

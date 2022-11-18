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


    private void Awake()
    {
        m_Instance = this;
        MoneyCount = GlobalDataTypes.StartBudget;
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
            gdp = 50;
        else
            gdp = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]].GDP;

        cost = Mathf.FloorToInt(GlobalDataTypes.GDPtoMult(gdp) * basePrice);

        return cost;
    }

}

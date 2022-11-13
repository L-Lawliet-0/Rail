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
        MoneyCount = 100000000;
    }

    private void UpdateMoney(int value)
    {
        m_Money = value;
        MoneyTxt.text = "Money: " + m_Money.ToString();
    }

    public int StationBaseCost = 1000;
    public int CrossBaseCost = 100;
    public int GetStationCrossCost(GridData.GridSave targetGrid, bool cross)
    {
        int cost = 0;
        if (targetGrid.name.Equals("sea"))
        {
            cost = 50000;
            if (cross)
                cost = 5000;
        }
        else
        {
            float gdp = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[targetGrid.Index]].GDP;
            cost = Mathf.FloorToInt(gdp * StationBaseCost);
            if (cross)
                cost = Mathf.FloorToInt(gdp * CrossBaseCost);
        }

        return cost;
    }

    public int PathBaseCost = 100;
    public int GetPathCost(List<GridData.GridSave> path)
    {
        int cost = 0;
        foreach (GridData.GridSave grid in path)
        {
            if (grid.name.Equals("sea"))
                cost += 5000;
            else
            {
                float gdp = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]].GDP;
                cost += Mathf.FloorToInt(gdp * PathBaseCost);
            }
        }

        return cost;
    }

}

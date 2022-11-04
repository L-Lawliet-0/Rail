using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    private static CityManager m_Instance;
    public static CityManager Instance { get { return m_Instance; } }

    public EconData InitialCityData;
    public List<CityData> CityDatas;

    private float Proportions = 5f / 1400f;

    public Transform TravelPanel;

    private Dictionary<int, int> GridToCity;

    private void Awake()
    {
        m_Instance = this;

        CityDatas = new List<CityData>();
        foreach (CityData cd in InitialCityData.Sheet1)
        {
            CityData city = new CityData();
            city.CityName = cd.CityName;
            city.GDP = cd.GDP;
            city.ResidentPopulation = (int)(cd.Population * 10000);
            city.VisitorPopulation = new Dictionary<int, int>();
            CityDatas.Add(city);
        }

        
    }

    private void Start()
    {
        // Debug.LogError("City count : " + CityNamesParent.Instance.transform.GetChild(1).childCount);

        for (int i = 0; i < CityDatas.Count; i++)
        {
            for (int j = 0; j < CityNamesParent.Instance.transform.GetChild(1).childCount; j++)
            {
                Transform tran = CityNamesParent.Instance.transform.GetChild(1).GetChild(j);
                string name = tran.name;

                if (name.Equals(CityDatas[i].CityName.Split(',')[1]))
                {
                    CityDatas[i].x = tran.position.x;
                    CityDatas[i].y = tran.position.y;
                    break;
                }
            }
        }

        CalculateTravelNeed();
        Clear();

        // build grid and city dictonary
        GridToCity = new Dictionary<int, int>();
        for (int i = 0; i < GridData.Instance.GridDatas.Count; i++)
        {
            GridData.GridSave grid = GridData.Instance.GridDatas[i];
            if (!grid.name.Equals("sea"))
            {
                for (int j = 0; j < CityDatas.Count; j ++)
                {
                    if (grid.name.Equals(CityDatas[j].CityName))
                    {
                        GridToCity.Add(i, j);
                        break;
                    }
                }
            }
        }
    }

    // calculate and log travel need info
    // 
    //
    private List<List<TravelData>> TravelNeeds;
    public void CalculateTravelNeed()
    {
        TravelNeeds = new List<List<TravelData>>();
        List<List<TravelData>> TotalMults = new List<List<TravelData>>();
        for (int i = 0; i < CityDatas.Count; i++)
        {
            // randomlize the travel cities
            float totalMult = 0;
            //Debug.LogError(CityDatas[i].ResidentPopulation);
            float travelPopulation = Proportions * CityDatas[i].ResidentPopulation;
            //Debug.LogError(travelPopulation.ToString());

            List<TravelData> need = new List<TravelData>();
            for (int j = 0; j < CityDatas.Count; j++)
            {
                if (i != j)
                {
                    // calculate how many people need to transport from i to j
                    float targetGDP = CityDatas[j].GDP;
                    float distance = Vector3.Distance(CityDatas[i].posV3, CityDatas[j].posV3);

                    float mult = targetGDP / distance;
                    totalMult += mult;

                    TravelData td = new TravelData() { HomeCity = i, TargetCity = j, Mult = mult };

                    need.Add(td);
                }
                else
                {
                    TravelData td = new TravelData() { HomeCity = i, TargetCity = i, Mult = 0 };

                    need.Add(td);
                }
            }
            
            for (int j = 0; j < need.Count; j++)
            {
                // resident travel need
                need[j].Population = Mathf.FloorToInt(need[j].Mult / totalMult * travelPopulation);
            }

            need.Sort(new Tsorter());

            TravelNeeds.Add(need);
        }

        // visitor needs
        for (int i = 0; i < CityDatas.Count; i++)
        {
            List<int> keys = new List<int>(CityDatas[i].VisitorPopulation.Keys);
            foreach (int key in keys)
            {
                TravelData returns = new TravelData();
                returns.HomeCity = key;
                returns.TargetCity = key;
                returns.Population = Mathf.FloorToInt(CityDatas[i].VisitorPopulation[key] * .2f * .8f);
                TravelNeeds[i].Add(returns);

                float total = 0;
                for (int j = 0; j < 10; j++)
                    total += TravelNeeds[i][j].Population;

                for (int j = 0; j < 10; j++)
                {
                    TravelData visits = new TravelData();
                    visits.HomeCity = key;
                    visits.TargetCity = TravelNeeds[i][j].TargetCity;
                    visits.Population = Mathf.FloorToInt(CityDatas[i].VisitorPopulation[key] * .2f * .2f * TravelNeeds[i][j].Population / total);
                    TravelNeeds[i].Add(visits);
                }
            }
        }

        float totalTravelCount = 0;
        foreach (List<TravelData> list in TravelNeeds)
        {
            foreach (TravelData tuple in list)
                totalTravelCount += tuple.Population;
        }

        Debug.LogError("This many people will travel this day: " + totalTravelCount);
    }

    public void OnGridClick(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid.name.Equals("sea"))
            return;
        for (int i = 0; i < TravelNeeds.Count; i++)
        {
            if (CityDatas[i].CityName.Equals(grid.name))
            {
                float total = 0;
                for (int j = 0; j < TravelNeeds[i].Count; j++)
                {
                    total += TravelNeeds[i][j].Population;
                }
                TravelPanel.GetChild(0).GetComponent<Text>().text = total.ToString();
                for (int j = 1; j < 5; j++)
                {
                    TravelPanel.GetChild(j).GetComponent<Text>().text = CityDatas[TravelNeeds[i][j - 1].TargetCity].CityName + " : " + TravelNeeds[i][j - 1].Population;
                }
                break;
            }
        }

        TravelPanel.gameObject.SetActive(true);
    }

    public void Clear()
    {
        TravelPanel.gameObject.SetActive(false);
    }

    public class Tsorter : IComparer<TravelData>
    {
        public int Compare(TravelData x, TravelData y)
        {
            return y.Population.CompareTo(x.Population);
        }
    }

    public class MultSorter : IComparer<TravelData>
    {
        public int Compare(TravelData x, TravelData y)
        {
            return y.Mult.CompareTo(x.Mult);
        }
    }

    /// <summary>
    /// important!!!!
    /// estimate train arrived at location, take and drop passenger in and out
    /// </summary>
    /// <param name="trainData"></param>
    public void TrainArrivedAtStation(TrainManager.TrainData trainData)
    {
        GridData.GridSave arriveStation = GridData.Instance.GridDatas[trainData.Paths[trainData.CurrentIndex]];

        if (arriveStation.StationData == null || arriveStation.name.Equals("sea"))
            return;

        // the city we are in
        int cityIndex = GridToCity[arriveStation.Index];

        Debug.LogError("The train is arriving at " + CityDatas[cityIndex].CityName);

        // unload travller
        for (int i = trainData.Passengers.Count - 1; i >= 0; i--)
        {
            // unload passenger
            if (trainData.Passengers[i].TargetCity == cityIndex)
            {
                AdjustVisitors(i, trainData.Passengers[i].HomeCity, trainData.Passengers[i].Population);
                Debug.LogError("unboarding !!!" + trainData.Passengers[i].Population);
                trainData.Passengers.RemoveAt(i);
            }
        }

        // load travller
        // calculator remaining seat
        int load = trainData.Capacity - trainData.CurrentCapacity();

        if (load <= 0)
            return;

        for (int i = 0; i < TravelNeeds[cityIndex].Count; i++)
        {
            TravelData td = TravelNeeds[cityIndex][i];
            if (PathContainsStation(trainData, td.TargetCity))
            {
                if (load >= td.Population)
                {
                    load -= td.Population;

                    td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = td.Population };
                    TravelNeeds[cityIndex][i].Population = 0;

                    trainData.Passengers.Add(td);
                    Debug.LogError("boarding !!!" + td.Population);
                }
                else
                {
                    td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = load };
                    TravelNeeds[cityIndex][i].Population -= load;

                    trainData.Passengers.Add(td);

                    load = 0;
                    Debug.LogError("boarding !!!" + td.Population);
                    break;
                }
            }
        }
        
    }

    public void AdjustVisitors(int cityIndex, int homeCity, int population)
    {
        if (CityDatas[cityIndex].VisitorPopulation.ContainsKey(homeCity))
        {
            CityDatas[cityIndex].VisitorPopulation[homeCity] += population;
            if (CityDatas[cityIndex].VisitorPopulation[homeCity] <= 0)
                CityDatas[cityIndex].VisitorPopulation.Remove(homeCity);
        }
        else if (population > 0)
        {
            CityDatas[cityIndex].VisitorPopulation.Add(homeCity, population);
        }
    }

    public bool PathContainsStation(TrainManager.TrainData train, int cityIndex)
    {
        for (int i = 0; i < train.Paths.Count; i++)
        {
            int gridIndex = train.Paths[i];
            if (!GridData.Instance.GridDatas[gridIndex].name.Equals("sea"))
            {
                if (GridToCity[gridIndex] == cityIndex)
                    return true;
            }
        }

        return false;
    }
}

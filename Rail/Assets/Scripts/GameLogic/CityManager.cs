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

    public Dictionary<int, int> GridToCity;

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

    public Transform TravelNeedsParent;
    public GameObject TravelArrowPrefab;
    public GameObject HumanCntPrefab;
    public void OnGridClick(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid.name.Equals("sea"))
            return;

        int arriveCnt = 0;
        for (int i = 0; i < TravelNeeds.Count; i++)
        {
            for (int j = 0; j < TravelNeeds[i].Count; j++)
            {
                if (TravelNeeds[i][j].TargetCity == GridToCity[grid.Index])
                {
                    // the target city is this grid
                    arriveCnt += TravelNeeds[i][j].Population;
                }
            }
        }

        for (int i = 0; i < TravelNeeds.Count; i++)
        {
            if (CityDatas[i].CityName.Equals(grid.name))
            {
                float total = 0;
                for (int j = 0; j < TravelNeeds[i].Count; j++)
                {
                    total += TravelNeeds[i][j].Population;
                }
                TravelPanel.GetChild(0).GetComponent<Text>().text = CityDatas[i].ResidentPopulation + " Resident" + CityDatas[i].VisitorCount + " Visitors";
                for (int j = 1; j < 11; j++)
                {
                    TravelPanel.GetChild(j).GetComponent<Text>().text = CityDatas[TravelNeeds[i][j - 1].TargetCity].CityName + " : " + TravelNeeds[i][j - 1].Population;
                }

                // testing, draw arrow for the top 7 travel city
                for (int j = 0; j < 7; j++)
                {
                    string cityName = CityDatas[TravelNeeds[i][j].TargetCity].CityName;

                    Vector3 targetPos = CityNamesParent.Instance.FindMatchCityPosition(cityName.Split(',')[1]);
                    if (targetPos.x != 0)
                    {
                        Vector3 startPos = grid.PosV3;
                        Vector3 direction = (targetPos - startPos).normalized;

                        int cnt = (int)(targetPos - startPos).magnitude / 30;

                        for (int c = 0; c < cnt; c++)
                        {
                            Vector3 spawnPos = startPos + direction * 30f * (c + 1);
                            GameObject spawn = Instantiate(TravelArrowPrefab, TravelNeedsParent);
                            spawn.transform.position = spawnPos;
                            spawn.transform.up = direction;
                        }

                        // one person indicate one hundred person
                        float population = TravelNeeds[i][j].Population / 100f;
                        float decimial = population - Mathf.FloorToInt(population);
                        int peopleCnt = Mathf.FloorToInt(population);

                        Vector3 peopleStartPos = targetPos - Vector3.right * 6 * peopleCnt / 2;
                        for (int c = 0; c < peopleCnt; c++)
                        {
                            GameObject obj = Instantiate(HumanCntPrefab, TravelNeedsParent);
                            obj.transform.position = peopleStartPos + Vector3.up * 20 + Vector3.right * c * 6;

                        }

                        if (decimial > 0)
                        {
                            GameObject obj = Instantiate(HumanCntPrefab, TravelNeedsParent);
                            obj.transform.position = peopleStartPos + Vector3.up * 20 + Vector3.right * 6 * peopleCnt;
                            obj.GetComponent<SpriteRenderer>().size = new Vector2(4.44f * decimial, 12);
                        }
                    }
                }

                break;
            }
        }

        TravelPanel.gameObject.SetActive(true);
    }

    public void Clear()
    {
        TravelPanel.gameObject.SetActive(false);
        for (int i = TravelNeedsParent.childCount - 1; i >= 0; i--)
            Destroy(TravelNeedsParent.GetChild(i).gameObject);
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
                AdjustPopulation(cityIndex, trainData.Passengers[i].HomeCity, trainData.Passengers[i].Population);
                Debug.LogError("unboarding !!!" + trainData.Passengers[i].Population);
                trainData.Passengers.RemoveAt(i);
            }
        }

        // load travller
        // calculator remaining seat
        int load = trainData.Capacity - trainData.CurrentCapacity();

        if (load <= 0)
            return;

        for (int i = arriveStation.StationData.StationQueue.Count - 1; i >= 0; i --)
        {
            TravelData td = arriveStation.StationData.StationQueue[i];
            if (PathContainsStation(trainData, td.TargetCity))
            {
                if (load >= td.Population)
                {
                    load -= td.Population;

                    td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = td.Population };
                    arriveStation.StationData.StationQueue.RemoveAt(i);

                    trainData.Passengers.Add(td);

                    if (load == 0)
                        break;

                }
                else
                {
                    td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = load };
                    arriveStation.StationData.StationQueue[i].Population -= load;

                    trainData.Passengers.Add(td);

                    break;
                }
            }
        }
        
    }

    /// <summary>
    /// hourly operation, flush the travel into each citys train station
    /// </summary>
    public void FlushNeedsToStation()
    {
        for (int i = 0; i < GridData.Instance.GridDatas.Count; i++)
        {
            GridData.GridSave grid = GridData.Instance.GridDatas[i];

            // we only check grid that has a station built on it
            if (!grid.name.Equals("sea") && grid.StationData != null)
            {
                int capacity = grid.StationData.Capacity - grid.StationData.CurrentCount();
                int cityIndex = GridToCity[grid.Index];

                if (capacity > 0)
                {
                    for (int j = TravelNeeds[cityIndex].Count - 1; j >= 0; j--)
                    {
                        TravelData td = TravelNeeds[cityIndex][j];

                        if (TrainManager.Instance.TrainPassBy(grid.Index, td.TargetCity, 60 * 60 * 3))
                        {
                            // check if this can be fulfilled
                            if (capacity >= td.Population)
                            {
                                TravelData newTD = new TravelData()
                                {
                                    HomeCity = td.HomeCity,
                                    TargetCity = td.TargetCity,
                                    Population = td.Population
                                };

                                grid.StationData.StationQueue.Add(newTD);
                                TravelNeeds[cityIndex].RemoveAt(j);
                                AdjustPopulation(cityIndex, td.HomeCity, -td.Population);

                                capacity -= td.Population;

                                if (capacity <= 0)
                                    break;
                            }
                            else
                            {
                                TravelData newTD = new TravelData()
                                {
                                    HomeCity = td.HomeCity,
                                    TargetCity = td.TargetCity,
                                    Population = capacity
                                };

                                grid.StationData.StationQueue.Add(newTD);
                                TravelNeeds[cityIndex][j].Population -= capacity;
                                AdjustPopulation(cityIndex, td.HomeCity, -capacity);

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public void AdjustPopulation(int cityIndex, int homeCity, int population)
    {
        if (cityIndex == homeCity)
        {
            CityDatas[cityIndex].ResidentPopulation += population;
            Debug.LogError("adjust population " + population);
        }
        else
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

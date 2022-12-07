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
    public List<int> CityGridCount;

    private float Proportions = 5f / 1400f;

    public Transform TravelPanel;

    public Dictionary<int, int> GridToCity;

    public float MinGDP, MaxGDP;
    public const float SEAGDP = 50000;

    public Dictionary<int, List<int>> CityStations;

    private void Awake()
    {
        m_Instance = this;

        CityDatas = new List<CityData>();

        MinGDP = float.MaxValue;
        MaxGDP = float.MinValue;
        foreach (CityData cd in InitialCityData.Sheet1)
        {
            CityData city = new CityData();
            city.CityName = cd.CityName;
            city.GDP = cd.GDP;
            city.ResidentPopulation = (int)(cd.Population * 10000);
            city.VisitorPopulation = new Dictionary<int, int>();
            city.Population = cd.Population;
            CityDatas.Add(city);

            MinGDP = Mathf.Min(cd.GDP * cd.Population, MinGDP);
            MaxGDP = Mathf.Max(cd.GDP * cd.Population, MaxGDP);
        }

        CityStations = new Dictionary<int, List<int>>();

    }

    private class GDPsort : IComparer<CityData>
    {
        public int Compare(CityData x, CityData y)
        {
            return (x.GDP * x.Population).CompareTo(y.GDP * y.Population);
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

        CityGridCount = new List<int>();
        for (int i = 0; i < CityDatas.Count; i++)
            CityGridCount.Add(0);
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
                        CityGridCount[j]++;
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
    public void OnGridClick(Vector3 worldPos, bool isTrain = false)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid.name.Equals("sea"))
            return;

        if (grid.StationData == null && !isTrain)
        {
            for (int i = 0; i < TravelNeeds.Count; i++)
            {
                if (CityDatas[i].CityName.Equals(grid.name))
                {
                    VisualizeTravelData(TravelNeeds[i]);

                    // testing, draw arrow for the top 7 travel city
                    for (int j = 0; j < 7; j++)
                    {
                        string cityName = CityDatas[TravelNeeds[i][j].TargetCity].CityName;
                        Vector3 targetPos = CityNamesParent.Instance.FindMatchCityPosition(cityName.Split(',')[1]);
                        if (targetPos.x != 0)
                        {
                            Vector3 startPos = grid.PosV3;
                            //Vector3 startPos = CityNamesParent.Instance.FindMatchCityPosition(CityDatas[i].CityName.Split(',')[1]);
                            float population = TravelNeeds[i][j].Population;
                            DrawArrowBetweenPoints(startPos, targetPos, population);
                        }
                    }
                    break;
                }
            }
        }
        else
        {
            // adjust this station queue
            List<TravelData> temp = new List<TravelData>();

            List<TravelData> target;
            if (isTrain)
            {
                TrainManager.TrainData train = TrainManager.Instance.AllTrains[TrainManager.Instance.TrainCache];
                target = train.Passengers;
            }
            else
                target = grid.StationData.StationQueue;

            for (int i = 0; i < target.Count; i++)
            {
                TravelData td = target[i];
                bool added = false;
                for (int j = 0; j < temp.Count; j++)
                {
                    if (temp[j].TargetCity == td.TargetCity)
                    {
                        temp[j].Population += td.Population;
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    TravelData dup = new TravelData()
                    {
                        TargetCity = td.TargetCity,
                        Population = td.Population
                    };
                    temp.Add(dup);
                }
            }

            VisualizeTravelData(temp);

            for (int i = 0; i < temp.Count; i++)
            {
                string cityName = CityDatas[temp[i].TargetCity].CityName;
                Vector3 targetPos = CityNamesParent.Instance.FindMatchCityPosition(cityName.Split(',')[1]);
                if (targetPos.x != 0)
                {
                    Vector3 startPos = grid.PosV3;
                    float population = temp[i].Population;
                    DrawArrowBetweenPoints(startPos, targetPos, population);
                }
            }
        }
        //TravelPanel.gameObject.SetActive(true);
        TravelPanel.GetComponent<Animation>().Play("ShiftRight");
    }

    public void VisualizeTravelData(List<TravelData> datas)
    {
        // visualize this data set
        RectTransform content = TravelPanel.GetComponent<ScrollRect>().content; // get the content

        // destroy any previous data
        for (int i = content.transform.childCount - 1; i >= 1; i--)
            Destroy(content.transform.GetChild(i).gameObject);

        content.sizeDelta = new Vector2(content.sizeDelta.x, datas.Count * 64);

        content.localPosition = Vector3.zero;

        // generate current data
        for (int i = 0; i < datas.Count; i++)
        {
            // duplicate the first gameobject
            GameObject obj = Instantiate(content.GetChild(0).gameObject);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(content.transform);
            rect.localPosition = Vector3.right * 128 + Vector3.down * i * 64 + Vector3.down * 32;

            // name of the city
            rect.GetChild(0).GetComponent<Text>().text = CityDatas[datas[i].TargetCity].CityName.Split(',')[1];
            rect.GetChild(0).gameObject.SetActive(true);

            // calculate the population
            float population = datas[i].Population / 100f;
            float decimial = population - Mathf.FloorToInt(population);
            int peopleCnt = Mathf.FloorToInt(population);

            for (int c = 0; c < peopleCnt; c++)
            {
                GameObject people = Instantiate(content.GetChild(0).GetChild(1).gameObject);
                RectTransform rectTran = people.GetComponent<RectTransform>();
                rectTran.SetParent(rect);
                rectTran.localPosition = Vector3.right * c * 22 - 53 * Vector3.right;
                people.SetActive(true);
            }

            if (decimial != 0)
            {
                GameObject people = Instantiate(content.GetChild(0).GetChild(1).gameObject);
                RectTransform rectTran = people.GetComponent<RectTransform>();
                rectTran.SetParent(rect);
                rectTran.localPosition = Vector3.right * peopleCnt * 22 - 53 * Vector3.right;
                people.SetActive(true);
                people.GetComponent<Image>().fillAmount = decimial;
            }

        }
    }

    public void Clear()
    {
        //TravelPanel.gameObject.SetActive(false);
        TravelPanel.GetComponent<Animation>().Play("ShiftLeft");
        for (int i = TravelNeedsParent.childCount - 1; i >= 0; i--)
            Destroy(TravelNeedsParent.GetChild(i).gameObject);
    }

    private void DrawArrowBetweenPoints(Vector3 start, Vector3 end, float population)
    {
        int cnt = (int)(end - start).magnitude / 30;
        Vector3 direction = (end - start).normalized;

        for (int c = 0; c < cnt; c++)
        {
            Vector3 spawnPos = start + direction * 30f * (c + 1);
            GameObject spawn = Instantiate(TravelArrowPrefab, TravelNeedsParent);
            spawn.transform.position = spawnPos;
            spawn.transform.up = direction;
            spawn.transform.GetComponent<SpriteRenderer>().sortingOrder = GlobalDataTypes.PointingArrowsOrder;
        }

        // one person indicate one hundred person
        population /= 100f;
        float decimial = population - Mathf.FloorToInt(population);
        int peopleCnt = Mathf.FloorToInt(population);
        Vector3 peopleStartPos = end - Vector3.right * 6 * peopleCnt / 2;
        for (int c = 0; c < peopleCnt; c++)
        {
            GameObject obj = Instantiate(HumanCntPrefab, TravelNeedsParent);
            obj.transform.position = peopleStartPos + Vector3.up * 20 + Vector3.right * c * 6;
            obj.GetComponent<SpriteRenderer>().sortingOrder = GlobalDataTypes.PeopleCntOrder;
        }

        if (decimial > 0)
        {
            GameObject obj = Instantiate(HumanCntPrefab, TravelNeedsParent);
            obj.transform.position = peopleStartPos + Vector3.up * 20 + Vector3.right * 6 * peopleCnt;
            obj.GetComponent<SpriteRenderer>().size = new Vector2(4.44f * decimial, 12);
            obj.GetComponent<SpriteRenderer>().sortingOrder = GlobalDataTypes.PeopleCntOrder;
        }
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

        int totalMoney = 0;
        int totalUnboard = 0;

        trainData.TotalUnboard = 0;
        trainData.TotalBoard = 0;
        trainData.TotalMoney = 0;

        trainData.Poped = false;

        // unload travller
        for (int i = trainData.Passengers.Count - 1; i >= 0; i--)
        {
            // check if this train path still contains the destination
            bool canReach = false;
            canReach = trainData.Paths.Contains(trainData.Passengers[i].TravelPath[0]);
            /*
            foreach (int index in trainData.Paths)
            {
                GridData.GridSave grid = GridData.Instance.GridDatas[index];
                if (!grid.name.Equals("sea") && grid.Index == trainData.Passengers[i].TargetCity && grid.StationData != null)
                {
                    canReach = true;
                    break;
                }
            }
            */

            // unload passenger
            if (!canReach || trainData.Passengers[i].TargetCity == cityIndex || trainData.Passengers[i].TravelPath[0] == arriveStation.Index)
            {
                if (!canReach || trainData.Passengers[i].TravelPath.Count <= 1)
                {

                    AdjustPopulation(cityIndex, trainData.Passengers[i].HomeCity, trainData.Passengers[i].Population);

                    // update transportation goal
                    if (canReach)
                    {
                        TimeManager.Instance.GoalTrack += trainData.Passengers[i].Population;
                        TimeManager.Instance.LastDayTrafficCount += trainData.Passengers[i].Population;
                        EconManager.Instance.MoneyCount += trainData.Passengers[i].TicketPriceDue;
                        TimeManager.Instance.LastDayIncomeCount += trainData.Passengers[i].TicketPriceDue;

                        totalMoney += trainData.Passengers[i].TicketPriceDue;
                    }

                    totalUnboard += trainData.Passengers[i].Population;
                    trainData.Passengers.RemoveAt(i);
                }
                else
                {
                    // flush this data into station
                    EconManager.Instance.MoneyCount += trainData.Passengers[i].TicketPriceDue;
                    TimeManager.Instance.LastDayIncomeCount += trainData.Passengers[i].TicketPriceDue;

                    totalMoney += trainData.Passengers[i].TicketPriceDue;
                    totalUnboard += trainData.Passengers[i].Population;

                    trainData.Passengers[i].TicketPriceDue = 0;
                    trainData.Passengers[i].TravelPath.RemoveAt(0);
                    arriveStation.StationData.StationQueue.Add(trainData.Passengers[i]);

                    trainData.Passengers.RemoveAt(i);
                }
            }
        }

        if (totalUnboard != 0)
        {
            LogPanel.Instance.AppendMessage(trainData.TrainName + " Unboard --- " + totalUnboard + " at station " + CityDatas[cityIndex].CityName.Split(',')[1]);
            trainData.TotalUnboard = totalUnboard;
            trainData.StationPause = true;
            trainData.PauseCounter = TrainManager.PauseTime;
        }

        if (totalMoney != 0)
        {
            LogPanel.Instance.AppendMessage(trainData.TrainName + " Income +++ " + totalMoney + CityDatas[cityIndex].CityName.Split(',')[1]);
            trainData.TotalMoney = totalMoney;
        }
        

        // load travller
        // calculator remaining seat
        int load = trainData.Capacity - trainData.CurrentCapacity();

        if (load <= 0)
            return;

        int totalBoard = 0;
        for (int i = arriveStation.StationData.StationQueue.Count - 1; i >= 0; i --)
        {
            TravelData td = arriveStation.StationData.StationQueue[i];

            // do these people want to take the train
            // probability of them taking the train
            // higher gdp means higher income means more likely the take the high price train

            if (PathContainsStation(trainData, td.TravelPath[0]))
            {

                float gdp = CityDatas[td.HomeCity].GDP * CityDatas[td.HomeCity].Population; // between 0 ~ 30 // about
                float price = trainData.TrainPrice;  // between .1f and .5f, or min and max

                float percent = GDPtoTravelPercent(gdp, price);

                int population = Mathf.FloorToInt(percent * td.Population);

                int notafford = td.Population - population;
                if (notafford > 0)
                {
                    td.Population -= notafford;
                    AdjustPopulation(cityIndex, td.HomeCity, notafford);
                    if (td.Population <= 0)
                    {
                        arriveStation.StationData.StationQueue.RemoveAt(i);
                        continue;
                    }
                }

                if (population > 0)
                {
                    if (load >= population)
                    {
                        load -= population;

                        td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = population, TravelPath = td.TravelPath };
                        

                        arriveStation.StationData.StationQueue[i].Population -= population;
                        totalBoard += population;

                        if (arriveStation.StationData.StationQueue[i].Population <= 0)
                            arriveStation.StationData.StationQueue.RemoveAt(i);

                        trainData.Passengers.Add(td);

                        if (load == 0)
                            break;
                    }
                    else
                    {
                        td = new TravelData() { HomeCity = td.HomeCity, TargetCity = td.TargetCity, Population = load, TravelPath = td.TravelPath };
                        arriveStation.StationData.StationQueue[i].Population -= load;
                        totalBoard += load;

                        trainData.Passengers.Add(td);

                        break;
                    }
                }
            }
        }

        if (totalBoard != 0)
        {
            LogPanel.Instance.AppendMessage(trainData.TrainName + " board +++ " + totalBoard + CityDatas[cityIndex].CityName.Split(',')[1]);
            trainData.TotalBoard = totalBoard;
            trainData.StationPause = true;
            trainData.PauseCounter = TrainManager.PauseTime;
        }

    }

    public float GDPtoTravelPercent(float gdp, float price)
    {
        float percent = (.6f - price) * 2; // 20 ~ 100% .3f => 60%
        // convert gdp level to percent
        // 0 ~ 1
        float gdpMult = (gdp - MinGDP) / (MaxGDP - MinGDP);
        percent += gdpMult;
        percent = Mathf.Clamp(percent, 0, 1);

        return percent;
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
                int cityIndex = GridToCity[grid.Index];

                // check existing queue, force them to leave if their needs can't be sat
                for (int j = grid.StationData.StationQueue.Count - 1; j >= 0; j--)
                {
                    TravelData td = grid.StationData.StationQueue[j];
                    if (!TrainManager.Instance.TrainPassByCheck(grid.Index, td.TravelPath[0], 60 * 60 * 3))
                    {
                        // force them to leave station
                        AdjustPopulation(cityIndex, td.HomeCity, td.Population);
                        grid.StationData.StationQueue.RemoveAt(j);
                    }
                }

                int capacity = grid.StationData.Capacity - grid.StationData.CurrentCount();
                

                if (capacity > 0)
                {
                    List<int> keys = new List<int>(grid.StationData.CityCoverage.Keys);

                    foreach (int cityKey in keys)
                    {
                        if (capacity <= 0)
                            break;

                        for (int j = TravelNeeds[cityKey].Count - 1; j >= 0; j--)
                        {
                            TravelData td = TravelNeeds[cityKey][j];
                            if (td.TargetCity == cityIndex)
                                continue;

                            List<int> routes = new List<int>();

                            int population = Mathf.FloorToInt(td.Population * grid.StationData.CityCoverage[cityKey]);

                            // if less than 5 people, we flush all of them or 5 people in side the station
                            if (population < 5 && td.Population > 0 && grid.StationData.CityCoverage[cityKey] > 0)
                                population = Mathf.Min(td.Population, 5);

                            if (population > 0 && TrainManager.Instance.TrainPassBy(grid.Index, td.TargetCity, 60 * 60 * 3, out routes))
                            {
                                // check if this can be fulfilled
                                if (capacity >= population)
                                {
                                    TravelData newTD = new TravelData()
                                    {
                                        HomeCity = td.HomeCity,
                                        TargetCity = td.TargetCity,
                                        Population = population,
                                        TravelPath = routes
                                    };

                                    grid.StationData.StationQueue.Add(newTD);

                                    TravelNeeds[cityKey][j].Population -= population;
                                    if (TravelNeeds[cityKey][j].Population <= 0)
                                        TravelNeeds[cityKey].RemoveAt(j);
                                    AdjustPopulation(cityKey, td.HomeCity, -population);

                                    capacity -= population;

                                    if (capacity <= 0)
                                        break;
                                }
                                else
                                {
                                    TravelData newTD = new TravelData()
                                    {
                                        HomeCity = td.HomeCity,
                                        TargetCity = td.TargetCity,
                                        Population = capacity,
                                        TravelPath = routes
                                    };

                                    grid.StationData.StationQueue.Add(newTD);
                                    TravelNeeds[cityKey][j].Population -= capacity;
                                    AdjustPopulation(cityKey, td.HomeCity, -capacity);

                                    capacity = 0;
                                    break;
                                }
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

    public bool PathContainsStation(TrainManager.TrainData train, int gridIndex)
    {
        return GridData.Instance.GridDatas[gridIndex].StationData != null && train.Paths.Contains(gridIndex);
    }

    public void AddStationToCity(int gridIndex)
    {
        int city = GridToCity[gridIndex];

        if (CityStations.ContainsKey(city))
            CityStations[city].Add(gridIndex);
        else
        {
            List<int> list = new List<int>();
            list.Add(gridIndex);
            CityStations.Add(city, list);
        }
    }
}

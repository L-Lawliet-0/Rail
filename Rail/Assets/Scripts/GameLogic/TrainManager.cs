using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    public static int TrainNameIndex = 500;
    public const int PauseTime = 2;
    [System.Serializable]
    public class TrainData
    {
        public List<int> Paths; // the path the train has to follow

        public List<int> newPaths;
        public bool changePath;

        public int CurrentIndex;
        public float Progress;
        public float TrainSpeed; // speed of this fucking train
        public int Capacity;
        public int Level;
        public bool Selected;
        public float TrainPrice; // per km

        public Transform TrainSprite;

        public List<TravelData> Passengers;

        public bool StationPause; // the train is in a pause in station
        public float PauseCounter;

        public int TotalBoard, TotalUnboard, TotalMoney; // station info cache
        public bool Poped;

        public string TrainName;
        public TrainData()
        {
            TrainName = "G" + TrainNameIndex++;
        }

        public int CurrentCapacity()
        {
            int cnt = 0;
            foreach (TravelData td in Passengers)
                cnt += td.Population;
            return cnt;
        }
    }

    public GameObject TrainPrefab;
    public List<TrainData> AllTrains;

    private static TrainManager m_Instance;
    public static TrainManager Instance { get { return m_Instance; } }

    private List<GridData.GridSave> CurrentPath;
    private HashSet<GridData.GridSave> CurrentChoices; // the connected possible grids

    public Dictionary<int, List<int>> GridConnectedRoads;

    private GameObject CurrentHighlight;

    private void Awake()
    {
        m_Instance = this;
        AllTrains = new List<TrainData>();
        RoadIndexs = new Dictionary<GridData.GridSave, GameObject>();
        TrainCache = -1;
    }

    private void Start()
    {
        ConstructConnections();
    }

    private void Update()
    {
        bool updateCapacity = false;
        foreach (TrainData td in AllTrains)
        {
            if (td.Selected)
                continue;

            if (td.StationPause)
            {
                td.PauseCounter -= Time.deltaTime;
                if (td.PauseCounter <= 0)
                    td.StationPause = false;

                if (!td.Poped)
                {
                    td.Poped = true;
                    CityNamesParent.Instance.ShowBoardInfo(td);
                }

                continue;
            }

            TOP:
            // get the current road we're travelling
            List<int> connections = GridConnectedRoads[td.Paths[td.CurrentIndex]];
            List<int> currentPath = new List<int>();
            int roadIndex = -1;
            foreach (int i in connections)
            {
                List<int> road = RoadManager.Instance.AllRoads[i];
                if (road[0] == td.Paths[td.CurrentIndex + 1] || road[road.Count - 1] == td.Paths[td.CurrentIndex + 1])
                {
                    currentPath = road;
                    roadIndex = i;
                    if (currentPath[0] != td.Paths[td.CurrentIndex])
                        currentPath.Reverse();
                    break;
                }
            }

            float roadSpeed = GlobalDataTypes.Speeds[RoadManager.Instance.RoadLevels[roadIndex]];
            float trainSpeed = GlobalDataTypes.Speeds[td.Level];

            float speed = Mathf.Min(roadSpeed, trainSpeed) / 3600f;

            float travelDistance = speed * Time.deltaTime * TimeManager.RealTimeToGameTime * .5f; // the distance this train travel in last frame, divided by two because night yield less traffic

            float wholeDistance = 10f * (currentPath.Count - 1);

            float offset = travelDistance / wholeDistance;

            td.Progress += offset;

            if (td.Progress >= 1f)
            {
                travelDistance = wholeDistance * (td.Progress - 1f);
                td.CurrentIndex++;
                td.Progress = 0;
                if (td.CurrentIndex >= td.Paths.Count - 1)
                {
                    td.CurrentIndex = 0;
                    if (td.Paths[0] != td.Paths[td.Paths.Count - 1])
                        td.Paths.Reverse();
                }

                if (td.changePath)
                {
                    td.Paths = td.newPaths;
                    td.changePath = false;
                    td.CurrentIndex = 0;
                    td.Progress = 0;
                }

                for (int i = 0; i < td.Passengers.Count; i++)
                {
                    td.Passengers[i].TicketPriceDue += Mathf.FloorToInt((currentPath.Count - 1) * 10 * td.TrainPrice * td.Passengers[i].Population);
                }

                // this is where a train arrived at a station
                // drop and pick up passengers
                CityManager.Instance.TrainArrivedAtStation(td);
                updateCapacity = true;

                goto TOP;
            }

            // update sprite position based on progress

            float length = td.Progress * wholeDistance;
            int start = Mathf.FloorToInt(length / 10f);
            float hexOffset = length - (start * 10);
            if ((start > 0 || hexOffset > 5) && ((start < currentPath.Count - 2) || (start < currentPath.Count - 1 && hexOffset < 5)))
            {
                Vector3 g0;
                Vector3 g1;
                Vector3 g2;
                float t;
                if (hexOffset > 5)
                {
                    g0 = GridData.Instance.GridDatas[currentPath[start]].PosV3;
                    g1 = GridData.Instance.GridDatas[currentPath[start + 1]].PosV3;
                    g2 = GridData.Instance.GridDatas[currentPath[start + 2]].PosV3;
                    t = (hexOffset - 5f) / 10f;
                }
                else
                {
                    g0 = GridData.Instance.GridDatas[currentPath[start - 1]].PosV3;
                    g1 = GridData.Instance.GridDatas[currentPath[start]].PosV3;
                    g2 = GridData.Instance.GridDatas[currentPath[start + 1]].PosV3;
                    t = (hexOffset + 5f) / 10f;
                }
                Vector3 p0 = g0 + (g1 - g0) / 2;
                Vector3 p1 = g1;
                Vector3 p2 = g1 + (g2 - g1) / 2;

                td.TrainSprite.position = GlobalDataTypes.BezierCurve(p0, p1, p2, t);
                if (t < .98f)
                    td.TrainSprite.up = GlobalDataTypes.BezierCurve(p0, p1, p2, t + .01f) - td.TrainSprite.position;
                else
                    td.TrainSprite.up = g2 - td.TrainSprite.position;
            }
            else
            {
                GridData.GridSave g0 = GridData.Instance.GridDatas[currentPath[start]];
                GridData.GridSave g1 = GridData.Instance.GridDatas[currentPath[start + 1]];
                td.TrainSprite.position = g0.PosV3 + (g1.PosV3 - g0.PosV3).normalized * hexOffset;
                td.TrainSprite.up = (g1.PosV3 - g0.PosV3).normalized;
            }
            

        }

        CityNamesParent.Instance.UpdateTrainObjects(updateCapacity);
    }

    // initalize gridconnectedroads based on existing roads
    private void ConstructConnections()
    {
        GridConnectedRoads = new Dictionary<int, List<int>>();
        for (int i = 0; i < RoadManager.Instance.AllRoads.Count; i++)
        {
            int start = RoadManager.Instance.AllRoads[i][0];
            int end = RoadManager.Instance.AllRoads[i][RoadManager.Instance.AllRoads.Count - 1];

            AddOrUpdateConnection(start, end, i);
        }
    }

    public void AddOrUpdateConnection(int start, int end, int index)
    {
        if (!GridConnectedRoads.ContainsKey(start))
            GridConnectedRoads.Add(start, new List<int>());
        if (!GridConnectedRoads.ContainsKey(end))
            GridConnectedRoads.Add(end, new List<int>());

        if (!GridConnectedRoads[start].Contains(index))
            GridConnectedRoads[start].Add(index);

        if (!GridConnectedRoads[end].Contains(index))
            GridConnectedRoads[end].Add(index);
    }

    private Dictionary<GridData.GridSave, GameObject> RoadIndexs;
    public void Init()
    {
        CurrentPath = new List<GridData.GridSave>();
        CurrentChoices = new HashSet<GridData.GridSave>();
        RoadIndexs.Clear();
    }

    public void Repath()
    {
        RepathMode = true;
        TrainData td = AllTrains[TrainCache];
        NextPoint(GridData.Instance.GridDatas[td.Paths[td.CurrentIndex + 1]]);
        HudManager.Instance.TrainMode(GridData.Instance.GridDatas[td.Paths[td.CurrentIndex + 1]]);
    }

    /// <summary>
    /// highlight a grid and highlight all other connected grids
    /// </summary>
    /// <param name="grid"></param>
    public void NextPoint(GridData.GridSave grid)
    {
        Destroy(CurrentHighlight);

        CurrentHighlight = new GameObject();
        CurrentHighlight.transform.position = grid.PosV3;
        MeshFilter mf = CurrentHighlight.AddComponent<MeshFilter>();
        mf.mesh = GlobalDataTypes.GetHexagonMesh();
        MeshRenderer mr = CurrentHighlight.AddComponent<MeshRenderer>();
        mr.material = GlobalDataTypes.Instance.TestHexMaterial;
        mr.material.SetColor("_BaseColor", Color.blue);
        CurrentHighlight.transform.localScale = Vector3.one * 1.5f;
        mr.sortingOrder = 0;


        CurrentPath.Add(grid);
        CurrentChoices.Clear();

        if (CurrentPath.Count > 1)
        {
            List<int> oldConnection = GridConnectedRoads[CurrentPath[CurrentPath.Count - 2].Index];
            foreach (int i in oldConnection)
                RoadManager.Instance.SetRoadColor(i, Color.black);
        }
        List<int> connections = GridConnectedRoads[grid.Index];
        foreach (int i in connections)
        {
            List<int> road = RoadManager.Instance.AllRoads[i];
            int target;
            if (road[0] != grid.Index)
                target = road[0];
            else
                target = road[road.Count - 1];
            GridData.GridSave targetG = GridData.Instance.GridDatas[target];
            //if (!CurrentPath.Contains(targetG))
            //{
                CurrentChoices.Add(targetG);
                RoadManager.Instance.SetRoadColor(i, Color.yellow);
            //}
        }

        // draw the path exsited to color blue
        if (CurrentPath.Count > 1)
        {
            for (int i = 0; i < CurrentPath.Count - 1; i++)
            {
                GridData.GridSave g1 = CurrentPath[i];
                GridData.GridSave g2 = CurrentPath[i + 1];

                List<int> roads = GridConnectedRoads[g1.Index];
                foreach (int index in roads)
                {
                    List<int> road = RoadManager.Instance.AllRoads[index];
                    if (road[0] == g2.Index || road[road.Count - 1] == g2.Index)
                    {
                        RoadManager.Instance.SetRoadColor(index, Color.blue);
                        break;
                    }
                }
            }
        }

        // road index
        if (RoadIndexs.ContainsKey(grid))
        {
            CityNamesParent.Instance.CreateTrainIndex(CurrentPath.Count.ToString(), grid.PosV3, RoadIndexs[grid]);
        }
        else
        {
            RoadIndexs.Add(grid, CityNamesParent.Instance.CreateTrainIndex(CurrentPath.Count.ToString(), grid.PosV3).gameObject);
        }
    }

    public bool TryPoint(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (CurrentChoices.Contains(grid))
        {
            NextPoint(grid);
            return true;
        }
        return false;
    }

    public void SelectTrainLevel()
    {
        if (RepathMode)
            FinishTrain(0);
        else
            HudManager.Instance.TrainBuild();
    }

    public void CancelTrain()
    {
        for (int i = 0; i < RoadManager.Instance.AllRoads.Count; i++)
        {
            RoadManager.Instance.SetRoadColor(i, GlobalDataTypes.TrackRarityColors[RoadManager.Instance.RoadLevels[i]]);
        }

        CurrentPath.Clear();
        CurrentChoices.Clear();

        InputManager.Instance.ExitTrainMode();
        Destroy(CurrentHighlight);

        foreach (KeyValuePair<GridData.GridSave, GameObject> pair in RoadIndexs)
            Destroy(pair.Value);
        RoadIndexs.Clear();

        RepathMode = false;
    }
    public bool RepathMode;
    public void FinishTrain(int level)
    {
        List<int> paths = new List<int>();
        foreach (GridData.GridSave g in CurrentPath)
            paths.Add(g.Index);
        if (!RepathMode)
        {
            int cost = GlobalDataTypes.TrainPrices[level];
            if (EconManager.Instance.MoneyCount < cost)
            {
                LogPanel.Instance.AppendMessage("Not enough money!!!");
                CancelTrain();
                return;
            }

            EconManager.Instance.MoneyCount -= cost;
            EconManager.Instance.MarkDirty = true;

            TrainData td = new TrainData();
           
            td.Paths = paths;
            td.TrainSpeed = 200f / 3600f; // km per sec
            td.Progress = 0;
            td.CurrentIndex = 0;
            td.TrainSprite = Instantiate(TrainPrefab).transform;
            td.TrainSprite.GetComponent<SpriteRenderer>().color = GlobalDataTypes.RarityColors[level];
            td.Passengers = new List<TravelData>();

            td.Level = level;
            td.Capacity = GlobalDataTypes.TrainCapacity[level]; // testing

            td.TrainPrice = .3f;

            AllTrains.Add(td);

            CityNamesParent.Instance.CreateTrainCounter(td);
        }
        else
        {
            TrainData td = AllTrains[TrainCache];

            td.changePath = true;
            td.newPaths = paths;
        }

        CancelTrain();
    }

    public int TrainCache;
    public int FindIndex(GameObject obj)
    {
        for (int i = 0; i < AllTrains.Count; i++)
        {
            if (obj == AllTrains[i].TrainSprite.gameObject)
            {
                TrainCache = i;
                return i;
            }
        }
        return -1;
    }

    public void ClearCache()
    {
        if (TrainCache != -1)
        {
            AllTrains[TrainCache].Selected = false;
            TrainCache = -1;
        }
    }

    public void UpgradeTrain()
    {
        int cost = GlobalDataTypes.TrainUpgradePrices[AllTrains[TrainCache].Level];
        if (EconManager.Instance.MoneyCount < cost)
        {
            LogPanel.Instance.AppendMessage("Not enough money!!!");
            InputManager.Instance.ExitSelectionMode();
            return;
        }
        EconManager.Instance.MoneyCount -= cost;
        EconManager.Instance.MarkDirty = true;

        AllTrains[TrainCache].Level++;
        AllTrains[TrainCache].Capacity = GlobalDataTypes.TrainCapacity[AllTrains[TrainCache].Level];
        AllTrains[TrainCache].TrainSprite.GetComponent<SpriteRenderer>().color = GlobalDataTypes.RarityColors[AllTrains[TrainCache].Level];

        InputManager.Instance.ExitSelectionMode();
    }

    public bool FindRoute(int startGrid, int targetGrid, List<int> routes)
    {
        if (routes.Count > 3)
            return false; // dont over search

        List<TrainData> trains = new List<TrainData>();
        foreach (TrainData td in AllTrains)
        {
            foreach (int index in td.Paths)
            {
                if (index == targetGrid && GridData.Instance.GridDatas[index].StationData != null)
                {
                    trains.Add(td);
                    break;
                }
            }
        }

        if (trains.Count == 0)
            return false;

        routes.Add(targetGrid);
        
        foreach (TrainData td in trains)
        {
            if (td.Paths.Contains(startGrid))
            {
                routes.Add(startGrid);
                return true;
            }

            foreach (int index in td.Paths)
            {
                if (!routes.Contains(index) && FindRoute(startGrid, index, routes)) 
                {
                    return true;
                }
            }
        }

        routes.Remove(targetGrid);
        return false;
    }

    public bool TrainPassByCheck(int currentGrid, int targetGrid, int timeInterval)
    {
        foreach (TrainData td in AllTrains)
        {
            bool containCity = false;
            foreach (int index in td.Paths)
            {
                if (index == targetGrid && GridData.Instance.GridDatas[index].StationData != null)
                {
                    containCity = true;
                    break;
                }
            }

            if (td.Paths.Contains(currentGrid) && containCity)
            {


                // check if this train can be arrived within time interval
                float timeUsed = 0;
                bool firstTrack = true;

                int currentIndex = td.CurrentIndex;
                List<int> paths = new List<int>(td.Paths);


            TOP:
                // get the current road we're travelling
                List<int> connections = GridConnectedRoads[paths[currentIndex]];
                List<int> currentPath = new List<int>();
                foreach (int i in connections)
                {
                    List<int> road = RoadManager.Instance.AllRoads[i];
                    if (road[0] == paths[currentIndex + 1] || road[road.Count - 1] == paths[currentIndex + 1])
                    {
                        currentPath = road;

                        if (currentPath[0] != td.Paths[td.CurrentIndex])
                            currentPath.Reverse();
                        break;
                    }
                }

                float distance = (currentPath.Count - 1) * 10;
                timeUsed += distance / td.TrainSpeed;

                if (firstTrack)
                {
                    firstTrack = false;
                    timeUsed *= (1 - td.Progress);
                }

                currentIndex++;
                if (currentIndex >= paths.Count - 1)
                {
                    currentIndex = 0;
                    if (paths[0] != paths[paths.Count - 1])
                        paths.Reverse();
                }

                if (paths[currentIndex] == currentGrid)
                {
                    if (timeUsed <= timeInterval)
                        return true;
                }
                else
                    goto TOP;
            }
        }

        return false;
    }

    public bool TrainPassBy(int currentGrid, int targetCity, int timeInterval, out List<int> routes)
    {
        // get a list of station in target city
        routes = new List<int>();
        if (CityManager.Instance.CityStations.ContainsKey(targetCity))
        {
            List<int> stations = CityManager.Instance.CityStations[targetCity];

            

            foreach (int station in stations)
            {
                bool find = FindRoute(currentGrid, station, routes);
                if (find )//&& routes.Count > 2)
                {
                    routes.Reverse();
                    routes.RemoveAt(0); // remove the starting grid
                    targetCity = routes[0];
                    break;
                }
                routes.Clear();
            }
        }

        if (routes.Count == 0)
            return false;

        return TrainPassByCheck(currentGrid, targetCity, timeInterval);
    }

    public void AdjustTrainPrice(float value)
    {
        if (TrainCache != -1)
        {
            AllTrains[TrainCache].TrainPrice = value;
            HudManager.Instance.PriceAdjustor.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = value + " per km";
        }
    }
}

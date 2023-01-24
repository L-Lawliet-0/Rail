using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveControl : MonoBehaviour
{
    private static SaveControl m_Instance;
    public static SaveControl Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }

    public void Save(int slot)
    {
        string path = Application.persistentDataPath + "/Save/Slot" + slot + "/";
        if (Application.isEditor)
            path = Application.dataPath + "/Save/Slot" + slot + "/";

        Directory.CreateDirectory(path.Substring(0, path.Length - 1));

        // save station data
        // which is all the grids
        string stationPath = path + "Grids";
        using (Stream file = File.Open(stationPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, GridData.Instance.GridDatas);
        }

        // save road data
        string roadPath = path + "Paths";
        using (Stream file = File.Open(roadPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, RoadManager.Instance.AllRoads);
        }

        string roadLevelPath = path + "RoadLevels";
        using (Stream file = File.Open(roadLevelPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, RoadManager.Instance.RoadLevels);
        }

        // save train data
        string trainPath = path + "Trains";
        using (Stream file = File.Open(trainPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, TrainManager.Instance.AllTrains);
        }

        string connectionPath = path + "Connection";
        using (Stream file = File.Open(connectionPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, TrainManager.Instance.GridConnectedRoads);
        }

        // city datas
        string cityPath = path + "Cities";
        using (Stream file = File.Open(cityPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, CityManager.Instance.CityDatas);
        }

        // travel needs
        string travelNeedPath = path + "TravelNeed";
        using (Stream file = File.Open(travelNeedPath, FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, CityManager.Instance.TravelNeeds);
        }

        // time parameter
        string timePath = path + "Time";
        using (Stream file = File.Open(timePath, FileMode.OpenOrCreate))
        {
            TimeProperties tp = new TimeProperties();
            tp.DayCounter = TimeManager.Instance.DayCounter;
            tp.HourCounter = TimeManager.Instance.HourCounter;
            tp.MonthCount = TimeManager.Instance.MonthCount;
            tp.DayCount = TimeManager.Instance.DayCount;
            tp.HourCount = TimeManager.Instance.HourCount;
            tp.MonthlyGoal = TimeManager.Instance.MonthlyGoal;
            tp.GoalTrack = TimeManager.Instance.GoalTrack;
            tp.LastDayTrafficCount = TimeManager.Instance.LastDayTrafficCount;
            tp.LastDayIncomeCount = TimeManager.Instance.LastDayIncomeCount;
            tp.MoneyCount = EconManager.Instance.MoneyCount;
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, tp);
        }

        Main.Instance.RefreshSLpanel(slot);
    }

    [System.Serializable]
    public class TimeProperties
    {
        public float DayCounter, HourCounter;
        public int MonthCount, DayCount, HourCount;
        public int MonthlyGoal, GoalTrack;
        public int LastDayTrafficCount, LastDayIncomeCount;
        public int MoneyCount;
    }

    public void Load(int slot)
    {
        string path = Application.persistentDataPath + "/Save/Slot" + slot + "/";
        if (Application.isEditor)
            path = Application.dataPath + "/Save/Slot" + slot + "/";
        string stationPath = path + "Grids";
        string roadPath = path + "Paths";
        string roadLevelPath = path + "RoadLevels";
        string trainPath = path + "Trains";
        string connectionPath = path + "Connection";
        string cityPath = path + "Cities";
        string travelNeedPath = path + "TravelNeed";

        List<GridData.GridSave> GridDatas;
        List<List<int>> AllRoads;
        List<int> RoadLevels;
        List<TrainManager.TrainData> AllTrains;
        Dictionary<int, List<int>> GridConnectedRoads;
        List<CityData> CityDatas;
        List<List<TravelData>> TravelNeeds;

        using (Stream file = File.Open(stationPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GridDatas = bf.Deserialize(file) as List<GridData.GridSave>;
        }

        using (Stream file = File.Open(roadPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllRoads = bf.Deserialize(file) as List<List<int>>;
        }

        using (Stream file = File.Open(roadLevelPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            RoadLevels = bf.Deserialize(file) as List<int>;
        }

        using (Stream file = File.Open(trainPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllTrains = bf.Deserialize(file) as List<TrainManager.TrainData>;
        }

        using (Stream file = File.Open(connectionPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GridConnectedRoads = bf.Deserialize(file) as Dictionary<int, List<int>>;
        }
        
        using (Stream file = File.Open(cityPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            CityDatas = bf.Deserialize(file) as List<CityData>;
        }

        using (Stream file = File.Open(travelNeedPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            TravelNeeds = bf.Deserialize(file) as List<List<TravelData>>;
        }

        // now we have what we want
        // clear old data

        //clear city
        CityManager.Instance.ClearCityData();
        CityManager.Instance.CityDatas = CityDatas;
        CityManager.Instance.TravelNeeds = TravelNeeds;

        // clear station and cross
        IconManager.Instance.ClearAllIcon();
        // rebuild all icon
        GridData.Instance.GridDatas = GridDatas;
        GameMain.Instance.ReconstructStationAndCross();

        // rebuild the roads
        RoadManager.Instance.Clear();
        RoadManager.Instance.AllRoads = AllRoads;
        RoadManager.Instance.RoadLevels = RoadLevels;
        RoadManager.Instance.Reconstruct();

        // rebuild trains
        TrainManager.Instance.Clear();
        TrainManager.Instance.AllTrains = AllTrains;
        TrainManager.Instance.GridConnectedRoads = GridConnectedRoads;
        TrainManager.Instance.Reconstruct();

        // load time and etc stuff
        string timePath = path + "Time";
        using (Stream file = File.Open(timePath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            TimeProperties tp = new TimeProperties();
            tp = bf.Deserialize(file) as TimeProperties;

            TimeManager.Instance.DayCounter = tp.DayCounter;
            TimeManager.Instance.HourCounter = tp.HourCounter;
            TimeManager.Instance.MonthCount = tp.MonthCount;
            TimeManager.Instance.DayCount = tp.DayCount;
            TimeManager.Instance.HourCount = tp.HourCount;
            TimeManager.Instance.MonthlyGoal = tp.MonthlyGoal;
            TimeManager.Instance.GoalTrack = tp.GoalTrack;
            TimeManager.Instance.LastDayTrafficCount = tp.LastDayTrafficCount;
            TimeManager.Instance.LastDayIncomeCount = tp.LastDayIncomeCount;
            EconManager.Instance.MoneyCount = tp.MoneyCount;
        }

        Main.Instance.ForcePlay();
    }

    public Main.SaveSummary TryReadSave(int slot)
    {
        string path = Application.persistentDataPath + "/Save/Slot" + slot + "/";
        if (Application.isEditor)
            path = Application.dataPath + "/Save/Slot" + slot + "/";
        string stationPath = path + "Grids";
        string roadPath = path + "Paths";
        string trainPath = path + "Trains";

        if (!File.Exists(stationPath) || !File.Exists(roadPath) || !File.Exists(trainPath))
            return null;

        Main.SaveSummary ss = new Main.SaveSummary();

        List<GridData.GridSave> GridDatas;
        List<List<int>> AllRoads;
        List<TrainManager.TrainData> AllTrains;

        using (Stream file = File.Open(stationPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GridDatas = bf.Deserialize(file) as List<GridData.GridSave>;

            ss.StationCount = 0;
            foreach (GridData.GridSave grid in GridDatas)
            {
                if (grid.StationData != null)
                    ss.StationCount++;
            }
        }

        using (Stream file = File.Open(trainPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllTrains = bf.Deserialize(file) as List<TrainManager.TrainData>;

            ss.TrainCount = AllTrains.Count;
        }

        using (Stream file = File.Open(roadPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllRoads = bf.Deserialize(file) as List<List<int>>;

            ss.TrackLength = 0;
            foreach (List<int> road in AllRoads)
            {
                ss.TrackLength += (road.Count - 1) * 10;
            }
        }

        string timePath = path + "Time";
        using (Stream file = File.Open(timePath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            TimeProperties tp = new TimeProperties();
            tp = bf.Deserialize(file) as TimeProperties;


            ss.WeekCount = tp.MonthCount;
        }

        return ss;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            Save(1);
        if (Input.GetKeyDown(KeyCode.L))
            Load(1);
    }
}

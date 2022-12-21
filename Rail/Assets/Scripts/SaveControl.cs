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

    public void Save()
    {
        string path = Application.dataPath + "/Save/";
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
    }

    public void Load()
    {
        string path = Application.dataPath + "/Save/";
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            Save();
        if (Input.GetKeyDown(KeyCode.L))
            Load();
    }
}

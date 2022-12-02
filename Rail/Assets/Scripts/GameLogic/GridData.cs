using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[ExecuteInEditMode]
public class GridData : MonoBehaviour
{
    private static GridData m_Instance;
    public static GridData Instance { get { return m_Instance; } }

    [System.Serializable]
    public class GridSave
    {
        public float posX, posY; // position of this grid
        public string name; // name of this grid
        public int Index; // the sibling index
        public CrossingSave CrossData;
        public StationSave StationData;

        public bool Covered = false;

        public Vector3 PosV3 { get { return new Vector3(posX, posY); } }
    }

    [System.Serializable]
    public class CrossingSave
    {

    }

    [System.Serializable]
    public class StationSave
    {
        public List<TravelData> StationQueue;
        public int Capacity;
        public int Level;

        public Dictionary<int, float> CityCoverage;

        public StationSave(int level)
        {
            StationQueue = new List<TravelData>();
            Level = level;
            Capacity = GlobalDataTypes.StationCapacity[level];
        }

        public void Upgrade()
        {
            Level++;
            Capacity = GlobalDataTypes.StationCapacity[Level];
        }

        public int CurrentCount()
        {
            int cnt = 0;
            foreach (TravelData td in StationQueue)
            {
                cnt += td.Population;
            }
            return cnt;
        }
    }


    public List<GridSave> GridDatas;
    private void Awake()
    {
        m_Instance = this;

        if (Application.isPlaying)
            LoadData();
    }

    public bool InitSave;
    private void Update()
    {
        if (InitSave)
        {
            string dataPath = Application.dataPath + "/HexInfos";
            List<HexSaver.HexInfo> datas;

            using (Stream file = File.Open(dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                datas = bf.Deserialize(file) as List<HexSaver.HexInfo>;
            }

            List<GridSave> initDatas = new List<GridSave>();
            // let's generate some hexagons
            for (int i = 0; i < datas.Count; i ++)
            {
                HexSaver.HexInfo hi = datas[i];
                initDatas.Add(new GridSave() { name = hi.name, posX = hi.posX, posY = hi.posY, CrossData = null, StationData = null, Index = i });
            }

            dataPath = Application.dataPath + "/GridDatas";
            using (Stream file = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, initDatas);
            }

            InitSave = false;
        }
    }

    public void LoadData()
    {
        string dataPath = Application.dataPath + "/GridDatas";
        using (Stream file = File.Open(dataPath, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            GridDatas = bf.Deserialize(file) as List<GridSave>;
        }

        /*
        HashSet<string> gridNames = new HashSet<string>();
        foreach (GridSave gs in GridDatas)
            gridNames.Add(gs.name);

        foreach (string str in gridNames)
            Debug.LogError(str);
        */
    }

    /// <summary>
    /// from a world position, return the cloest hex grid on the map
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public GridSave GetNearbyGrid(Vector3 worldPos)
    {
        float x = worldPos.x - 8016;
        float y = worldPos.y - 1933;

        x /= GlobalDataTypes.Xdistance;
        y /= GlobalDataTypes.HexDistance;

        int xc = (int)x;
        int yc = (int)y;

        int[] candidates = new int[]
        {
            GlobalDataTypes.GetIndexXY(xc, yc - 1),
            GlobalDataTypes.GetIndexXY(xc, yc),
            GlobalDataTypes.GetIndexXY(xc, yc + 1),
            GlobalDataTypes.GetIndexXY(xc - 1, yc),
            GlobalDataTypes.GetIndexXY(xc - 1, yc - 1),
            GlobalDataTypes.GetIndexXY(xc - 1, yc + 1),
            GlobalDataTypes.GetIndexXY(xc + 1, yc),
            GlobalDataTypes.GetIndexXY(xc + 1, yc - 1),
            GlobalDataTypes.GetIndexXY(xc + 1, yc + 1),
        };

        float minDistance = float.MaxValue;
        int value = candidates[0];
        for (int i = 1; i < candidates.Length; i++)
        {
            float distance = Vector3.Distance(worldPos, GridDatas[candidates[i]].PosV3);
            if (distance < minDistance)
            {
                minDistance = distance;
                value = candidates[i];
            }
        }

        return GridDatas[value];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [System.Serializable]
    public class TrainData
    {
        public List<int> Paths; // the path the train has to follow
        public int CurrentIndex;
        public float Progress;
        public float TrainSpeed; // speed of this fucking train

        public Transform TrainSprite;
    }

    public GameObject TrainPrefab;
    private List<TrainData> AllTrains;

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
    }

    private void Start()
    {
        ConstructConnections();
    }

    private void Update()
    {
        foreach (TrainData td in AllTrains)
        {
            float travelDistance = td.TrainSpeed * Time.deltaTime * 1440; // the distance this train travel in last frame

            TOP:
            // get the current road we're travelling
            List<int> connections = GridConnectedRoads[td.Paths[td.CurrentIndex]];
            List<int> currentPath = new List<int>();
            foreach (int i in connections)
            {
                List<int> road = RoadManager.Instance.AllRoads[i];
                if (road[0] == td.Paths[td.CurrentIndex + 1] || road[road.Count - 1] == td.Paths[td.CurrentIndex + 1])
                {
                    currentPath = road;

                    if (currentPath[0] != td.Paths[td.CurrentIndex])
                        currentPath.Reverse();
                    break;
                }
            }

            Debug.LogError(currentPath.Count);
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

    public void FinishTrain(bool confirm)
    {
        if (confirm)
        {
            TrainData td = new TrainData();

            List<int> paths = new List<int>();
            foreach (GridData.GridSave g in CurrentPath)
                paths.Add(g.Index);
            td.Paths = paths;
            td.TrainSpeed = 200f / 3600f; // km per sec
            td.Progress = 0;
            td.CurrentIndex = 0;
            td.TrainSprite = Instantiate(TrainPrefab).transform;

            AllTrains.Add(td);
        }

        // reset all path color to black
        for (int i = 0; i < RoadManager.Instance.AllRoads.Count; i++)
        {
            RoadManager.Instance.SetRoadColor(i, Color.black);
        }

        /*
        if (CurrentPath.Count > 0)
        {
            List<int> oldConnection = GridConnectedRoads[CurrentPath[CurrentPath.Count - 1].Index];
            foreach (int i in oldConnection)
                RoadManager.Instance.SetRoadColor(i, Color.black);
        }
        */

        CurrentPath.Clear();
        CurrentChoices.Clear();

        InputManager.Instance.ExitTrainMode();
        Destroy(CurrentHighlight);

        foreach (KeyValuePair<GridData.GridSave, GameObject> pair in RoadIndexs)
            Destroy(pair.Value);
        RoadIndexs.Clear();
    }
}

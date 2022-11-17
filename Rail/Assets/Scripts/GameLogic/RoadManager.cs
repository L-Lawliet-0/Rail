using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    private static RoadManager m_Instance;
    public static RoadManager Instance { get { return m_Instance; } }

    public GameObject RoadLine;
    private void Awake()
    {
        m_Instance = this;

        AllRoads = new List<List<int>>();
        AllVisuals = new List<GameObject>();
        RoadLevels = new List<int>();

        CurrentTrack = new List<GridData.GridSave>();
        VisualGrids = new List<GameObject>();
        ControlPoints = new List<GridData.GridSave>();

        RoadCache = -1;

        CacheHighLight = new GameObject().transform;
        CacheHighLight.transform.position = Vector3.zero;
    }

    public GridData.GridSave StartGrid; // the start grid of this draw
    public GridData.GridSave CurrentGrid;

    public List<GridData.GridSave> CurrentTrack;
    private List<GameObject> VisualGrids;
    private List<GridData.GridSave> ControlPoints; // the points that used to control the flow of the track
    private int ControlPointIndex;

    public List<List<int>> AllRoads; // this is the data we need
    public List<GameObject> AllVisuals; // the visualization of the track, indexed
    public List<int> RoadLevels;

    public void InitData(GridData.GridSave grid)
    {
        StartGrid = CurrentGrid = grid;
        CurrentTrack.Clear();
        ControlPoints.Clear();

        CurrentTrack.Add(CurrentGrid);
    }

    public bool TryEndPoint(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid != StartGrid && (grid.StationData != null || grid.CrossData != null))
        {
            // build road from start point to end point using a*

            ControlPoints.Clear();
            ControlPoints.Add(StartGrid);
            ControlPoints.Add(grid);

            return RedrawPath();
        }

        return false;
    }

    private int recursiveCount;
    private const int maxRecursion = 100000;
    private const int maxDetour = 10;
    private int CurrentDetour;
    private bool RedrawPath()
    {
        List<GridData.GridSave> tempTracks = new List<GridData.GridSave>();
        tempTracks.Add(ControlPoints[0]);
        for (int i = 0; i < ControlPoints.Count - 1; i++)
        {
            List<GridData.GridSave> tracks = new List<GridData.GridSave>();
            recursiveCount = 0;
            CurrentDetour = 0;
            bool found = FindPath(ControlPoints[i], ControlPoints[i + 1], tempTracks, tracks); // tracks include the end point but not start point
            if (!found)
            {
                Debug.LogError("Illegal path!");
                return false;
            }
            tempTracks.AddRange(tracks);
        }

        for (int i = VisualGrids.Count - 1; i >= 0; i--)
            Destroy(VisualGrids[i]);
        VisualGrids.Clear();
        CurrentTrack.Clear();
        CurrentTrack.AddRange(tempTracks);

        VisualizeGrids(CurrentTrack, true);
        HudManager.Instance.UpdateRoadCost(EconManager.GetPathCost(CurrentTrack));

        return true;
    }

    private void VisualizeGrids(List<int> grids, bool addToList = false)
    {
        List<GridData.GridSave> values = new List<GridData.GridSave>();
        foreach (int index in grids)
            values.Add(GridData.Instance.GridDatas[index]);
        VisualizeGrids(values, addToList);
    }

    private void VisualizeGrids(List<GridData.GridSave> grids, bool addToList = false)
    {
        for (int i = 1; i < grids.Count - 1; i++)
        {
            GridData.GridSave g = grids[i];

            GameObject hex = new GameObject();
            hex.transform.position = g.PosV3;
            MeshFilter mf = hex.AddComponent<MeshFilter>();
            mf.mesh = GlobalDataTypes.GetHexagonMesh();
            MeshRenderer mr = hex.AddComponent<MeshRenderer>();
            mr.material = GlobalDataTypes.Instance.TestHexMaterial;
            mr.material.SetColor("_BaseColor", Color.blue);

            if (ControlPoints.Contains(g))
                mr.material.SetColor("_BaseColor", Color.red);

            if (addToList)
                VisualGrids.Add(hex);


            GameObject line = Instantiate(RoadLine);
            line.transform.position = g.PosV3;
            Vector3 g0 = grids[i - 1].PosV3;
            Vector3 g1 = grids[i].PosV3;
            Vector3 g2 = grids[i + 1].PosV3;

            Vector3 p0 = g0 + (g1 - g0) / 2;
            Vector3 p1 = g1;
            Vector3 p2 = g1 + (g2 - g1) / 2;

            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.positionCount = 11;
            float offset = 1f / (lr.positionCount - 1);
            Vector3[] positions = new Vector3[lr.positionCount];
            for (int j = 0; j < lr.positionCount; j++)
            {
                positions[j] = GlobalDataTypes.BezierCurve(p0, p1, p2, offset * j);
            }

            lr.startWidth = LineSize;
            lr.endWidth = LineSize;

            lr.SetPositions(positions);

            if (addToList)
                VisualGrids.Add(line);
        }
    }

    // we either edit or add a control point here
    public bool TryControlPoint(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);

        for (int i = 1; i < ControlPoints.Count - 1; i++)
        {
            if (grid == ControlPoints[i])
            {
                ControlPointIndex = i;
                return true;
            }
        }

        int index = 1;
        // can not edit the start and end point
        for (int i = 1; i < CurrentTrack.Count - 1; i++)
        {
            if (ControlPoints.Contains(CurrentTrack[i]))
                index++;

            if (grid == CurrentTrack[i])
            {
                ControlPoints.Insert(index, grid);
                ControlPointIndex = index;
                if (!RedrawPath())
                {
                    ControlPoints.RemoveAt(index);
                    return false;
                }
                return true;
            }
        }

        return false;
    }

    public void UpdateControlPoint(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid.StationData == null && grid.CrossData == null && !ControlPoints.Contains(grid))
        {
            GridData.GridSave temp = ControlPoints[ControlPointIndex];
            ControlPoints[ControlPointIndex] = grid;

            if (!RedrawPath())
                ControlPoints[ControlPointIndex] = temp;
        }
    }

    /// <summary>
    /// current grid is already in the tracks
    /// </summary>
    /// <param name="currentGrid"></param>
    /// <param name="targetGrid"></param>
    /// <param name="constraints"></param>
    /// <param name="tracks"></param>
    /// <returns></returns>
    public bool FindPath(GridData.GridSave currentGrid, GridData.GridSave targetGrid, List<GridData.GridSave> constraints, List<GridData.GridSave> tracks)
    {
        recursiveCount++;
        if (recursiveCount > maxRecursion)
            return false;
        // find six neighbours
        int[] neighbors = GlobalDataTypes.FindNeighbors(currentGrid);

        // traverse neightbors
        List<GridData.GridSave> grids = new List<GridData.GridSave>();
        for (int i = 0; i < neighbors.Length; i++)
            grids.Add(GridData.Instance.GridDatas[neighbors[i]]);

        // sort grids by distance to target
        DistanceCompare dc = new DistanceCompare(targetGrid.PosV3);
        grids.Sort(dc);

        // traverse grids
        foreach (GridData.GridSave grid in grids)
        {
            // check grid condition
            if (constraints.Contains(grid))
                continue; // no overlap
            if (tracks.Contains(grid))
                continue; // no overlap
            bool addDetour = false;
            if (Vector3.Distance(targetGrid.PosV3, grid.PosV3) > Vector3.Distance(currentGrid.PosV3, targetGrid.PosV3))
            {
                CurrentDetour++;
                addDetour = true;
                if (CurrentDetour > maxDetour)
                    continue;
            }

            tracks.Add(grid);

            if (grid == targetGrid)
                return true;
            else if (grid.StationData == null && grid.CrossData == null)
            {
                if (FindPath(grid, targetGrid, constraints, tracks))
                    return true;
            }

            tracks.RemoveAt(tracks.Count - 1);
            if (addDetour)
                CurrentDetour--;
        }

        return false;
    }

    private class DistanceCompare : IComparer<GridData.GridSave>
    {
        private Vector3 Point;
        public DistanceCompare(Vector3 point)
        {
            Point = point;
        }

        public int Compare(GridData.GridSave x, GridData.GridSave y)
        {
            return (int)(Vector3.Distance(x.PosV3, Point) - Vector3.Distance(y.PosV3, Point));
        }
    }

    public void SelectRoadLevel()
    {
        if (CurrentTrack.Count > 1)
            HudManager.Instance.RoadBuild();
        else
            CancelRoad();
    }

    public void FinishRoad(int level = 0)
    {
            // add or edit existing road
            List<int> newPath = new List<int>();
            foreach (GridData.GridSave grid in CurrentTrack)
                newPath.Add(grid.Index);

            GameObject parent = new GameObject();
            parent.transform.position = Vector3.zero;
            foreach (GameObject visual in VisualGrids)
                visual.transform.SetParent(parent.transform);
            bool replace = false;
            for (int i = 0; i < AllRoads.Count; i++)
            {
                List<int> road = AllRoads[i];
                if ((road[0] == newPath[0] && road[road.Count - 1] == newPath[newPath.Count - 1]) || (road[0] == newPath[newPath.Count - 1] && road[road.Count - 1] == newPath[0]))
                {
                    // edit existing path
                    GameObject old = AllVisuals[i];
                    Destroy(old);
                    AllVisuals[i] = parent;
                    AllRoads[i] = newPath;
                    RoadLevels[i] = level;
                    replace = true;
                    SetRoadColor(i, GlobalDataTypes.TrackRarityColors[level]);
                    break;
                }
            }

            if (!replace)
            {
                AllRoads.Add(newPath);
                AllVisuals.Add(parent.gameObject);
                RoadLevels.Add(level);

                SetRoadColor(AllRoads.Count - 1, GlobalDataTypes.TrackRarityColors[level]);
            }
            TrainManager.Instance.AddOrUpdateConnection(newPath[0], newPath[newPath.Count - 1], AllRoads.Count - 1);
            
            for (int i = parent.transform.childCount - 1; i >= 0; i --)
            {
                LineRenderer lr = parent.transform.GetChild(i).GetComponent<LineRenderer>();
                if (!lr)
                    Destroy(parent.transform.GetChild(i).gameObject);
                else
                {
                    // add a mesh collider
                    lr.useWorldSpace = false;
                    MeshCollider mc = lr.gameObject.AddComponent<MeshCollider>();
                    Mesh mesh = new Mesh();
                    lr.BakeMesh(mesh, Camera.main, false);
                    Vector3[] vertices = mesh.vertices;
                    for (int k = 0; k < vertices.Length; k++)
                        vertices[k] -= lr.transform.position;
                    mesh.vertices = vertices;
                    mc.sharedMesh = mesh;
                    lr.useWorldSpace = true;

                    lr.gameObject.layer = LayerMask.NameToLayer("Road");
                }
            }

            EconManager.Instance.MoneyCount -= EconManager.GetPathCost(CurrentTrack, level);

        
        

        VisualGrids.Clear();
        CurrentTrack.Clear();

        InputManager.Instance.ExitRoadMode();
    }

    public void CancelRoad()
    {
        // destory visual clues if not in edit mode
        for (int i = VisualGrids.Count - 1; i >= 0; i--)
            Destroy(VisualGrids[i]);

        VisualGrids.Clear();
        CurrentTrack.Clear();

        InputManager.Instance.ExitRoadMode();
    }

    public void SetRoadColor(int roadIndex, Color color)
    {
        Transform tran = AllVisuals[roadIndex].transform;

        /*
        MeshRenderer[] mrs = tran.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs)
            mr.material.SetColor("_BaseColor", color);
        */

        LineRenderer[] lrs = tran.GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer lr in lrs)
        {
            lr.startColor = color;
            lr.endColor = color;
        }
    }

    private float LineSize = 2f;
    public void UpdateRoadSize(float mult)
    {
        LineSize = 2 + 6 * mult;
        foreach (GameObject parent in AllVisuals)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                SizeHelper(parent.transform.GetChild(i), mult);
            }
        }

        foreach (GameObject parent in VisualGrids)
        {
            SizeHelper(parent.transform, mult);
        }
    }

    private void SizeHelper(Transform obj, float mult)
    {
        LineRenderer lr = obj.GetComponent<LineRenderer>();
        if (lr)
        {
            lr.SetWidth(LineSize, LineSize);
            obj.GetComponent<LineRenderer>().startWidth = LineSize;
            obj.GetComponent<LineRenderer>().endWidth = LineSize;
        }

    }

    public int CacheLevel { get { return RoadLevels[RoadCache]; } }

    public int RoadCache;
    public int ObjectToIndex(GameObject obj)
    {
        for (int i = 0; i < AllVisuals.Count; i ++)
        {
            if (obj == AllVisuals[i])
            {
                RoadCache = i;
                return i;
            }
        }

        return -1;
    }

    public void UpgradeRoad()
    {
        List<GridData.GridSave> grids = new List<GridData.GridSave>();
        foreach (int index in AllRoads[RoadCache])
        {
            grids.Add(GridData.Instance.GridDatas[index]);
        }

        EconManager.Instance.MoneyCount -= EconManager.GetPathUpgradeCost(grids, RoadLevels[RoadCache]);
        RoadLevels[RoadCache]++;

        SetRoadColor(RoadCache, GlobalDataTypes.TrackRarityColors[RoadLevels[RoadCache]]);

        RoadCache = -1;
        InputManager.Instance.ExitSelectionMode();
    }

    public void ClearCache()
    {
        if (RoadCache != -1)
        {
            SetRoadColor(RoadCache, GlobalDataTypes.TrackRarityColors[RoadLevels[RoadCache]]);
            RoadCache = -1;
        }

        for (int i = CacheHighLight.childCount - 1; i >= 0; i--)
            Destroy(CacheHighLight.GetChild(i).gameObject);
    }

    private Transform CacheHighLight;

    public void HighLightCache()
    {
        for (int i = 0; i < AllRoads[RoadCache].Count; i++)
        {
            GridData.GridSave g = GridData.Instance.GridDatas[AllRoads[RoadCache][i]];

            GameObject hex = new GameObject();
            hex.transform.position = g.PosV3;
            MeshFilter mf = hex.AddComponent<MeshFilter>();
            mf.mesh = GlobalDataTypes.GetHexagonMesh();
            MeshRenderer mr = hex.AddComponent<MeshRenderer>();
            mr.material = GlobalDataTypes.Instance.TestHexMaterial;
            mr.material.SetColor("_BaseColor", Color.blue);

            hex.transform.parent = CacheHighLight;
        }
    }
}

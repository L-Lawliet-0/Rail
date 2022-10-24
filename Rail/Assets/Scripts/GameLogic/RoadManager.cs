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
        CurrentTrack = new List<GridData.GridSave>();
        VisualGrids = new List<GameObject>();
        ControlPoints = new List<GridData.GridSave>();
    }

    public GridData.GridSave StartGrid; // the start grid of this draw
    public GridData.GridSave CurrentGrid;

    private List<GridData.GridSave> CurrentTrack;
    private List<GameObject> VisualGrids;
    private List<GridData.GridSave> ControlPoints; // the points that used to control the flow of the track
    private int ControlPointIndex;

    public void InitData(GridData.GridSave grid)
    {
        StartGrid = CurrentGrid = grid;
        CurrentTrack.Clear();
        ControlPoints.Clear();

        CurrentTrack.Add(CurrentGrid);
    }

    public bool TryStartDraw(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid == CurrentGrid)
        {
            Debug.LogError("Drawing!");
            return true;
        }
        else
            return false;
    }

    public void DrawUpdate(Vector3 worldPos, bool erase = false)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);

        if (!erase && grid != CurrentGrid) // we only check new grid
        {
            if (Vector3.Distance(worldPos, CurrentGrid.PosV3) < 6)
                return;

            if (Vector3.Distance(grid.PosV3, CurrentGrid.PosV3) > 11)
            {
                Debug.LogError("User is drawing too fast");
                return;
            }

            if (CurrentTrack.Contains(grid))
            {
                Debug.LogError("User is drawing a loop");
                return;
            }

            CurrentTrack.Add(grid);
            CurrentGrid = grid;

            if (grid.StationData != null || grid.CrossData != null)
            {
                InputManager.Instance.ExitDrawMode();
            }

            // create a hexagon to keep track the track
            GameObject hex = new GameObject();
            hex.transform.position = grid.PosV3;
            MeshFilter mf = hex.AddComponent<MeshFilter>();
            mf.mesh = GlobalDataTypes.GetHexagonMesh();
            MeshRenderer mr = hex.AddComponent<MeshRenderer>();
            mr.material = GlobalDataTypes.Instance.TestHexMaterial;
            mr.material.SetColor("_BaseColor", Color.gray);

            VisualGrids.Add(hex);

            int index = VisualGrids.Count - 2;
            if (index >= 0)
            {
                MeshRenderer temp = VisualGrids[index].GetComponent<MeshRenderer>();
                temp.material.SetColor("_BaseColor", Color.blue);
            }
        }
        else if (erase && grid == CurrentGrid && CurrentTrack.Count > 1)
        {
            CurrentTrack.RemoveAt(CurrentTrack.Count - 1);
            CurrentGrid = CurrentTrack[CurrentTrack.Count - 1];

            GameObject obj = VisualGrids[VisualGrids.Count - 1];
            VisualGrids.RemoveAt(VisualGrids.Count - 1);
            Destroy(obj);

            if (VisualGrids.Count > 0)
            {
                MeshRenderer temp = VisualGrids[VisualGrids.Count - 1].GetComponent<MeshRenderer>();
                temp.material.SetColor("_BaseColor", Color.gray);
            }
        }

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

        for (int i = 1; i < CurrentTrack.Count - 1; i++)
        {
            GridData.GridSave g = CurrentTrack[i];
            
            GameObject hex = new GameObject();
            hex.transform.position = g.PosV3;
            MeshFilter mf = hex.AddComponent<MeshFilter>();
            mf.mesh = GlobalDataTypes.GetHexagonMesh();
            MeshRenderer mr = hex.AddComponent<MeshRenderer>();
            mr.material = GlobalDataTypes.Instance.TestHexMaterial;
            mr.material.SetColor("_BaseColor", Color.blue);

            if (ControlPoints.Contains(g))
                mr.material.SetColor("_BaseColor", Color.red);
            VisualGrids.Add(hex);
            

            GameObject line = Instantiate(RoadLine);
            line.transform.position = g.PosV3;
            Vector3 g0 = CurrentTrack[i - 1].PosV3;
            Vector3 g1 = CurrentTrack[i].PosV3;
            Vector3 g2 = CurrentTrack[i + 1].PosV3;

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
            lr.SetPositions(positions);

            VisualGrids.Add(line);
        }

        return true;
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
}

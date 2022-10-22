using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    private static RoadManager m_Instance;
    public static RoadManager Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
        CurrentTrack = new List<GridData.GridSave>();
        VisualGrids = new List<GameObject>();
    }

    public GridData.GridSave StartGrid; // the start grid of this draw
    public GridData.GridSave CurrentGrid;

    private List<GridData.GridSave> CurrentTrack;
    private List<GameObject> VisualGrids;

    public void InitData(GridData.GridSave grid)
    {
        StartGrid = CurrentGrid = grid;
        CurrentTrack.Clear();

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
}

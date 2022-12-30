using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// the purpose of this class is to make a good trailer
/// Trailer sequence -> scene starts with camera zoom into a train, follow the train for a few seconds,
/// then zoom to another train and follow it for a few seconds
/// then camera move around through out the scene to show different built lines
/// then finally it zoom out and show the whole picture of chinese map
/// </summary>
public class TrailerMaker : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine("ConstructNewRail");
    }

    private IEnumerator ConstructNewRail()
    {
        yield return new WaitForSeconds(1); // wait for everything to initialize

        // each city has a station build chances of .83f to get a station
        Dictionary<int, List<int>> cityGrids = new Dictionary<int, List<int>>();
        List<GridData.GridSave> stations = new List<GridData.GridSave>();
        for (int i = 0; i < GridData.Instance.GridDatas.Count; i++)
        {
            GridData.GridSave grid = GridData.Instance.GridDatas[i];
            if (!grid.name.Equals("sea"))
            {
                int city = CityManager.Instance.GridToCity[grid.Index];
                if (!cityGrids.ContainsKey(city))
                    cityGrids.Add(city, new List<int>());
                cityGrids[city].Add(grid.Index);
            }
        }

        foreach (KeyValuePair<int, List<int>> pair in cityGrids)
        {
            Debug.LogError("iteration");
            GridData.GridSave grid = GridData.Instance.GridDatas[pair.Value[Random.Range(0, pair.Value.Count)]];
            GameMain.Instance.HighLightGrid = grid;
            GameMain.Instance.TryBuildStation(0);

            stations.Add(grid);
            yield return null;
        }

        HashSet<RoadUnit> roads = new HashSet<RoadUnit>();
        // build some road between stations
        for (int i = 0; i < stations.Count; i++)
        {
            DisSort sort = new DisSort(stations[i]);

            List<GridData.GridSave> options = new List<GridData.GridSave>();
            options.AddRange(stations);
            options.Remove(stations[i]);

            options.Sort(sort);

            for (int k = 0; k < 3; k++)
            {
                RoadUnit ru = new RoadUnit();
                ru.g1 = stations[i];
                ru.g2 = options[k];
                roads.Add(ru);
            }
        }

        int cnt = 0;
        int bigCount = 1000 / (int)(3f / Time.fixedDeltaTime);
        foreach (RoadUnit ru in roads)
        {
            RoadManager.Instance.InitData(ru.g1);
            RoadManager.Instance.TryEndPoint(ru.g2.PosV3);
            RoadManager.Instance.FinishRoad();

            cnt++;
            if (cnt > bigCount)
            {
                yield return new WaitForFixedUpdate();
                cnt = 0;
            }
        }

        for (int j = 0; j < 3; j++)
        {
            // create the first train and follow its position
            List<GridData.GridSave> paths = new List<GridData.GridSave>();
            GridData.GridSave start = stations[Random.Range(0, stations.Count)];
            // start from this station, go to thers
            paths.Add(start);

            for (int i = 0; i < 3; i++)
            {
                foreach (RoadUnit rd in roads)
                {
                    if (rd.g1 == start || rd.g2 == start)
                    {
                        GridData.GridSave other = rd.g1 == start ? rd.g2 : rd.g1;
                        if (!paths.Contains(other))
                        {
                            start = other;
                            paths.Add(start);
                            break;
                        }

                    }
                }
            }

            TrainManager.Instance.Init();
            TrainManager.Instance.CurrentPath = paths;
            TrainManager.Instance.FinishTrain(0);

            TrainManager.TrainData td = TrainManager.Instance.AllTrains[TrainManager.Instance.AllTrains.Count - 1];
            yield return new WaitForSeconds(.1f);

            float zoomSpeed = 1000;
            if (j > 0)
                zoomSpeed = 100;
            float zoomScale = 150f - j * 50;
            float moveSpeed = 500;
            // now we use camera to floow the train for some times
            while (true)
            {
                if (CameraController.Instance.CurrentOrthographicSize > zoomScale)
                    CameraController.Instance.CurrentOrthographicSize -= Time.deltaTime * zoomSpeed;
                if (CameraController.Instance.CurrentOrthographicSize < zoomScale)
                    CameraController.Instance.CurrentOrthographicSize = zoomScale;

                Vector3 direction = td.TrainSprite.position - CameraController.Instance.transform.position;
                direction.z = 0;
                direction = direction.normalized;

                CameraController.Instance.transform.position += direction * moveSpeed * Time.deltaTime;

                Vector3 newDir = td.TrainSprite.position - CameraController.Instance.transform.position;
                newDir.z = 0;
                newDir = newDir.normalized;

                if (Vector3.Dot(newDir, direction) < 0)
                    CameraController.Instance.transform.position = new Vector3(td.TrainSprite.position.x, td.TrainSprite.position.y, CameraController.Instance.transform.position.z);

                Vector3 pos1 = CameraController.Instance.transform.position;
                Vector3 pos2 = td.TrainSprite.position;
                pos1.z = pos2.z = 0;

                if (CameraController.Instance.CurrentOrthographicSize <= zoomScale && Vector3.Distance(pos1, pos2) < 1)
                    break;

                yield return null;
            }

            float counter = 5f;
            while (counter > 0)
            {
                counter -= Time.deltaTime;
                CameraController.Instance.transform.position = new Vector3(td.TrainSprite.position.x, td.TrainSprite.position.y, CameraController.Instance.transform.position.z);
                yield return null;
            }

        }

        // make some trains
        for (int j = 0; j < 497; j++)
        {
            // create the first train and follow its position
            List<GridData.GridSave> paths = new List<GridData.GridSave>();
            GridData.GridSave start = stations[Random.Range(0, stations.Count)];
            // start from this station, go to thers
            paths.Add(start);

            for (int i = 0; i < 2; i++)
            {
                foreach (RoadUnit rd in roads)
                {
                    if (rd.g1 == start || rd.g2 == start)
                    {
                        GridData.GridSave other = rd.g1 == start ? rd.g2 : rd.g1;
                        if (!paths.Contains(other))
                        {
                            start = other;
                            paths.Add(start);
                            break;
                        }

                    }
                }
            }

            TrainManager.Instance.Init();
            TrainManager.Instance.CurrentPath = paths;
            TrainManager.Instance.FinishTrain(0);

            yield return null;
        }

        // move camera around for the last time
        for (int j = 0; j < 3; j++)
        {
            GridData.GridSave start = stations[Random.Range(0, stations.Count)];
            float zoomSpeed = 100;
            float zoomScale = 50f + (j + 1) * 100;
            float moveSpeed = 500;
            // now we use camera to floow the train for some times
            while (true)
            {
                if (CameraController.Instance.CurrentOrthographicSize < zoomScale)
                    CameraController.Instance.CurrentOrthographicSize += Time.deltaTime * zoomSpeed;
                if (CameraController.Instance.CurrentOrthographicSize > zoomScale)
                    CameraController.Instance.CurrentOrthographicSize = zoomScale;

                Vector3 direction = start.PosV3 - CameraController.Instance.transform.position;
                direction.z = 0;
                direction = direction.normalized;

                CameraController.Instance.transform.position += direction * moveSpeed * Time.deltaTime;

                Vector3 newDir = start.PosV3 - CameraController.Instance.transform.position;
                newDir.z = 0;
                newDir = newDir.normalized;

                if (Vector3.Dot(newDir, direction) < 0)
                    CameraController.Instance.transform.position = new Vector3(start.PosV3.x, start.PosV3.y, CameraController.Instance.transform.position.z);

                Vector3 pos1 = CameraController.Instance.transform.position;
                Vector3 pos2 = start.PosV3;
                pos1.z = pos2.z = 0;

                if (CameraController.Instance.CurrentOrthographicSize >= zoomScale && Vector3.Distance(pos1, pos2) < 1)
                    break;

                yield return null;
            }

            float counter = 5f;
            while (counter > 0)
            {
                counter -= Time.deltaTime;
                CameraController.Instance.transform.position = new Vector3(start.PosV3.x, start.PosV3.y, CameraController.Instance.transform.position.z);
                yield return null;
            }
        }

        Debug.LogError("Done!!!");
    }

    public class RoadUnit
    {
        public GridData.GridSave g1, g2;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            RoadUnit other = (RoadUnit)obj;

            if (other == null)
                return false;

            return (other.g1 == g2 && other.g2 == g1) || (other.g1 == g1 && other.g2 == g2);
        }
    }

    public class DisSort : IComparer<GridData.GridSave>
    {
        private Vector3 Point;
        public DisSort(GridData.GridSave origin)
        {
            Point = origin.PosV3;
        }

        public int Compare(GridData.GridSave x, GridData.GridSave y)
        {
            return (int)(Vector3.Distance(Point, x.PosV3) - Vector3.Distance(Point, y.PosV3));
        }
    }
}

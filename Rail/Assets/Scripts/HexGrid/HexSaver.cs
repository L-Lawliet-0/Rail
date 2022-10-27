using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static GridData;

[ExecuteInEditMode]
public class HexSaver : MonoBehaviour
{
    public bool SaveObjects;
    public bool LoadObjects;

    [System.Serializable]
    public class HexInfo
    {
        public float posX, posY;
        public string name;

        public HexInfo(float x, float y, string hexName)
        {
            posX = x;
            posY = y;
            name = hexName;
        }
    }

    void Update()
    {
       
        if (SaveObjects)
        {
            string dataPath = Application.dataPath + "/HexInfos";
            List<HexInfo> datas = new List<HexInfo>();
            for (int i = 0; i < transform.childCount; i++)
                datas.Add(new HexInfo(transform.GetChild(i).position.x, transform.GetChild(i).position.y, transform.GetChild(i).name));

            using (Stream file = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, datas);
            }

            // clear all objects
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            SaveObjects = false;
        }

        if (LoadObjects)
        {
            string dataPath = Application.dataPath + "/HexInfos";
            List<HexInfo> datas;

            using (Stream file = File.Open(dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                datas = bf.Deserialize(file) as List<HexInfo>;
            }

            // let's generate some hexagons
            foreach (HexInfo hi in datas)
            {
                GameObject obj = new GameObject();
                obj.transform.position = new Vector3(hi.posX, hi.posY);
                obj.transform.name = hi.name;
                obj.transform.SetParent(transform);
            }

            /*
            dataPath = Application.dataPath + "/GridDatas";
            List<GridData.GridSave> GridDatas;
            using (Stream file = File.Open(dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                GridDatas = bf.Deserialize(file) as List<GridSave>;
            }

            int width = 7200;
            int height = 5200;

            Texture2D t2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
            float xOffset = 7200f / width;
            float yOffset = 5200f / height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // fill the texture
                    GridSave grid = GetNearbyGrid(new Vector3(xOffset * x, yOffset * y), GridDatas);
                    if (!grid.name.Equals("sea"))
                        t2d.SetPixel(x, y, new Color(244f / 255f, 245f / 255f, 247f / 255f));
                    else
                        t2d.SetPixel(x, y, Color.clear);
                }
            }
            t2d.Apply();
            byte[] bytes = t2d.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Cmap.png", bytes);
            */

            LoadObjects = false;
        }
    }

    private GridSave GetNearbyGrid(Vector3 worldPos, List<GridData.GridSave> GridDatas)
    {
        float x = worldPos.x;
        float y = worldPos.y;
        worldPos += new Vector3(8016, 1933);

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
        int value = candidates[1];
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] >= 0 && candidates[i] < GridDatas.Count)
            {
                float distance = Vector3.Distance(worldPos, GridDatas[candidates[i]].PosV3);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    value = candidates[i];
                }
            }
        }

        return GridDatas[value];
    }

    public Material TestMat;
}

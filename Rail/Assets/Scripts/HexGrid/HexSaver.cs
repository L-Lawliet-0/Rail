using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

            LoadObjects = false;
        }
    }
}

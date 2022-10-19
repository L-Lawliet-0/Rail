using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;

// used to generate city and province name
[ExecuteInEditMode]
public class NamesGenerator : MonoBehaviour
{
    public bool GenerateName;
    public bool ClearAll;
    public bool TrimShi;
    public bool AdjustPosition;

    public bool SaveInfo;
    public bool LoadInfo;

    public GameObject TextPrefab;
    public Transform CityParent;

    [System.Serializable]
    private class TextInfo
    {
        public float posX, posY;
        public float sizeX, sizeY;
        public int fontSize;
        public string text;
        public string name;
    }

    void Update()
    {
        if (GenerateName)
        {
            HashSet<string> generatedNames = new HashSet<string>();
            int provinceCount = 0;

            for (int i = 0; i < CityParent.childCount; i++)
            {
                string province = CityParent.GetChild(i).name.Split(',')[0];
                string city = CityParent.GetChild(i).name.Split(',')[1];

                if (!generatedNames.Contains(province))
                {
                    GameObject obj = Instantiate(TextPrefab);
                    obj.transform.SetParent(transform);
                    obj.transform.SetSiblingIndex(provinceCount);
                    provinceCount++;

                    Text txt = obj.GetComponent<Text>();
                    txt.text = province;

                    RectTransform rect = obj.GetComponent<RectTransform>();
                    Collider2D collider = CityParent.GetChild(i).GetComponent<Collider2D>();
                    rect.position = collider.bounds.center;
                    rect.sizeDelta = new Vector2(collider.bounds.size.x, collider.bounds.size.y);

                    obj.transform.name = province;
                    generatedNames.Add(province);
                }

                if (!generatedNames.Contains(city))
                {
                    GameObject obj = Instantiate(TextPrefab);
                    obj.transform.SetParent(transform);

                    Text txt = obj.GetComponent<Text>();
                    txt.text = city;

                    RectTransform rect = obj.GetComponent<RectTransform>();
                    Collider2D collider = CityParent.GetChild(i).GetComponent<Collider2D>();
                    rect.position = collider.bounds.center;
                    rect.sizeDelta = new Vector2(collider.bounds.size.x, collider.bounds.size.y);

                    obj.transform.name = city;

                    generatedNames.Add(city);
                }
            }

            GenerateName = false;
        }

        if (ClearAll)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            ClearAll = false;
        }

        if (TrimShi)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Text>().text = transform.GetChild(i).name.Split('市')[0];
            }

            TrimShi = false;
        }

        if (AdjustPosition)
        {
            Dictionary<string, Collider2D> bestFits = new Dictionary<string, Collider2D>();
            for (int i = 0; i < CityParent.childCount; i++)
            {
                string province = CityParent.GetChild(i).name.Split(',')[0];
                string city = CityParent.GetChild(i).name.Split(',')[1];
                Collider2D collider = CityParent.GetChild(i).GetComponent<Collider2D>();

                if (bestFits.ContainsKey(province) && bestFits[province].bounds.size.magnitude < collider.bounds.size.magnitude)
                    bestFits[province] = collider;
                else if (!bestFits.ContainsKey(province))
                    bestFits.Add(province, collider);

                if (bestFits.ContainsKey(city) && bestFits[city].bounds.size.magnitude < collider.bounds.size.magnitude)
                    bestFits[city] = collider;
                else if (!bestFits.ContainsKey(city))
                    bestFits.Add(city, collider);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                if (bestFits.ContainsKey(transform.GetChild(i).name))
                {
                    RectTransform rect = transform.GetChild(i).GetComponent<RectTransform>();
                    rect.position = bestFits[transform.GetChild(i).name].bounds.center;
                    rect.sizeDelta = bestFits[transform.GetChild(i).name].bounds.size;
                }
            }

            AdjustPosition = false;
        }

        if (SaveInfo)
        {
            string dataPath = Application.dataPath + "/TextInfos";
            List<TextInfo> datas = new List<TextInfo>();
            for (int i = 0; i < transform.childCount; i++)
            {
                TextInfo data = new TextInfo();
                RectTransform rect = transform.GetChild(i).GetComponent<RectTransform>();
                Text txt = transform.GetChild(i).GetComponent<Text>();
                data.posX = rect.position.x;
                data.posY = rect.position.y;
                data.sizeX = rect.sizeDelta.x;
                data.sizeY = rect.sizeDelta.y;
                data.text = txt.text;
                data.fontSize = txt.fontSize;
                data.name = transform.GetChild(i).name;

                datas.Add(data);
            }

            using (Stream file = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, datas);
            }

            // clear all objects
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            SaveInfo = false;
        }

        if (LoadInfo)
        {
            string dataPath = Application.dataPath + "/TextInfos";
            List<TextInfo> datas;

            using (Stream file = File.Open(dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                datas = bf.Deserialize(file) as List<TextInfo>;
            }

            // let's generate some hexagons
            foreach (TextInfo ti in datas)
            {
                GameObject obj = Instantiate(TextPrefab);
                RectTransform rect = obj.GetComponent<RectTransform>();
                Text txt = obj.GetComponent<Text>();

                obj.transform.SetParent(transform);
                rect.position = new Vector3(ti.posX, ti.posY);
                rect.sizeDelta = new Vector2(ti.sizeX, ti.sizeY);
                txt.text = ti.text;
                obj.transform.name = ti.name;
                txt.fontSize = ti.fontSize;
                txt.resizeTextForBestFit = false;
            }

            LoadInfo = false;
        }
    }
}

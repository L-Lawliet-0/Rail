using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// used to generate city and province name
[ExecuteInEditMode]
public class NamesGenerator : MonoBehaviour
{
    public bool GenerateName;
    public bool ClearAll;

    public GameObject TextPrefab;
    public Transform CityParent;

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
    }
}

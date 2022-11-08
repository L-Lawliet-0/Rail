using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityNamesParent : MonoBehaviour
{
    private static CityNamesParent m_Instance;
    public static CityNamesParent Instance { get { return m_Instance; } }

    public GameObject IndexPrefab;

    private void Awake()
    {
        m_Instance = this;
    }

    public void ActivateNames(int index, float sizeMult = 1f)
    {
        float fontBase = 16;
        if (index == 0)
            fontBase = 48;

        float fontAdd = 20f;
        if (index == 0)
            fontAdd = 24;

        for (int i = 0; i < 2; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == index);

            if (i == index)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Text>().fontSize = (int)(fontBase + fontAdd * sizeMult);
                    transform.GetChild(i).GetChild(j).GetComponent<Text>().color = Color.black;
                }
            }
            
        }
    }

    public void HideText(bool value)
    {
        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
            text.enabled = !value;
    }

    public GameObject CreateTrainIndex(string text, Vector3 gridPos ,GameObject parent = null)
    {
        if (parent == null)
        {
            parent = new GameObject();
            RectTransform parentRect = parent.gameObject.AddComponent<RectTransform>();
            parentRect.SetParent(transform);
            parentRect.localPosition = Vector3.zero;
        }

        GameObject obj = Instantiate(IndexPrefab);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent.transform);
        rect.position = gridPos + new Vector3(10, 10) + rect.GetSiblingIndex() * new Vector3(24, 0);
        rect.GetComponent<Text>().text = text;

        return parent;
    }

    public Vector3 FindMatchCityPosition(string trimmedCityName)
    {
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            if (transform.GetChild(1).GetChild(i).name.Equals(trimmedCityName))
                return transform.GetChild(1).GetChild(i).position;
        }

        return Vector3.zero;
    }
}

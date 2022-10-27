using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityNamesParent : MonoBehaviour
{
    private static CityNamesParent m_Instance;
    public static CityNamesParent Instance { get { return m_Instance; } }

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

        for (int i = 0; i < transform.childCount; i++)
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
}

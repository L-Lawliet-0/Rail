using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityNamesParent : MonoBehaviour
{
    private static CityNamesParent m_Instance;
    public static CityNamesParent Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }

    public void ActivateNames(int index)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(i == index);
    }
}

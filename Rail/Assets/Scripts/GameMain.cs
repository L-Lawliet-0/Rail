using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private static GameMain m_Instance;
    public static GameMain Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }

    public GameObject BorderLine, ProvinceLine, CityLine;
}

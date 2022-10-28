using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconManager : MonoBehaviour
{
    private static EconManager m_Instance;
    public static EconManager Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }


}

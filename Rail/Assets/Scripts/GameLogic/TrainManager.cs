using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    private static TrainManager m_Instance;
    public static TrainManager Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconManager : MonoBehaviour
{
    private static EconManager m_Instance;
    public static EconManager Instance { get { return m_Instance; } }

    public EconData EconData;

    private void Awake()
    {
        m_Instance = this;

        float minGDP = float.MaxValue;
        float maxGDP = float.MinValue;
        float totalPopulation = 0;

        foreach (CityData cd in EconData.Sheet1)
        {
            minGDP = Mathf.Min(minGDP, cd.GDP);
            maxGDP = Mathf.Max(maxGDP, cd.GDP);
            totalPopulation += cd.Population;
        }

        Debug.LogError("Total Population: " + totalPopulation);
        Debug.LogError("Max GDP: " + maxGDP);
        Debug.LogError("Min GDP: " + minGDP);
    }


}

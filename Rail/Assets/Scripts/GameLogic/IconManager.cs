using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    private static IconManager m_Instance;
    public static IconManager Instance { get { return m_Instance; } }

    private Dictionary<int, GameObject> StationIcons;

    private void Awake()
    {
        m_Instance = this;
        StationIcons = new Dictionary<int, GameObject>();
    }

    public void AddOrUpdateIcon(int key, GameObject newObj)
    {
        if (StationIcons.ContainsKey(key))
        {
            Destroy(StationIcons[key]);
            StationIcons[key] = newObj;
        }
        else
            StationIcons.Add(key, newObj);
    }

    public void UpdateSize(float mult)
    {
        foreach (KeyValuePair<int, GameObject> pair in StationIcons)
        {
            pair.Value.transform.localScale = Vector3.one * 2 + Vector3.one * 30f * mult;
        }
    }
}

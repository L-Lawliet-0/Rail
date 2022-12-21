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

    public void SwapIconTexture(int key, Sprite sprite)
    {
        if (StationIcons.ContainsKey(key))
            StationIcons[key].GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void ChangeIconColor(int key, Color color)
    {
        if (StationIcons.ContainsKey(key))
            StationIcons[key].GetComponent<SpriteRenderer>().color = color;
    }

    public void UpdateSize(float mult)
    {
        foreach (KeyValuePair<int, GameObject> pair in StationIcons)
        {
            pair.Value.transform.localScale = Vector3.one * 1 + Vector3.one * 15f * mult;
        }
    }

    public void ClearAllIcon()
    {
        foreach (KeyValuePair<int, GameObject> pair in StationIcons)
        {
            Destroy(pair.Value);
        }

        StationIcons.Clear();
    }
}

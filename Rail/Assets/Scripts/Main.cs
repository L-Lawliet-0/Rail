using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private static Main m_Instance;
    public static Main Instance { get { return m_Instance; } }

    public bool IsPlaying; // is this game in play mode
    public GameObject MainPanel, SLpanel;

    private void Awake()
    {
        m_Instance = this;
        IsPlaying = false;
    }

    public void StartNewGame()
    {
        MainPanel.gameObject.SetActive(false);
        IsPlaying = true;
    }
}

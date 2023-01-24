using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        RefreshSLpanel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                MainPanel.gameObject.SetActive(true);
            }
            else if (SLpanel.gameObject.activeInHierarchy)
            {
                SLpanel.gameObject.SetActive(false);
                MainPanel.gameObject.SetActive(true);
            }
            else
            {
                ForcePlay();
            }
        }
    }

    public void ForcePlay()
    {
        IsPlaying = true;
        MainPanel.gameObject.SetActive(false);
        SLpanel.gameObject.SetActive(false);
    }

    public void StartNewGame()
    {
        MainPanel.gameObject.SetActive(false);
        IsPlaying = true;
    }

    public void Save()
    {
        SLpanel.gameObject.SetActive(true);
        MainPanel.gameObject.SetActive(false);
        SetSaveText();
    }

    public void SetSaveText()
    {
        SLpanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Choose a save slot to save your data";
        for (int i = 1; i < 6; i++)
        {
            SLpanel.transform.GetChild(i).GetChild(8).GetChild(0).GetComponent<Text>().text = "Save";
            Button btn = SLpanel.transform.GetChild(i).GetChild(8).GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int slot = i;
            btn.onClick.AddListener(() => SaveControl.Instance.Save(slot));
        }
    }

    public void Load()
    {
        SLpanel.gameObject.SetActive(true);
        MainPanel.gameObject.SetActive(false);
        SetLoadText();
    }

    public void SetLoadText()
    {
        SLpanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "Choose a save slot to load your data";
        for (int i = 1; i < 6; i++)
        {
            SLpanel.transform.GetChild(i).GetChild(8).GetChild(0).GetComponent<Text>().text = "Load";
            Button btn = SLpanel.transform.GetChild(i).GetChild(8).GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int slot = i;
            btn.onClick.AddListener(() => SaveControl.Instance.Load(slot));
        }
    }

    public class SaveSummary
    {
        public int StationCount, TrainCount, TrackLength, WeekCount;
    }

    // refresh the sl panel
    public void RefreshSLpanel(int checkSlot = -1)
    {
        for (int i = 1; i < 6; i++)
        {
            if (checkSlot == -1 || i == checkSlot)
            {

                SaveSummary ss = SaveControl.Instance.TryReadSave(i);
                if (ss == null)
                {
                    for (int j = 1; j < 8; j += 2)
                    {
                        SLpanel.transform.GetChild(i).GetChild(j).GetComponent<Text>().text = "-";
                    }
                }
                else
                {
                    SLpanel.transform.GetChild(i).GetChild(1).GetComponent<Text>().text = ss.StationCount.ToString();
                    SLpanel.transform.GetChild(i).GetChild(3).GetComponent<Text>().text = ss.TrainCount.ToString();
                    SLpanel.transform.GetChild(i).GetChild(5).GetComponent<Text>().text = ss.TrackLength + "km";
                    SLpanel.transform.GetChild(i).GetChild(7).GetComponent<Text>().text = "Week " + ss.WeekCount;
                }
            }
        }
    }
}

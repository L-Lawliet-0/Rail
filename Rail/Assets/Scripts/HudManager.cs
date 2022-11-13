using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    private static HudManager m_Instance;
    public static HudManager Instance { get { return m_Instance; } }

    private void Awake()
    {
        m_Instance = this;
        SetEmpty();
        AllButtons = new RectTransform[]
        {
            BuildStation, BuildCross, BuildTrack, PlaceTrain, ConfirmRoad, CancelRoad, ConfirmTrain, CancelTrain
        };
    }

    public RectTransform ButtonsParent;
    public Text GridInfo;

    // a bunch of preset buttons
    public RectTransform BuildStation, BuildCross, BuildTrack, PlaceTrain, ConfirmRoad, CancelRoad, ConfirmTrain, CancelTrain;
    private RectTransform[] AllButtons;

    // don't show anything
    public void SetEmpty()
    {
        GridInfo.transform.parent.gameObject.SetActive(false);
        ButtonsParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// show the hud needed for right click
    /// </summary>
    /// <param name="grid"></param>
    public void RightClickHud(GridData.GridSave grid)
    {
        GridInfo.text = grid.name;
        GridInfo.transform.parent.gameObject.SetActive(true);

        List<RectTransform> activeButtons = new List<RectTransform>();

        BuildStation.GetChild(0).GetComponent<Text>().text = "Build Station";
        BuildCross.GetChild(0).GetComponent<Text>().text = "Build Cross";

        if (grid.StationData == null && grid.CrossData == null)
        {
            activeButtons.Add(BuildStation);
            activeButtons.Add(BuildCross);

            BuildStation.GetChild(0).GetComponent<Text>().text += " (" + EconManager.Instance.GetStationCrossCost(grid, false) + ")";
            BuildCross.GetChild(0).GetComponent<Text>().text += " (" + EconManager.Instance.GetStationCrossCost(grid, true) + ")";
        }

        if (grid.StationData != null || grid.CrossData != null)
        {
            activeButtons.Add(BuildTrack);
            activeButtons.Add(PlaceTrain);
        }

        ShowButtons(activeButtons);
    }

    public void TrackMode(GridData.GridSave grid)
    {
        GridInfo.text = grid.name;
        GridInfo.transform.parent.gameObject.SetActive(true);
        List<RectTransform> activeButtons = new List<RectTransform>();

        ConfirmRoad.GetChild(0).GetComponent<Text>().text = "Confirm Road";
        activeButtons.Add(ConfirmRoad);
        activeButtons.Add(CancelRoad);

        ShowButtons(activeButtons);
    }

    public void UpdateRoadCost(int cost)
    {
        Text text = ConfirmRoad.GetChild(0).GetComponent<Text>();
        text.text = text.text.Split('(')[0] + " (" + cost + ")";
    }

    public void TrainMode(GridData.GridSave grid)
    {
        GridInfo.text = grid.name;
        GridInfo.transform.parent.gameObject.SetActive(true);
        List<RectTransform> activeButtons = new List<RectTransform>();

        activeButtons.Add(ConfirmTrain);
        activeButtons.Add(CancelTrain);

        ShowButtons(activeButtons);
    }

    public void ShowButtons(List<RectTransform> buttons)
    {
        foreach (RectTransform rect in AllButtons)
            rect.gameObject.SetActive(false);

        float leftBound = -160 * buttons.Count / 2;
        ButtonsParent.sizeDelta = new Vector2(160 * buttons.Count, ButtonsParent.sizeDelta.y);

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].localPosition = new Vector3(leftBound + 80 + i * 160, 0);
        }
        
        ButtonsParent.gameObject.SetActive(true);
    }
}

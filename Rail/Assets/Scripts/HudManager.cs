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
            BuildStation, BuildCross, BuildTrack, PlaceTrain, ConfirmRoad, CancelRoad, ConfirmTrain, CancelTrain, UpgradeBtn, RepathBtn, PriceAdjustor
        };
        PriceAdjustor.GetComponentInChildren<Slider>().minValue = GlobalDataTypes.MinTrainPrice;
        PriceAdjustor.GetComponentInChildren<Slider>().maxValue = GlobalDataTypes.MaxTrainPrice;
    }

    public RectTransform ButtonsParent;
    public Text GridInfo;

    // a bunch of preset buttons
    public RectTransform BuildStation, BuildCross, BuildTrack, PlaceTrain, ConfirmRoad, CancelRoad, ConfirmTrain, CancelTrain, UpgradeBtn, RepathBtn, PriceAdjustor;
    private RectTransform[] AllButtons;

    // don't show anything
    public void SetEmpty()
    {
        GridInfo.transform.parent.gameObject.SetActive(false);
        ButtonsParent.gameObject.SetActive(false);
        LevelPromote.gameObject.SetActive(false);
    }

    /// <summary>
    /// show the hud needed for right click
    /// </summary>
    /// <param name="grid"></param>
    public void RightClickHud(GridData.GridSave grid, GameObject road = null, GameObject train = null)
    {
        GridInfo.text = grid.name.Replace(',', ' ');
        GridInfo.transform.parent.gameObject.SetActive(true);

        List<RectTransform> activeButtons = new List<RectTransform>();

        BuildStation.GetChild(0).GetComponent<Text>().text = "Build Station";
        BuildCross.GetChild(0).GetComponent<Text>().text = "Build Cross";

        if (grid.StationData == null && grid.CrossData == null)
        {
            activeButtons.Add(BuildStation);
            activeButtons.Add(BuildCross);

            //BuildStation.GetChild(0).GetComponent<Text>().text += " (" + EconManager.Instance.GetStationCrossCost(grid, false) + ")";
            BuildCross.GetChild(0).GetComponent<Text>().text += " (" + EconManager.GetCrossCost(grid) + ")";

            if (train)
            {
                if (TrainManager.Instance.AllTrains[TrainManager.Instance.TrainCache].Level < GlobalDataTypes.MaxLevel)
                {
                    UpgradeBtn.GetChild(0).GetComponent<Text>().text = "Upgrade Train (" + GlobalDataTypes.TrainUpgradePrices[TrainManager.Instance.AllTrains[TrainManager.Instance.TrainCache].Level] + ")";
                    Button upgrade = UpgradeBtn.GetComponent<Button>();
                    upgrade.onClick.RemoveAllListeners();
                    upgrade.onClick.AddListener(() => { TrainManager.Instance.UpgradeTrain(); });
                    activeButtons.Add(UpgradeBtn);
                }
                activeButtons.Add(RepathBtn);
                PriceAdjustor.GetComponentInChildren<Slider>().value = TrainManager.Instance.AllTrains[TrainManager.Instance.TrainCache].TrainPrice;
                activeButtons.Add(PriceAdjustor);
            }
            else if (road)
            {
                UpgradeBtn.GetChild(0).GetComponent<Text>().text = "Upgrade Road (" + EconManager.GetPathUpgradeCost(RoadManager.Instance.AllRoads[RoadManager.Instance.RoadCache], RoadManager.Instance.CacheLevel) + ")";
                Button upgrade = UpgradeBtn.GetComponent<Button>();
                upgrade.onClick.RemoveAllListeners();
                upgrade.onClick.AddListener(() => { RoadManager.Instance.UpgradeRoad(); });
                activeButtons.Add(UpgradeBtn);
            }
        }

        if (grid.StationData != null || grid.CrossData != null)
        {
            activeButtons.Add(BuildTrack);
            activeButtons.Add(PlaceTrain);

            if (grid.CrossData != null)
            {
                UpgradeBtn.GetChild(0).GetComponent<Text>().text = "Upgrade to station (" + EconManager.GetCrossUpgradeCost(grid) + ")";
                Button upgrade = UpgradeBtn.GetComponent<Button>();
                upgrade.onClick.RemoveAllListeners();
                upgrade.onClick.AddListener(() => { GameMain.Instance.UpgradeCrossToStation(grid); });
                activeButtons.Add(UpgradeBtn);
            }
            else if (grid.StationData.Level < GlobalDataTypes.MaxLevel)
            {
                UpgradeBtn.GetChild(0).GetComponent<Text>().text = "Upgrade Station (" + EconManager.GetStationUpgradeCost(grid, grid.StationData.Level) + ")";
                Button upgrade = UpgradeBtn.GetComponent<Button>();
                upgrade.onClick.RemoveAllListeners();
                upgrade.onClick.AddListener(() => { GameMain.Instance.UpgradeStation(grid); });
                activeButtons.Add(UpgradeBtn);
            }
        }

        ShowButtons(activeButtons);
    }

    public void TrackMode(GridData.GridSave grid)
    {
        GridInfo.text = grid.name.Replace(',', ' ');
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
        GridInfo.text = grid.name.Replace(',', ' ');
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

    public Transform LevelPromote;

    public void StationBuild()
    {
        SetEmpty();

        LevelPromote.gameObject.SetActive(true);
        LevelPromote.GetChild(0).GetComponent<Text>().text = "Select the level of station you want to build";

        // update the buttons
        for (int i = 1; i < 6; i++)
        {
            Button btn = LevelPromote.GetChild(i).GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int level = i - 1;
            btn.onClick.AddListener( () => { GameMain.Instance.TryBuildStation(level); });

            btn.transform.GetChild(0).GetComponent<Text>().text = "Level " + level + " (" + EconManager.GetStationCost(GameMain.Instance.HighLightGrid, level) + ")";
        }

        Button cancel = LevelPromote.GetChild(6).GetComponent<Button>();
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() => { InputManager.Instance.ExitSelectionMode(); });
    }

    public void RoadBuild()
    {
        SetEmpty();

        LevelPromote.gameObject.SetActive(true);
        LevelPromote.GetChild(0).GetComponent<Text>().text = "Select the level of track you want to build";

        // update the buttons
        for (int i = 1; i < 6; i++)
        {
            Button btn = LevelPromote.GetChild(i).GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int level = i - 1;
            btn.onClick.AddListener(() => { RoadManager.Instance.FinishRoad(level); });

            btn.transform.GetChild(0).GetComponent<Text>().text = "Level " + level + " (" + EconManager.GetPathCost(RoadManager.Instance.CurrentTrack, level) + ")";
        }

        Button cancel = LevelPromote.GetChild(6).GetComponent<Button>();
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() => { RoadManager.Instance.CancelRoad(); });
    }

    public void TrainBuild()
    {
        SetEmpty();

        LevelPromote.gameObject.SetActive(true);
        LevelPromote.GetChild(0).GetComponent<Text>().text = "Select the level of train you want to build";

        // update the buttons
        for (int i = 1; i < 6; i++)
        {
            Button btn = LevelPromote.GetChild(i).GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int level = i - 1;
            btn.onClick.AddListener(() => { TrainManager.Instance.FinishTrain(level); });

            btn.transform.GetChild(0).GetComponent<Text>().text = "Level " + level + " (" + GlobalDataTypes.TrainPrices[level] + ")";
        }

        Button cancel = LevelPromote.GetChild(6).GetComponent<Button>();
        cancel.onClick.RemoveAllListeners();
        cancel.onClick.AddListener(() => { TrainManager.Instance.CancelTrain(); });
    }
}

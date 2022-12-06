using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicInfo : MonoBehaviour
{
    private static DynamicInfo m_Instance;
    public static DynamicInfo Instance { get { return m_Instance; } }
    public Sprite CitySprite, TrainSprite, StationSprite, TrackSprite;

    private CanvasGroup m_Canvas;

    private Image Icon;
    private Text NameText, Title1, Content1, Title2, Content2;

    private void Awake()
    {
        m_Instance = this;
        m_Canvas = GetComponent<CanvasGroup>();

        Icon = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        NameText = transform.GetChild(0).GetChild(1).GetComponent<Text>();

        Title1 = transform.GetChild(1).GetChild(0).GetComponent<Text>();
        Content1 = transform.GetChild(1).GetChild(1).GetComponent<Text>();

        Title2 = transform.GetChild(2).GetChild(0).GetComponent<Text>();
        Content2 = transform.GetChild(2).GetChild(1).GetComponent<Text>();

        Hide();
    }

    public void Activate(GridData.GridSave grid, int roadIndex = -1, TrainManager.TrainData train = null)
    {
        if (grid.name.Equals("sea"))
            return;

        // track
        NameText.text = grid.name.Split(',')[1];
        if (roadIndex != -1)
        {
            Icon.sprite = TrackSprite;
            Title1.text = "Speed : ";
            Content1.text = GlobalDataTypes.Speeds[RoadManager.Instance.RoadLevels[roadIndex]].ToString();
            Title2.text = "Length : ";
            Content2.text = (RoadManager.Instance.AllRoads[roadIndex].Count - 1) * 10 + " km";
        }
        else if (train != null)
        {
            NameText.text = train.TrainName;
            Icon.sprite = TrainSprite;
            Title1.text = "Speed : ";
            Content1.text = GlobalDataTypes.Speeds[train.Level].ToString();
            Title2.text = "Capacity : ";
            Content2.text = train.CurrentCapacity() + " / " + train.Capacity;
        }
        else
        {
            if (grid.StationData != null)
            {
                Icon.sprite = StationSprite;
                Title1.text = "Max Capacity : ";
                Content1.text = grid.StationData.Capacity.ToString();
                Title2.text = "Current Capacity : ";
                Content2.text = grid.StationData.CurrentCount().ToString();
            }
            else
            {
                Icon.sprite = CitySprite;
                CityData city = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]];
                Title1.text = "GDP per capita : ";
                Content1.text = (city.GDP * 10000).ToString();
                Title2.text = "Population : ";
                Content2.text = city.ResidentPopulation.ToString();
            }
        }

        //m_Canvas.alpha = 1;
        GetComponent<Animation>().Play("ShiftRight");
    }

    public void Hide()
    {
        //m_Canvas.alpha = 0;
        GetComponent<Animation>().Play("ShiftLeft");
    }

}

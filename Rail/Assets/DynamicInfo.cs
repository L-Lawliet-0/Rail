using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicInfo : MonoBehaviour
{
    private static DynamicInfo m_Instance;
    public static DynamicInfo Instance { get { return m_Instance; } }
    public Sprite CitySprite, TrainSprite, StationSprite, TrackSprite, SpeedIcon, TrackLengthIcon, TrainCapacityIcon, MaxCapIcon, CurrentCapIcon, PopulationIcon, GDPicon;

    private CanvasGroup m_Canvas;

    private Image Icon, Icon1, Icon2;
    private Text NameText, Content1, Content2;

    public Image CityPic;

    private List<Sprite> CitySprites;

    private void Awake()
    {
        m_Instance = this;
        m_Canvas = GetComponent<CanvasGroup>();

        Icon = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        NameText = transform.GetChild(0).GetChild(1).GetComponent<Text>();

        Icon1 = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        Content1 = transform.GetChild(1).GetChild(1).GetComponent<Text>();

        Icon2 = transform.GetChild(2).GetChild(0).GetComponent<Image>();
        Content2 = transform.GetChild(2).GetChild(1).GetComponent<Text>();

        Hide();

        CitySprites = new List<Sprite>();
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
            Icon1.sprite = SpeedIcon;
            Content1.text = GlobalDataTypes.Speeds[RoadManager.Instance.RoadLevels[roadIndex]].ToString() + "km/h";
            Icon2.sprite = TrackLengthIcon;
            Content2.text = (RoadManager.Instance.AllRoads[roadIndex].Count - 1) * 10 + " km";
        }
        else if (train != null)
        {
            NameText.text = train.TrainName;
            Icon.sprite = TrainSprite;
            Icon1.sprite = SpeedIcon;
            Content1.text = GlobalDataTypes.Speeds[train.Level].ToString() + "km/h";
            Icon2.sprite = TrainCapacityIcon;
            Content2.text = train.CurrentCapacity() + " / " + train.Capacity;
        }
        else
        {
            if (grid.StationData != null)
            {
                Icon.sprite = StationSprite;
                Icon1.sprite = MaxCapIcon;
                Content1.text = grid.StationData.Capacity.ToString() + "(Max)";
                Icon2.sprite = CurrentCapIcon;
                Content2.text = grid.StationData.CurrentCount().ToString() + "(Current)";
            }
            else
            {
                Icon.sprite = CitySprite;
                CityData city = CityManager.Instance.CityDatas[CityManager.Instance.GridToCity[grid.Index]];
                Icon1.sprite = GDPicon;
                Content1.text = (city.GDP * 10000).ToString() + " rmb/person";
                Icon2.sprite = PopulationIcon;
                Content2.text = city.ResidentPopulation.ToString() + " residents";
            }
        }

        //m_Canvas.alpha = 1;
        GetComponent<Animation>().Play("ShiftRight");

        Resources.UnloadUnusedAssets();

        for (int i = CitySprites.Count - 1; i >= 0; i--)
            Destroy(CitySprites[i]);

        CitySprites.Clear();
        for (int i = 1; i < 6; i++)
        {
            // picture time
            Sprite og = Resources.Load<Sprite>("CityPics/" + grid.name.Split(",")[1] + "/" + i.ToString());
            int length = Mathf.Min(og.texture.width, og.texture.height);
            Texture2D corp = new Texture2D(length, length);
            if (og.texture.width > og.texture.height)
            {
                int offset = (og.texture.width - og.texture.height) / 2;
                corp.SetPixels(og.texture.GetPixels(offset, 0, length, length)); // corp the middle part
            }
            else
                corp.SetPixels(og.texture.GetPixels(0, 0, length, length));
            corp.Apply();

            Sprite final = Sprite.Create(corp, new Rect(0, 0, length, length), Vector2.one * .5f);
            CitySprites.Add(final);
        }
        StartCoroutine("FadePicture");
    }

    public static void PicTest(string city)
    {
        Resources.UnloadUnusedAssets();
        for (int i = 1; i < 6; i++)
        {
            Sprite og = Resources.Load<Sprite>("CityPics/" + city + "/" + i.ToString());
            if (og == null || og.texture == null)
                Debug.LogError("Error : " + city);
            og.texture.GetPixels(0, 0, og.texture.width, og.texture.height);
        }
    }

    public void Hide()
    {
        //m_Canvas.alpha = 0;
        if (GetComponent<RectTransform>().anchoredPosition.x > -128)
            GetComponent<Animation>().Play("ShiftLeft");
        StopAllCoroutines();
    }

    private IEnumerator FadePicture()
    {
        int currentIndex = Random.Range(0, 5);
        float fadeTime = 2f;
        float transparency = .2f;

        TOP:
        CityPic.sprite = CitySprites[currentIndex];
        CityPic.color = new Color(1, 1, 1, transparency);
        float counter = fadeTime;

        // show image
        while (counter > 0)
        {
            CityPic.color += Color.black * Time.deltaTime / fadeTime * (1 - transparency);
            counter -= Time.deltaTime;
            yield return null;
        }

        CityPic.color = Color.white;
        yield return new WaitForSeconds(3); // display time

        // fade away
        counter = fadeTime;
        while (counter > 0)
        {
            CityPic.color -= Color.black * Time.deltaTime / fadeTime * (1 - transparency);
            counter -= Time.deltaTime;
            yield return null;
        }

        List<int> indexRange = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (i != currentIndex)
                indexRange.Add(i);
        }
        currentIndex = indexRange[Random.Range(0, indexRange.Count)];

        goto TOP;
    }

}

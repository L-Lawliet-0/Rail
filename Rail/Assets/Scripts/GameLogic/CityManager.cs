using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    private static CityManager m_Instance;
    public static CityManager Instance { get { return m_Instance; } }

    public EconData InitialCityData;
    public List<CityData> CityDatas;

    private float Proportions = 5f / 1400f;

    public Transform TravelPanel;

    private void Awake()
    {
        m_Instance = this;

        CityDatas = new List<CityData>();
        foreach (CityData cd in InitialCityData.Sheet1)
        {
            CityData city = new CityData();
            city.CityName = cd.CityName;
            city.GDP = cd.GDP;
            city.ResidentPopulation = (int)(cd.Population * 10000);
            city.VisitorPopulation = new Dictionary<int, int>();
            CityDatas.Add(city);
        }

        
    }

    private void Start()
    {
        // Debug.LogError("City count : " + CityNamesParent.Instance.transform.GetChild(1).childCount);

        for (int i = 0; i < CityDatas.Count; i++)
        {
            for (int j = 0; j < CityNamesParent.Instance.transform.GetChild(1).childCount; j++)
            {
                Transform tran = CityNamesParent.Instance.transform.GetChild(1).GetChild(j);
                string name = tran.name;

                if (name.Equals(CityDatas[i].CityName.Split(',')[1]))
                {
                    CityDatas[i].x = tran.position.x;
                    CityDatas[i].y = tran.position.y;
                    break;
                }
            }
        }

        CalculateTravelNeed();
        Clear();
    }

    // calculate and log travel need info
    // 
    //
    private List<List<TravelData>> TravelNeeds;
    public void CalculateTravelNeed()
    {
        TravelNeeds = new List<List<TravelData>>();
        List<float> TotalMults = new List<float>();
        for (int i = 0; i < CityDatas.Count; i++)
        {
            // randomlize the travel cities
            float totalMult = 0;
            //Debug.LogError(CityDatas[i].ResidentPopulation);
            float travelPopulation = Proportions * CityDatas[i].ResidentPopulation;
            //Debug.LogError(travelPopulation.ToString());

            List<TravelData> need = new List<TravelData>();
            for (int j = 0; j < CityDatas.Count; j++)
            {
                if (i != j)
                {
                    // calculate how many people need to transport from i to j
                    float targetGDP = CityDatas[j].GDP;
                    float distance = Vector3.Distance(CityDatas[i].posV3, CityDatas[j].posV3);

                    float mult = targetGDP / distance;
                    totalMult += mult;

                    TravelData td = new TravelData() { HomeCity = i, TargetCity = j, Mult = mult };

                    need.Add(td);
                }
                else
                {
                    TravelData td = new TravelData() { HomeCity = i, TargetCity = i, Mult = 0 };

                    need.Add(td);
                }
            }
            
            for (int j = 0; j < need.Count; j++)
            {
                // resident travel need
                need[j].Population = Mathf.FloorToInt(need[j].Mult / totalMult * travelPopulation);
            }

            need.Sort(new Tsorter());

            TravelNeeds.Add(need);
            TotalMults.Add(totalMult);
        }

        // visitor needs
        for (int i = 0; i < CityDatas.Count; i++)
        {
            List<int> keys = new List<int>(CityDatas[i].VisitorPopulation.Keys);
            foreach (int key in keys)
            {
                TravelData returns = new TravelData();
                returns.HomeCity = key;
                returns.TargetCity = key;
                returns.Population = Mathf.FloorToInt(CityDatas[i].VisitorPopulation[key] * .2f * .8f);


            }
        }

        float totalTravelCount = 0;
        foreach (List<TravelData> list in TravelNeeds)
        {
            foreach (TravelData tuple in list)
                totalTravelCount += tuple.Population;
        }

        Debug.LogError("This many people will travel this day: " + totalTravelCount);
    }

    public void OnGridClick(Vector3 worldPos)
    {
        GridData.GridSave grid = GridData.Instance.GetNearbyGrid(worldPos);
        if (grid.name.Equals("sea"))
            return;
        for (int i = 0; i < TravelNeeds.Count; i++)
        {
            if (CityDatas[i].CityName.Equals(grid.name))
            {
                float total = 0;
                for (int j = 0; j < TravelNeeds[i].Count; j++)
                {
                    total += TravelNeeds[i][j].Population;
                }
                TravelPanel.GetChild(0).GetComponent<Text>().text = total.ToString();
                for (int j = 1; j < 5; j++)
                {
                    TravelPanel.GetChild(j).GetComponent<Text>().text = CityDatas[TravelNeeds[i][TravelNeeds[i].Count - j].TargetCity].CityName + " : " + TravelNeeds[i][TravelNeeds[i].Count - j].Population;
                }
                break;
            }
        }

        TravelPanel.gameObject.SetActive(true);
    }

    public void Clear()
    {
        TravelPanel.gameObject.SetActive(false);
    }

    public class Tsorter : IComparer<TravelData>
    {
        public int Compare(TravelData x, TravelData y)
        {
            return x.Population.CompareTo(y.Population);
        }
    }
}

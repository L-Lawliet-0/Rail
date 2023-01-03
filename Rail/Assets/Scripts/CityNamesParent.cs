using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityNamesParent : MonoBehaviour
{
    private static CityNamesParent m_Instance;
    public static CityNamesParent Instance { get { return m_Instance; } }

    public GameObject IndexPrefab;

    private void Awake()
    {
        m_Instance = this;
        TrainDatas = new List<TrainManager.TrainData>();
        TrainObjs = new List<RectTransform>();
        GetComponent<Canvas>().sortingOrder = GlobalDataTypes.CitynamesOrder;
    }

    public void ActivateNames(int index, float sizeMult = 1f)
    {
        float fontBase = 16;
        if (index == 0)
            fontBase = 48;

        float fontAdd = 20f;
        if (index == 0)
            fontAdd = 24;

        for (int i = 0; i < 2; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == index);

            
            if (i == index)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Text>().fontSize = (int)(fontBase + fontAdd * sizeMult);
                    transform.GetChild(i).GetChild(j).GetComponent<Text>().color = Color.black;
                }
            }
            
        }
    }

    public void HideText(bool value)
    {
        for (int i = 0; i < 2; i++)
        {
            Text[] texts = transform.GetChild(i).GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
                text.enabled = !value;
        }
    }

    public GameObject CreateTrainIndex(string text, Vector3 gridPos ,GameObject parent = null)
    {
        if (parent == null)
        {
            parent = new GameObject();
            RectTransform parentRect = parent.gameObject.AddComponent<RectTransform>();
            parentRect.SetParent(transform);
            parentRect.localPosition = Vector3.zero;
        }

        GameObject obj = Instantiate(IndexPrefab);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent.transform);
        rect.position = gridPos + new Vector3(10, 10) + rect.GetSiblingIndex() * new Vector3(24, 0);
        rect.GetComponent<Text>().text = text;

        return parent;
    }

    public Vector3 FindMatchCityPosition(string trimmedCityName)
    {
        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            if (transform.GetChild(1).GetChild(i).name.Equals(trimmedCityName))
                return transform.GetChild(1).GetChild(i).position;
        }

        return Vector3.zero;
    }

    public List<TrainManager.TrainData> TrainDatas;
    public List<RectTransform> TrainObjs;
    public GameObject TrainObjPrefab;
    public void CreateTrainCounter(TrainManager.TrainData data)
    {
        TrainDatas.Add(data);
        // crate and add object
        GameObject obj = Instantiate(TrainObjPrefab);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(transform);
        TrainObjs.Add(rect);
    }

    public void ClearTrainCounter()
    {
        for (int i = 0; i < TrainObjs.Count; i++)
            Destroy(TrainObjs[i].gameObject);
        TrainObjs.Clear();
        TrainDatas.Clear();
    }

    public void UpdateTrainObjects(bool[] updates)
    {
        for (int i = 0; i < TrainObjs.Count; i++)
        {
            TrainObjs[i].position = TrainDatas[i].TrainSprite.position + Vector3.up * 6;

            if (updates[i])
            {
                for (int d = TrainObjs[i].childCount - 1; d >= 1; d--)
                    Destroy(TrainObjs[i].GetChild(d).gameObject);

                float population = TrainDatas[i].CurrentCapacity() / 100f;
                float decimial = population - Mathf.FloorToInt(population);
                int peopleCnt = Mathf.FloorToInt(population);

                Vector3 startPos = Vector3.left * 2.3f * peopleCnt / 2f;

                for (int c = 0; c < peopleCnt; c++)
                {
                    GameObject people = Instantiate(TrainObjs[i].GetChild(0).gameObject);
                    RectTransform rectTran = people.GetComponent<RectTransform>();
                    rectTran.SetParent(TrainObjs[i]);
                    rectTran.localPosition = startPos + Vector3.right * 2.3f * c;
                    people.SetActive(true);
                }

                if (decimial != 0)
                {
                    GameObject people = Instantiate(TrainObjs[i].GetChild(0).gameObject);
                    RectTransform rectTran = people.GetComponent<RectTransform>();
                    rectTran.SetParent(TrainObjs[i]);
                    rectTran.localPosition = startPos + Vector3.right * peopleCnt * 2.3f;
                    people.SetActive(true);
                    people.GetComponent<Image>().fillAmount = decimial;
                };
            }
        }
    }

    public GameObject BoardInfo;
    public void ShowBoardInfo(TrainManager.TrainData train)
    {
        GameObject board = Instantiate(BoardInfo);
        board.transform.parent = transform;

        board.gameObject.SetActive(true);
        board.transform.position = train.TrainSprite.position + Vector3.right * 140 * board.transform.localScale.x;
        Text t_Unboard = board.transform.GetChild(1).GetComponent<Text>();
        Text t_Board = board.transform.GetChild(2).GetComponent<Text>();
        Text t_Money = board.transform.GetChild(4).GetComponent<Text>();

        t_Unboard.text = "-" + train.TotalUnboard;
        t_Board.text = "+" + train.TotalBoard;
        t_Money.text = "+" + train.TotalMoney;

        StartCoroutine("FadeDestroy", board.GetComponent<CanvasGroup>());
    }

    private IEnumerator FadeDestroy(CanvasGroup cg)
    {
        yield return new WaitForSeconds(.5f);

        float counter = 2.5f;

        while (counter > 0)
        {
            cg.alpha = counter;
            counter -= Time.deltaTime;
            yield return null;
        }

        Destroy(cg.gameObject);
    }
}

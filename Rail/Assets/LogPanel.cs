using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogPanel : MonoBehaviour
{
    private static LogPanel m_Instance;
    public static LogPanel Instance { get { return m_Instance; } }

    public Transform Content;

    public List<string> messages;
    public List<Text> Texts;

    private void Awake()
    {
        m_Instance = this;
        messages = new List<string>();
        Texts = new List<Text>();
    }

    private void Start()
    {
        // generate the text
        Texts.Add(Content.GetChild(0).GetChild(0).GetComponent<Text>());
        for (int i = 1; i < 20; i++)
        {
            GameObject line = Instantiate(Content.GetChild(0).gameObject);
            line.transform.parent = Content;
            line.transform.localPosition = Content.GetChild(0).localPosition;
            line.transform.localPosition -= Vector3.up * i * 32;
            Texts.Add(line.transform.GetChild(0).GetComponent<Text>());
            if (i % 2 == 1)
                line.GetComponent<Image>().color = new Color(100f / 255f, 100f / 255f, 100f / 255f);
        }
    }

    public void AppendMessage(string message)
    {
        if (messages.Count == Texts.Count)
            messages.RemoveAt(0);
        messages.Add(message);

        for (int i = 0; i < messages.Count; i++)
        {
            Texts[i].text = messages[i];
        }
    }
}

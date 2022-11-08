using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager m_Instance;
    public static TimeManager Instance { get { return m_Instance; } }
    public const float RealTimeToGameTime = 1440f;
    public const int DaySecs = 24 * 60 * 60;
    public const int HourSecs = 60 * 60;
    public float DayCounter = 0;
    public float HourCounter = 0;

    private void Awake()
    {
        m_Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // advance an hour
            DayCounter += 3601;
            HourCounter += 3601;
        }

        DayCounter += Time.deltaTime * RealTimeToGameTime;

        if (DayCounter >= DaySecs)
        {
            DayCounter -= DaySecs;
            // a day is passed
            // recalculate city travel needs;
            CityManager.Instance.CalculateTravelNeed();
        }

        HourCounter += Time.deltaTime * RealTimeToGameTime;

        if (HourCounter >= HourSecs)
        {
            HourCounter -= HourSecs;
            // an hour has passed
            // flush travel needs to station
            CityManager.Instance.FlushNeedsToStation();
        }
    }
}

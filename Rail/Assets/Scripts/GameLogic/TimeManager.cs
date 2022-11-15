using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    private static TimeManager m_Instance;
    public static TimeManager Instance { get { return m_Instance; } }
    public const float RealTimeToGameTime = 300;//1440f;
    public const int DaySecs = 24 * 60 * 60;
    public const int HourSecs = 60 * 60;
    public float DayCounter = 0;
    public float HourCounter = 0;

    public int MonthCount;
    public int DayCount;
    public int HourCount;
    public Text DayText;

    public int MonthlyGoal;

    public Text GoalTrackText;
    public int GoalTrack { get { return goalTrack; } set { UpdateGoalTrack(value); } }
    private void UpdateGoalTrack(int value)
    {
        goalTrack = value;
        GoalTrackText.text = goalTrack + " / " + MonthlyGoal + " people transported";
    }
    private int goalTrack;

    private void Awake()
    {
        m_Instance = this;
        DayCount = 1;
        HourCount = 0;
        MonthCount = 1;
        UpdateGoal();
        GoalTrack = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // advance an hour
            DayCounter += 3601;
            HourCounter += 3601;

            CityManager.Instance.FlushNeedsToStation();
        }

        DayCounter += Time.deltaTime * RealTimeToGameTime;

        if (DayCounter >= DaySecs)
        {
            DayCounter -= DaySecs;
            // a day is passed
            // recalculate city travel needs;
            CityManager.Instance.CalculateTravelNeed();

            DayCount++;
            if (DayCount > 30)
            {
                DayCount = 1;
                MonthCount++;
                // refresh goal
                UpdateGoal();
            }
            DayText.text = "Day " + DayCount + " Hour " + HourCount;
        }

        HourCounter += Time.deltaTime * RealTimeToGameTime;

        if (HourCounter >= HourSecs)
        {
            HourCounter -= HourSecs;
            // an hour has passed
            // flush travel needs to station
            CityManager.Instance.FlushNeedsToStation();
            HourCount++;
            if (HourCount > 23)
                HourCount = 0;

            DayText.text = "Day " + DayCount + " Hour " + HourCount;
        }
    }

    public Text GoalText;
    public void UpdateGoal()
    {
        MonthlyGoal = 5000 + 1000 * MonthCount;
        GoalText.text = "You need to transport " + MonthlyGoal + " people this month";
    }
}

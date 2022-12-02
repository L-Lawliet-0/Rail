using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class TimeManager : MonoBehaviour
{
    private static TimeManager m_Instance;
    public static TimeManager Instance { get { return m_Instance; } }
    public const float RealTimeToGameTime = 720f;
    public const int DaySecs = 24 * 60 * 60;
    public const int CycleDayCount = 7;
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

    public Text LastDayTraffic;
    public int LastDayTrafficCount;

    public Text LastDayIncome;
    public int LastDayIncomeCount;

    private void Awake()
    {
        m_Instance = this;
        DayCount = 1;
        HourCount = 0;
        MonthCount = 1;
    }

    private void Start()
    {
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
            EconManager.Instance.MoneyCount -= EconManager.Instance.DailySpend;
            CityManager.Instance.CalculateTravelNeed();
            LastDayTraffic.text = "Last Day Traffic : " + LastDayTrafficCount;
            LastDayIncome.text = "Last Day Income: " + LastDayIncomeCount;
            LastDayTrafficCount = 0;
            LastDayIncomeCount = 0;

            DayCount++;
            if (DayCount > CycleDayCount)
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
        if (MonthlyGoal > 0 && GoalTrack < MonthlyGoal)
        {
            Debug.LogError("!!!Game End!!! Fail monthly requirement");
        }

        GoalTrack = 0;
        if (MonthCount == 1)
            MonthlyGoal = GlobalDataTypes.Instance.ExpectedFirstMonthTraffic;
        else
            MonthlyGoal = GlobalDataTypes.Instance.ExpectedTraffic * MonthCount;
        GoalText.text = "You need to transport " + MonthlyGoal + " people this month";
    }
}

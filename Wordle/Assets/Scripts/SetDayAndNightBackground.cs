using System;
using UnityEngine;

public class SetDayAndNightBackground : MonoBehaviour
{
    [Header("Elements")]
    public GameObject backgroundDay;
    public GameObject backgroundNight;

    [Header("Day Hour")]
    public int dayHour = 6; 
    
    [Header("Night Hour")]
    public int nightHour = 19;

    void Start()
    {
        UpdateBackground();
    }

    void UpdateBackground()
    {
        int hour = DateTime.Now.Hour;
        
        if (hour >= dayHour && hour < nightHour)
        {
            DayMode();
        }
        else
        {
            NightMode();
        }
    }

    void DayMode()
    {
        if(backgroundDay != null) backgroundDay.SetActive(true);
        if(backgroundNight != null) backgroundNight.SetActive(false);
    }

    void NightMode()
    {
        if(backgroundDay != null) backgroundDay.SetActive(false);
        if(backgroundNight != null) backgroundNight.SetActive(true);
    }
}

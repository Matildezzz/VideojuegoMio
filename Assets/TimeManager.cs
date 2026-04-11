using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Fecha y hora actual")]
    [SerializeField] private int day = 1;
    [SerializeField] private int hour = 8;
    [SerializeField] private int minute = 0;

    [Header("Configuracion del tiempo")]
    [SerializeField] private float realSecondsPerGameMinute = 1f;

    private float timer;

    public int Day => day;
    public int Hour => hour;
    public int Minute => minute;

    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnDayChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NotifyTimeChanged();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        while (timer >= realSecondsPerGameMinute)
        {
            timer -= realSecondsPerGameMinute;
            AdvanceOneMinute();
        }
    }

    private void AdvanceOneMinute()
    {
        minute++;

        if (minute >= 60)
        {
            minute = 0;
            hour++;
        }

        if (hour >= 24)
        {
            hour = 0;
            day++;
            OnDayChanged?.Invoke(day);
        }

        NotifyTimeChanged();
    }

    private void NotifyTimeChanged()
    {
        OnTimeChanged?.Invoke(day, hour, minute);
    }

    public bool IsNight()
    {
        return hour >= 22 || hour < 6;
    }

    public bool IsDay()
    {
        return !IsNight();
    }

    public string GetFormattedTime()
    {
        return hour.ToString("00") + ":" + minute.ToString("00");
    }

    public void SetTime(int newDay, int newHour, int newMinute)
    {
        int previousDay = day;

        day = Mathf.Max(1, newDay);
        hour = Mathf.Clamp(newHour, 0, 23);
        minute = Mathf.Clamp(newMinute, 0, 59);
        timer = 0f;

        if (day != previousDay)
        {
            OnDayChanged?.Invoke(day);
        }

        NotifyTimeChanged();
    }

    public void AddMinutes(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AdvanceOneMinute();
        }
    }
}


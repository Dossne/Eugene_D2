using System;
using TriInspector;
using UnityEngine;

public enum TimerUnit
{
    Sec = 1,
    Ms = 2
}

[Serializable]
public class Timer
{
    [SerializeField, ReadOnly] private float maxTime = 0;
    [SerializeField, ReadOnly] private float timeCounter = 0;
    [SerializeField, ReadOnly] private TimerUnit unit;


    public Timer(float maxTime, TimerUnit unit = TimerUnit.Sec)
    {
        this.maxTime = maxTime;
        this.unit = unit;
        Reset();
    }


    public void SetCurTime(float t)
    {
        if (t <= maxTime && t > 0)
            timeCounter = t;

        else if (t > maxTime)
            timeCounter = maxTime;
        else
            timeCounter = 0;
    }


    public void Update(float deltaTime)
    {
        if (timeCounter > 0)
            timeCounter -= Mathf.Min(deltaTime * GetMultiplier(), timeCounter);
        else
            timeCounter = 0;
    }


    public void Reset(float maxTime, TimerUnit unit = TimerUnit.Sec)
    {
        this.maxTime = maxTime;
        this.unit = unit;
        Reset();
    }


    public void Reset()
    {
        timeCounter = maxTime;
    }


    public void SetOff()
    {
        timeCounter = 0;
    }

    
    /// <summary>
    /// Неявно обновляет время. Возвращает True, если таймер истёк
    /// </summary>
    public bool IsOffAfterUpdate(float deltaTime)
    {
        Update(deltaTime);
        return timeCounter == 0;
    }

    
    public bool IsOff() => timeCounter == 0;
    public float TimeRest() => timeCounter;
    public float TimeMax() => maxTime;
    public float TimeMaxSec() => maxTime / GetMultiplier();


    public float TimeRestPercent()
    {
        return timeCounter / maxTime * 100.0f;
    }


    public float TimeRestPart()
    {
        return timeCounter / maxTime;
    }


    public override string ToString()
    {
        string result;
        TimeSpan span = TimeSpan.FromSeconds(TimeRest() * GetMultiplier());

        if (span.TotalDays >= 1)
        {
            result = $"{span.Days}d{span.Hours}h";
        }
        else if (span.TotalHours >= 1)
        {
            result = $"{span.Hours}h{span.Minutes}m";
        }
        else if (span.TotalMinutes >= 1)
        {
            result = $"{span.Minutes}m{span.Seconds}s";
        }
        else
        {
            result = $"{Math.Round(span.TotalSeconds, 2).ToString()}s";
        }

        return $"Time rest: {result}";
    }


    private float GetMultiplier()
    {
        if (unit == TimerUnit.Ms)
            return 1000;
        
        return 1;
    }
}
using System;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static int CurrentDay { get; private set; } = 1;

    public static event Action<int> OnNewDay;

    [ContextMenu("Pasar al día siguiente")]
    public void NextDay()
    {
        CurrentDay++;
        OnNewDay?.Invoke(CurrentDay);
        Debug.Log("Nuevo día: " + CurrentDay);
    }
}
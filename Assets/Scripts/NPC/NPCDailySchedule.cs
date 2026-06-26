using System;
using System.Reflection;
using UnityEngine;

public sealed class NPCDailySchedule : MonoBehaviour
{
    [Header("Puntos del día")]
    [SerializeField] private Transform morningPoint;
    [SerializeField] private Transform middayPoint;
    [SerializeField] private Transform eveningPoint;
    [SerializeField] private Transform nightPoint;

    [Header("Horas de cambio")]
    [Range(0, 23)] [SerializeField] private int morningHour = 6;
    [Range(0, 23)] [SerializeField] private int middayHour = 12;
    [Range(0, 23)] [SerializeField] private int eveningHour = 20;
    [Range(0, 23)] [SerializeField] private int nightHour = 22;

    [Header("Movimiento")]
    [SerializeField] private bool teleportInsteadOfWalking = true;
    [SerializeField] private bool avoidRepeatingSamePoint = true;

    private Transform currentTarget;

    public void ApplyTime(int hour, int minute)
    {
        Transform target = GetTargetForHour(hour);
        MoveTo(target);
    }

    public void MoveToMiddayPoint()
    {
        MoveTo(middayPoint);
    }

    private Transform GetTargetForHour(int hour)
    {
        if (hour >= nightHour || hour < morningHour)
        {
            return nightPoint != null ? nightPoint : eveningPoint;
        }

        if (hour >= eveningHour)
        {
            return eveningPoint != null ? eveningPoint : middayPoint;
        }

        if (hour >= middayHour)
        {
            return middayPoint != null ? middayPoint : morningPoint;
        }

        return morningPoint;
    }

    private void MoveTo(Transform target)
    {
        if (target == null)
        {
            return;
        }

        if (avoidRepeatingSamePoint && currentTarget == target)
        {
            return;
        }

        currentTarget = target;

        if (!teleportInsteadOfWalking && TrySetDestination(target.position))
        {
            return;
        }

        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();

        if (rb2D != null)
        {
            rb2D.position = target.position;
            return;
        }

        transform.position = target.position;
    }

    private bool TrySetDestination(Vector3 position)
    {
        Component[] components = GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];

            if (component == null)
            {
                continue;
            }

            Type type = component.GetType();

            MethodInfo setDestination = type.GetMethod(
                "SetDestination",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Vector3) },
                null
            );

            if (setDestination != null)
            {
                setDestination.Invoke(component, new object[] { position });
                return true;
            }
        }

        return false;
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public enum ScheduledTimeEventKind
{
    StartDay,
    ShopsOpen,
    NpcMiddayMove,
    ShopsClose,
    NightWarning,
    MidnightPenalty,
    Custom
}

[Serializable]
public class ScheduledTimeEvent
{
    [Header("Identificador")]
    public string eventId = "evento_horario";
    public ScheduledTimeEventKind kind = ScheduledTimeEventKind.Custom;

    [Header("Hora")]
    [Range(0, 23)] public int hour;
    [Range(0, 59)] public int minute;
    public bool runOncePerDay = true;
    public bool active = true;

    [Header("Feedback")]
    [TextArea] public string toastMessage;
    public string soundName;

    [Header("Evento extra opcional")]
    public UnityEvent onTriggered;
}

public sealed class TimeEventController : MonoBehaviour
{
    public static TimeEventController Instance { get; private set; }

    [Header("Referencia al tiempo")]
    [SerializeField] private MonoBehaviour timeManager;

    [Header("Eventos horarios")]
    [SerializeField] private List<ScheduledTimeEvent> scheduledEvents = new List<ScheduledTimeEvent>();

    [Header("Tiendas")]
    [SerializeField] private bool controlShopOpeningHours = true;
    [SerializeField] private bool closeShopPanelWhenStoresClose = true;

    [Header("NPCs")]
    [SerializeField] private bool controlNpcSchedules = true;

    [Header("Penalización de medianoche")]
    [SerializeField] private GameObject tiredPenaltyTarget;
    [SerializeField] private int tiredEnergyPenalty = 15;
    [SerializeField] private bool warnIfPenaltyCouldNotBeApplied = false;

    private readonly HashSet<string> triggeredToday = new HashSet<string>();
    private int lastDay = -1;
    private int lastTotalMinutes = -1;
    private bool hasReadTime;

    private void Reset()
    {
        CreateDefaultEventsIfEmpty();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateDefaultEventsIfEmpty();
    }

    private void Start()
    {
        FindTimeManagerIfNeeded();
        UpdateFromCurrentTime(true);
    }

    private void Update()
    {
        UpdateFromCurrentTime(false);
    }

    private void UpdateFromCurrentTime(bool forceSyncOnly)
    {
        if (!TryReadCurrentTime(out int day, out int hour, out int minute))
        {
            return;
        }

        int currentTotalMinutes = GetTotalMinutes(hour, minute);

        if (controlShopOpeningHours)
        {
            ApplyShopOpeningHours(hour, minute);
        }

        if (controlNpcSchedules)
        {
            ApplyNpcSchedules(hour, minute);
        }

        if (!hasReadTime)
        {
            hasReadTime = true;
            lastDay = day;
            lastTotalMinutes = currentTotalMinutes;
            return;
        }

        if (day != lastDay)
        {
            triggeredToday.Clear();
        }

        if (forceSyncOnly || (day == lastDay && currentTotalMinutes == lastTotalMinutes))
        {
            lastDay = day;
            lastTotalMinutes = currentTotalMinutes;
            return;
        }

        TriggerEventsReached(day, lastTotalMinutes, currentTotalMinutes);

        lastDay = day;
        lastTotalMinutes = currentTotalMinutes;
    }

    private void TriggerEventsReached(int day, int previousTotalMinutes, int currentTotalMinutes)
    {
        for (int i = 0; i < scheduledEvents.Count; i++)
        {
            ScheduledTimeEvent timeEvent = scheduledEvents[i];

            if (timeEvent == null || !timeEvent.active)
            {
                continue;
            }

            int eventTotalMinutes = GetTotalMinutes(timeEvent.hour, timeEvent.minute);

            if (!HasReachedEventTime(previousTotalMinutes, currentTotalMinutes, eventTotalMinutes))
            {
                continue;
            }

            string key = day + "_" + GetEventKey(timeEvent, i);

            if (timeEvent.runOncePerDay && triggeredToday.Contains(key))
            {
                continue;
            }

            triggeredToday.Add(key);
            ExecuteTimeEvent(timeEvent);
        }
    }

    private bool HasReachedEventTime(int previousTotalMinutes, int currentTotalMinutes, int eventTotalMinutes)
    {
        if (previousTotalMinutes < 0)
        {
            return false;
        }

        if (currentTotalMinutes >= previousTotalMinutes)
        {
            return previousTotalMinutes < eventTotalMinutes && currentTotalMinutes >= eventTotalMinutes;
        }

        return eventTotalMinutes > previousTotalMinutes || eventTotalMinutes <= currentTotalMinutes;
    }

    private void ExecuteTimeEvent(ScheduledTimeEvent timeEvent)
    {
        switch (timeEvent.kind)
        {
            case ScheduledTimeEventKind.StartDay:
                ApplyShopOpeningHours(timeEvent.hour, timeEvent.minute);
                ShowToast(timeEvent.toastMessage);
                PlaySound(timeEvent.soundName);
                break;

            case ScheduledTimeEventKind.ShopsOpen:
                SetAllShopOpeningStates(true);
                ShowToast(timeEvent.toastMessage);
                PlaySound(timeEvent.soundName);
                break;

            case ScheduledTimeEventKind.NpcMiddayMove:
                MoveNpcsToMiddayPoint();
                ShowToast(timeEvent.toastMessage);
                PlaySound(timeEvent.soundName);
                break;

            case ScheduledTimeEventKind.ShopsClose:
                SetAllShopOpeningStates(false);
                CloseCurrentShopPanelIfNeeded();
                ShowToast(timeEvent.toastMessage);
                PlaySound(timeEvent.soundName);
                break;

            case ScheduledTimeEventKind.NightWarning:
                ShowToast(timeEvent.toastMessage);
                PlaySound(string.IsNullOrWhiteSpace(timeEvent.soundName) ? "Error" : timeEvent.soundName);
                break;

            case ScheduledTimeEventKind.MidnightPenalty:
                ApplyTiredPenalty();
                ShowToast(timeEvent.toastMessage);
                PlaySound(string.IsNullOrWhiteSpace(timeEvent.soundName) ? "Error" : timeEvent.soundName);
                break;

            default:
                ShowToast(timeEvent.toastMessage);
                PlaySound(timeEvent.soundName);
                break;
        }

        if (timeEvent.onTriggered != null)
        {
            timeEvent.onTriggered.Invoke();
        }
    }

    private void ApplyShopOpeningHours(int hour, int minute)
    {
        ShopOpeningHours[] shops = FindObjectsByType<ShopOpeningHours>(FindObjectsSortMode.None);

        for (int i = 0; i < shops.Length; i++)
        {
            if (shops[i] != null)
            {
                shops[i].ApplyTime(hour, minute, false);
            }
        }
    }

    private void SetAllShopOpeningStates(bool open)
    {
        ShopOpeningHours[] shops = FindObjectsByType<ShopOpeningHours>(FindObjectsSortMode.None);

        for (int i = 0; i < shops.Length; i++)
        {
            if (shops[i] != null)
            {
                shops[i].SetOpenState(open, false);
            }
        }
    }

    private void CloseCurrentShopPanelIfNeeded()
    {
        if (!closeShopPanelWhenStoresClose)
        {
            return;
        }

        Type shopControllerType = FindTypeByName("ShopController");

        if (shopControllerType == null)
        {
            return;
        }

        object instance = GetStaticInstance(shopControllerType, "Instance");

        if (instance == null)
        {
            return;
        }

        MethodInfo closeShopMethod = shopControllerType.GetMethod("CloseShop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (closeShopMethod != null)
        {
            closeShopMethod.Invoke(instance, null);
        }
    }

    private void ApplyNpcSchedules(int hour, int minute)
    {
        NPCDailySchedule[] schedules = FindObjectsByType<NPCDailySchedule>(FindObjectsSortMode.None);

        for (int i = 0; i < schedules.Length; i++)
        {
            if (schedules[i] != null)
            {
                schedules[i].ApplyTime(hour, minute);
            }
        }
    }

    private void MoveNpcsToMiddayPoint()
    {
        NPCDailySchedule[] schedules = FindObjectsByType<NPCDailySchedule>(FindObjectsSortMode.None);

        for (int i = 0; i < schedules.Length; i++)
        {
            if (schedules[i] != null)
            {
                schedules[i].MoveToMiddayPoint();
            }
        }
    }

    private void ApplyTiredPenalty()
    {
        GameObject target = tiredPenaltyTarget;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player;
            }
        }

        if (target == null)
        {
            if (warnIfPenaltyCouldNotBeApplied)
            {
                Debug.LogWarning("TimeEventController: no se encontró el Player para aplicar cansancio.");
            }

            return;
        }

        if (TrySendEnergyPenalty(target, tiredEnergyPenalty))
        {
            return;
        }

        if (warnIfPenaltyCouldNotBeApplied)
        {
            Debug.LogWarning("TimeEventController: no encontré un método compatible para bajar energía.");
        }
    }

    private bool TrySendEnergyPenalty(GameObject target, int amount)
    {
        Component[] components = target.GetComponents<Component>();

        string[] methodNames =
        {
            "SpendEnergy",
            "UseEnergy",
            "RemoveEnergy",
            "ConsumeEnergy",
            "DecreaseEnergy",
            "LoseEnergy"
        };

        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];

            if (component == null)
            {
                continue;
            }

            Type type = component.GetType();

            for (int m = 0; m < methodNames.Length; m++)
            {
                MethodInfo intMethod = type.GetMethod(methodNames[m], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);

                if (intMethod != null)
                {
                    intMethod.Invoke(component, new object[] { amount });
                    return true;
                }

                MethodInfo floatMethod = type.GetMethod(methodNames[m], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(float) }, null);

                if (floatMethod != null)
                {
                    floatMethod.Invoke(component, new object[] { (float)amount });
                    return true;
                }
            }

            MethodInfo addEnergyInt = type.GetMethod("AddEnergy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);

            if (addEnergyInt != null)
            {
                addEnergyInt.Invoke(component, new object[] { -amount });
                return true;
            }

            MethodInfo addEnergyFloat = type.GetMethod("AddEnergy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(float) }, null);

            if (addEnergyFloat != null)
            {
                addEnergyFloat.Invoke(component, new object[] { -(float)amount });
                return true;
            }
        }

        return false;
    }

    private void ShowToast(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Type toastManagerType = FindTypeByName("ToastManager");

        if (toastManagerType == null)
        {
            Debug.Log(message);
            return;
        }

        object instance = GetStaticInstance(toastManagerType, "Instance");

        if (instance == null)
        {
            Debug.Log(message);
            return;
        }

        MethodInfo showToast = toastManagerType.GetMethod("ShowToast", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);

        if (showToast != null)
        {
            showToast.Invoke(instance, new object[] { message });
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void PlaySound(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName))
        {
            return;
        }

        Type soundManagerType = FindTypeByName("SoundEffectManager");

        if (soundManagerType == null)
        {
            return;
        }

        MethodInfo playMethod = soundManagerType.GetMethod("Play", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null);

        if (playMethod != null)
        {
            playMethod.Invoke(null, new object[] { soundName, true });
            return;
        }

        playMethod = soundManagerType.GetMethod("Play", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);

        if (playMethod != null)
        {
            playMethod.Invoke(null, new object[] { soundName });
        }
    }

    private bool TryReadCurrentTime(out int day, out int hour, out int minute)
    {
        day = 1;
        hour = 0;
        minute = 0;

        FindTimeManagerIfNeeded();

        if (timeManager == null)
        {
            return false;
        }

        object source = timeManager;
        Type type = source.GetType();

        bool hasHour = TryReadInt(type, source, out hour, "CurrentHour", "Hour", "currentHour", "hour", "hours", "GetCurrentHour", "GetHour");
        bool hasMinute = TryReadInt(type, source, out minute, "CurrentMinute", "Minute", "currentMinute", "minute", "minutes", "GetCurrentMinute", "GetMinute");

        TryReadInt(type, source, out day, "CurrentDay", "Day", "currentDay", "day", "days", "GetCurrentDay", "GetDay");

        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);

        return hasHour && hasMinute;
    }

    private bool TryReadInt(Type type, object source, out int value, params string[] names)
    {
        value = 0;

        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];

            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null && IsNumericType(property.PropertyType))
            {
                value = Convert.ToInt32(property.GetValue(source));
                return true;
            }

            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null && IsNumericType(field.FieldType))
            {
                value = Convert.ToInt32(field.GetValue(source));
                return true;
            }

            MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

            if (method != null && IsNumericType(method.ReturnType))
            {
                value = Convert.ToInt32(method.Invoke(source, null));
                return true;
            }
        }

        return false;
    }

    private bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(long);
    }

    private void FindTimeManagerIfNeeded()
    {
        if (timeManager != null)
        {
            return;
        }

        Type timeManagerType = FindTypeByName("TimeManager");

        if (timeManagerType == null)
        {
            return;
        }

#pragma warning disable CS0618
        UnityEngine.Object found = FindObjectOfType(timeManagerType);
#pragma warning restore CS0618

        timeManager = found as MonoBehaviour;
    }

    private Type FindTypeByName(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private object GetStaticInstance(Type type, string propertyOrFieldName)
    {
        PropertyInfo property = type.GetProperty(propertyOrFieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (property != null)
        {
            return property.GetValue(null);
        }

        FieldInfo field = type.GetField(propertyOrFieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null)
        {
            return field.GetValue(null);
        }

        return null;
    }

    private int GetTotalMinutes(int hour, int minute)
    {
        return hour * 60 + minute;
    }

    private string GetEventKey(ScheduledTimeEvent timeEvent, int index)
    {
        if (!string.IsNullOrWhiteSpace(timeEvent.eventId))
        {
            return timeEvent.eventId;
        }

        return index.ToString();
    }

    private void CreateDefaultEventsIfEmpty()
    {
        if (scheduledEvents != null && scheduledEvents.Count > 0)
        {
            return;
        }

        scheduledEvents = new List<ScheduledTimeEvent>
        {
            new ScheduledTimeEvent
            {
                eventId = "start_day",
                kind = ScheduledTimeEventKind.StartDay,
                hour = 6,
                minute = 0,
                toastMessage = "Empieza un nuevo día.",
                soundName = "Success"
            },
            new ScheduledTimeEvent
            {
                eventId = "shops_open",
                kind = ScheduledTimeEventKind.ShopsOpen,
                hour = 8,
                minute = 0,
                toastMessage = "Las tiendas han abierto.",
                soundName = "Success"
            },
            new ScheduledTimeEvent
            {
                eventId = "npc_midday_move",
                kind = ScheduledTimeEventKind.NpcMiddayMove,
                hour = 12,
                minute = 0,
                toastMessage = "Los NPCs han cambiado de zona.",
                soundName = "Normal"
            },
            new ScheduledTimeEvent
            {
                eventId = "shops_close",
                kind = ScheduledTimeEventKind.ShopsClose,
                hour = 20,
                minute = 0,
                toastMessage = "Las tiendas han cerrado.",
                soundName = "Error"
            },
            new ScheduledTimeEvent
            {
                eventId = "night_warning",
                kind = ScheduledTimeEventKind.NightWarning,
                hour = 22,
                minute = 0,
                toastMessage = "Se está haciendo tarde...",
                soundName = "Error"
            },
            new ScheduledTimeEvent
            {
                eventId = "midnight_penalty",
                kind = ScheduledTimeEventKind.MidnightPenalty,
                hour = 0,
                minute = 0,
                toastMessage = "Es medianoche. Estás agotado.",
                soundName = "Error"
            }
        };
    }
}
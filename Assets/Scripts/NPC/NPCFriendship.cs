using UnityEngine;

public sealed class NPCFriendship : MonoBehaviour
{
    [Header("Identificacion")]
    [SerializeField] private string npcId = "npc_001";

    [Header("Corazones")]
    [SerializeField] [Range(0, 10)] private int currentHalfHearts = 0;
    [SerializeField] [Range(1, 10)] private int maxHalfHearts = 10;

    [Header("Subidas de amistad")]
    [SerializeField] private int talkGainHalfHearts = 1;
    [SerializeField] private int questCompletedGainHalfHearts = 2;
    [SerializeField] private int neutralGiftHalfHearts = 0;
    [SerializeField] private int likedGiftHalfHearts = 1;
    [SerializeField] private int lovedGiftHalfHearts = 2;
    [SerializeField] private int dislikedGiftHalfHearts = -1;
    [SerializeField] private int hatedGiftHalfHearts = -2;

    [Header("Restricciones")]
    [SerializeField] private bool gainOnlyOncePerDayWhenTalking = true;
    [SerializeField] private bool canGiftOncePerDay = true;

    [Header("Gustos")]
    [SerializeField] private ItemData[] likedItems;
    [SerializeField] private ItemData[] lovedItems;
    [SerializeField] private ItemData[] dislikedItems;
    [SerializeField] private ItemData[] hatedItems;

    private int lastTalkDay = -1;
    private int lastGiftDay = -1;

    public string NpcId => npcId;
    public int CurrentHalfHearts => currentHalfHearts;
    public float CurrentHearts => currentHalfHearts * 0.5f;

    public void RegisterTalk()
    {
        int currentDay = GetCurrentDay();

        if (gainOnlyOncePerDayWhenTalking && lastTalkDay == currentDay)
        {
            return;
        }

        AddHalfHearts(talkGainHalfHearts);
        lastTalkDay = currentDay;
    }

    public void RegisterQuestCompleted()
    {
        AddHalfHearts(questCompletedGainHalfHearts);
    }

    public bool TryGiftSelectedItem(PlayerInventory inventory, out string feedback)
    {
        feedback = string.Empty;

        if (inventory == null)
        {
            feedback = "No se encontró el inventario del jugador.";
            return false;
        }

        int currentDay = GetCurrentDay();

        if (canGiftOncePerDay && lastGiftDay == currentDay)
        {
            feedback = "Ya le has dado un regalo hoy.";
            return false;
        }

        InventorySlot slot = inventory.GetSelectedSlot();

        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            feedback = "Selecciona un objeto en la hotbar para regalar.";
            return false;
        }

        ItemData giftedItem = slot.Item;
        int friendshipDelta = GetGiftHalfHearts(giftedItem);

        bool removed = inventory.Hotbar.RemoveFromSlot(inventory.SelectedHotbarIndex, 1);

        if (!removed)
        {
            feedback = "No se pudo entregar el regalo.";
            return false;
        }

        AddHalfHearts(friendshipDelta);
        lastGiftDay = currentDay;
        feedback = BuildGiftFeedback(giftedItem, friendshipDelta);

        Debug.Log("NPCFriendship [" + npcId + "]: " + feedback + " | Corazones: " + CurrentHearts);
        return true;
    }

    public void AddHalfHearts(int amount)
    {
        currentHalfHearts = Mathf.Clamp(currentHalfHearts + amount, 0, Mathf.Max(1, maxHalfHearts));
    }

    public NPCFriendshipSaveData CaptureSaveData()
    {
        NPCFriendshipSaveData saveData = new NPCFriendshipSaveData();
        saveData.npcId = npcId;
        saveData.friendshipPoints = currentHalfHearts;
        saveData.lastTalkDay = lastTalkDay;
        saveData.lastGiftDay = lastGiftDay;
        return saveData;
    }

    public void LoadData(int savedPoints, int savedLastTalkDay, int savedLastGiftDay)
    {
        currentHalfHearts = Mathf.Clamp(savedPoints, 0, Mathf.Max(1, maxHalfHearts));
        lastTalkDay = savedLastTalkDay;
        lastGiftDay = savedLastGiftDay;
    }

    private int GetCurrentDay()
    {
        if (TimeManager.Instance != null)
        {
            return TimeManager.Instance.Day;
        }

        return 1;
    }

    private int GetGiftHalfHearts(ItemData item)
    {
        if (ContainsItem(lovedItems, item))
        {
            return lovedGiftHalfHearts;
        }

        if (ContainsItem(likedItems, item))
        {
            return likedGiftHalfHearts;
        }

        if (ContainsItem(hatedItems, item))
        {
            return hatedGiftHalfHearts;
        }

        if (ContainsItem(dislikedItems, item))
        {
            return dislikedGiftHalfHearts;
        }

        return neutralGiftHalfHearts;
    }

    private bool ContainsItem(ItemData[] items, ItemData target)
    {
        if (items == null || target == null)
        {
            return false;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildGiftFeedback(ItemData item, int delta)
    {
        string itemName = item != null ? item.DisplayName : "objeto";

        if (delta >= lovedGiftHalfHearts)
        {
            return "Le ha encantado " + itemName + ".";
        }

        if (delta >= likedGiftHalfHearts)
        {
            return "Le ha gustado " + itemName + ".";
        }

        if (delta <= hatedGiftHalfHearts)
        {
            return "Ha odiado " + itemName + ".";
        }

        if (delta <= dislikedGiftHalfHearts)
        {
            return "No le ha gustado " + itemName + ".";
        }

        return "Ha aceptado " + itemName + ".";
    }
}

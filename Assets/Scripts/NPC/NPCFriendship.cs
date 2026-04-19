using UnityEngine;

public sealed class NPCFriendship : MonoBehaviour
{
    [Header("Identificacion")]
    [SerializeField] private string npcId = "npc_001";

    [Header("Progreso")]
    [SerializeField] private int friendshipPoints = 0;
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private int pointsPerLevel = 100;

    [Header("Subidas de amistad")]
    [SerializeField] private int talkPointsPerDay = 5;
    [SerializeField] private int questCompletedPoints = 25;
    [SerializeField] private int neutralGiftPoints = 2;
    [SerializeField] private int likedGiftPoints = 10;
    [SerializeField] private int lovedGiftPoints = 20;
    [SerializeField] private int dislikedGiftPoints = -5;
    [SerializeField] private int hatedGiftPoints = -15;

    [Header("Restricciones")]
    [SerializeField] private bool canTalkOncePerDay = true;
    [SerializeField] private bool canGiftOncePerDay = true;

    [Header("Gustos")]
    [SerializeField] private ItemData[] likedItems;
    [SerializeField] private ItemData[] lovedItems;
    [SerializeField] private ItemData[] dislikedItems;
    [SerializeField] private ItemData[] hatedItems;

    private int lastTalkDay = -1;
    private int lastGiftDay = -1;

    public string NpcId => npcId;
    public int FriendshipPoints => friendshipPoints;
    public int Level
    {
        get
        {
            int safePointsPerLevel = Mathf.Max(1, pointsPerLevel);
            return Mathf.Clamp(friendshipPoints / safePointsPerLevel, 0, maxLevel);
        }
    }

    public void RegisterTalk()
    {
        int currentDay = GetCurrentDay();

        if (canTalkOncePerDay && lastTalkDay == currentDay)
        {
            return;
        }

        AddFriendship(talkPointsPerDay);
        lastTalkDay = currentDay;
    }

    public void RegisterQuestCompleted()
    {
        AddFriendship(questCompletedPoints);
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
        int friendshipDelta = GetGiftPoints(giftedItem);

        bool removed = inventory.Hotbar.RemoveFromSlot(inventory.SelectedHotbarIndex, 1);

        if (!removed)
        {
            feedback = "No se pudo entregar el regalo.";
            return false;
        }

        AddFriendship(friendshipDelta);
        lastGiftDay = currentDay;
        feedback = BuildGiftFeedback(giftedItem, friendshipDelta);

        Debug.Log("NPCFriendship [" + npcId + "]: " + feedback + " | Puntos: " + friendshipPoints);
        return true;
    }

    public void AddFriendship(int amount)
    {
        int maxPoints = Mathf.Max(1, maxLevel) * Mathf.Max(1, pointsPerLevel);
        friendshipPoints = Mathf.Clamp(friendshipPoints + amount, 0, maxPoints);
    }

    public float GetProgressToNextLevel01()
    {
        int safePointsPerLevel = Mathf.Max(1, pointsPerLevel);
        int currentLevelPoints = friendshipPoints % safePointsPerLevel;
        return currentLevelPoints / (float)safePointsPerLevel;
    }

    public NPCFriendshipSaveData CaptureSaveData()
    {
        NPCFriendshipSaveData saveData = new NPCFriendshipSaveData();
        saveData.npcId = npcId;
        saveData.friendshipPoints = friendshipPoints;
        saveData.lastTalkDay = lastTalkDay;
        saveData.lastGiftDay = lastGiftDay;
        return saveData;
    }

    public void LoadData(int savedPoints, int savedLastTalkDay, int savedLastGiftDay)
    {
        int maxPoints = Mathf.Max(1, maxLevel) * Mathf.Max(1, pointsPerLevel);
        friendshipPoints = Mathf.Clamp(savedPoints, 0, maxPoints);
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

    private int GetGiftPoints(ItemData item)
    {
        if (ContainsItem(lovedItems, item))
        {
            return lovedGiftPoints;
        }

        if (ContainsItem(likedItems, item))
        {
            return likedGiftPoints;
        }

        if (ContainsItem(hatedItems, item))
        {
            return hatedGiftPoints;
        }

        if (ContainsItem(dislikedItems, item))
        {
            return dislikedGiftPoints;
        }

        return neutralGiftPoints;
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

        if (delta >= lovedGiftPoints)
        {
            return "Le ha encantado " + itemName + ".";
        }

        if (delta >= likedGiftPoints)
        {
            return "Le ha gustado " + itemName + ".";
        }

        if (delta <= hatedGiftPoints)
        {
            return "Ha odiado " + itemName + ".";
        }

        if (delta <= dislikedGiftPoints)
        {
            return "No le ha gustado " + itemName + ".";
        }

        return "Ha aceptado " + itemName + ".";
    }
}